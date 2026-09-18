using FinSight.Application.Abstractions.Persistence;
using FinSight.Application.Features.Transactions;
using FinSight.Domain.Accounts;
using FinSight.Domain.Categories;
using FinSight.Domain.Merchants;
using FinSight.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements optimized, projection-based transaction queries.
/// </summary>
public sealed class TransactionQueryRepository(
    FinSightDbContext dbContext)
    : ITransactionQueryRepository
{
    /// <inheritdoc />
    public async Task<TransactionQueryResult> GetPageAsync(
        Guid userId,
        TransactionQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 10, 100);

        var filtered = BuildQuery(userId, query);
        var totalCount = await filtered.CountAsync(cancellationToken);
        var totalPages = totalCount == 0
            ? 1
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        page = Math.Min(page, totalPages);

        var ordered = ApplyOrdering(filtered, query);

        var projections = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = projections
            .Select(MapListItem)
            .ToArray();

        return new TransactionQueryResult(
            items,
            page,
            pageSize,
            totalCount,
            totalPages);
    }

    /// <inheritdoc />
    public async Task<TransactionDetailResponse?> GetDetailAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var projection = await BuildDetailQuery(userId, transactionId)
            .SingleOrDefaultAsync(cancellationToken);

        return projection is null
            ? null
            : MapDetail(projection);
    }

    private IQueryable<TransactionListProjection> BuildQuery(
        Guid userId,
        TransactionQuery query)
    {
        var transactions =
            from transaction in dbContext.Set<Transaction>().AsNoTracking()
            join account in dbContext.Set<FinancialAccount>().AsNoTracking()
                on transaction.AccountId equals account.Id.Value
            join institution in dbContext.Set<Institution>().AsNoTracking()
                on transaction.InstitutionId equals institution.Id
            join merchant in dbContext.Set<Merchant>().AsNoTracking()
                on transaction.MerchantId equals merchant.Id into merchantGroup
            from merchant in merchantGroup.DefaultIfEmpty()
            join category in dbContext.Set<Category>().AsNoTracking()
                on transaction.CategoryId equals category.Id into categoryGroup
            from category in categoryGroup.DefaultIfEmpty()
            join subcategory in dbContext.Set<Subcategory>().AsNoTracking()
                on transaction.SubcategoryId equals subcategory.Id into subcategoryGroup
            from subcategory in subcategoryGroup.DefaultIfEmpty()
            where transaction.UserId == userId &&
                  account.UserId == userId
            select new TransactionListProjection(
                transaction.Id.Value,
                account.Id.Value,
                account.Name,
                institution.Name,
                merchant == null
                    ? transaction.NormalizedDescription
                    : merchant.CanonicalName,
                transaction.RawDescription,
                transaction.NormalizedDescription,
                transaction.CategoryId,
                category == null ? null : category.Name,
                transaction.SubcategoryId,
                subcategory == null ? null : subcategory.Name,
                transaction.Amount,
                transaction.Currency,
                transaction.TransactionDate,
                transaction.Type,
                transaction.Status,
                transaction.ClassificationStatus,
                transaction.ClassificationSource,
                transaction.ClassificationConfidence,
                transaction.ClassifiedAt);

        if (query.AccountId.HasValue)
        {
            transactions = transactions.Where(
                x => x.AccountId == query.AccountId.Value);
        }

        if (query.CategoryId.HasValue)
        {
            transactions = transactions.Where(
                x => x.CategoryId == query.CategoryId.Value);
        }

        if (query.Type.HasValue)
        {
            transactions = transactions.Where(
                x => x.Type == query.Type.Value);
        }

        if (query.Status.HasValue)
        {
            transactions = transactions.Where(
                x => x.Status == query.Status.Value);
        }

        if (query.ClassificationStatus.HasValue)
        {
            transactions = transactions.Where(
                x => x.ClassificationStatus == query.ClassificationStatus.Value);
        }

        if (query.From.HasValue)
        {
            transactions = transactions.Where(
                x => x.TransactionDate >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            transactions = transactions.Where(
                x => x.TransactionDate < query.To.Value);
        }

        if (query.MinAmount.HasValue)
        {
            var minimum = Math.Abs(query.MinAmount.Value);
            transactions = transactions.Where(
                x => Math.Abs(x.Amount) >= minimum);
        }

        if (query.MaxAmount.HasValue)
        {
            var maximum = Math.Abs(query.MaxAmount.Value);
            transactions = transactions.Where(
                x => Math.Abs(x.Amount) <= maximum);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            var pattern = $"%{search}%";
            transactions = transactions.Where(
                x =>
                    EF.Functions.ILike(x.MerchantName, pattern) ||
                    EF.Functions.ILike(x.NormalizedDescription, pattern) ||
                    EF.Functions.ILike(x.RawDescription, pattern) ||
                    EF.Functions.ILike(x.AccountName, pattern) ||
                    EF.Functions.ILike(x.InstitutionName, pattern) ||
                    (x.CategoryName != null && EF.Functions.ILike(x.CategoryName, pattern)) ||
                    (x.SubcategoryName != null && EF.Functions.ILike(x.SubcategoryName, pattern)));
        }

        return transactions;
    }

    private static IOrderedQueryable<TransactionListProjection> ApplyOrdering(
        IQueryable<TransactionListProjection> query,
        TransactionQuery request)
    {
        return request.SortBy switch
        {
            TransactionSortField.Amount => request.Descending
                ? query.OrderByDescending(x => Math.Abs(x.Amount))
                    .ThenByDescending(x => x.TransactionDate)
                : query.OrderBy(x => Math.Abs(x.Amount))
                    .ThenByDescending(x => x.TransactionDate),
            TransactionSortField.Merchant => request.Descending
                ? query.OrderByDescending(x => x.MerchantName)
                    .ThenByDescending(x => x.TransactionDate)
                : query.OrderBy(x => x.MerchantName)
                    .ThenByDescending(x => x.TransactionDate),
            _ => request.Descending
                ? query.OrderByDescending(x => x.TransactionDate)
                    .ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.TransactionDate)
                    .ThenBy(x => x.Id),
        };
    }

    private static TransactionListItemResponse MapListItem(
        TransactionListProjection projection)
    {
        return new TransactionListItemResponse(
            projection.Id,
            projection.AccountId,
            projection.AccountName,
            projection.InstitutionName,
            projection.MerchantName,
            projection.RawDescription,
            projection.NormalizedDescription,
            projection.CategoryId,
            projection.CategoryName,
            projection.SubcategoryId,
            projection.SubcategoryName,
            projection.Amount,
            projection.Currency,
            projection.TransactionDate,
            projection.Type.ToString(),
            projection.Status.ToString(),
            projection.ClassificationStatus.ToString(),
            projection.ClassificationSource.ToString(),
            projection.ClassificationConfidence,
            projection.ClassifiedAt);
    }

    private static TransactionDetailResponse MapDetail(
        TransactionDetailProjection projection)
    {
        return new TransactionDetailResponse(
            projection.Id,
            projection.AccountId,
            projection.AccountName,
            projection.InstitutionId,
            projection.InstitutionName,
            projection.ProviderTransactionId,
            projection.RawDescription,
            projection.NormalizedDescription,
            projection.MerchantId,
            projection.MerchantName,
            projection.CategoryId,
            projection.CategoryName,
            projection.SubcategoryId,
            projection.SubcategoryName,
            projection.Amount,
            projection.Currency,
            projection.TransactionDate,
            projection.ImportedAt,
            projection.Type.ToString(),
            projection.Status.ToString(),
            projection.ClassificationStatus.ToString(),
            projection.ClassificationSource.ToString(),
            projection.ClassificationConfidence,
            projection.ClassifiedAt,
            projection.UserCorrectedAt);
    }

    private IQueryable<TransactionDetailProjection> BuildDetailQuery(
        Guid userId,
        Guid transactionId)
    {
        return
            from transaction in dbContext.Set<Transaction>().AsNoTracking()
            join account in dbContext.Set<FinancialAccount>().AsNoTracking()
                on transaction.AccountId equals account.Id.Value
            join institution in dbContext.Set<Institution>().AsNoTracking()
                on transaction.InstitutionId equals institution.Id
            join merchant in dbContext.Set<Merchant>().AsNoTracking()
                on transaction.MerchantId equals merchant.Id into merchantGroup
            from merchant in merchantGroup.DefaultIfEmpty()
            join category in dbContext.Set<Category>().AsNoTracking()
                on transaction.CategoryId equals category.Id into categoryGroup
            from category in categoryGroup.DefaultIfEmpty()
            join subcategory in dbContext.Set<Subcategory>().AsNoTracking()
                on transaction.SubcategoryId equals subcategory.Id into subcategoryGroup
            from subcategory in subcategoryGroup.DefaultIfEmpty()
            where transaction.UserId == userId &&
                  account.UserId == userId &&
                  transaction.Id.Value == transactionId
            select new TransactionDetailProjection(
                transaction.Id.Value,
                account.Id.Value,
                account.Name,
                transaction.InstitutionId,
                institution.Name,
                transaction.ProviderTransactionId,
                transaction.RawDescription,
                transaction.NormalizedDescription,
                transaction.MerchantId,
                merchant == null ? null : merchant.CanonicalName,
                transaction.CategoryId,
                category == null ? null : category.Name,
                transaction.SubcategoryId,
                subcategory == null ? null : subcategory.Name,
                transaction.Amount,
                transaction.Currency,
                transaction.TransactionDate,
                transaction.ImportedAt,
                transaction.Type,
                transaction.Status,
                transaction.ClassificationStatus,
                transaction.ClassificationSource,
                transaction.ClassificationConfidence,
                transaction.ClassifiedAt,
                transaction.UserCorrectedAt);
    }

    private sealed record TransactionListProjection(
        Guid Id,
        Guid AccountId,
        string AccountName,
        string InstitutionName,
        string MerchantName,
        string RawDescription,
        string NormalizedDescription,
        Guid? CategoryId,
        string? CategoryName,
        Guid? SubcategoryId,
        string? SubcategoryName,
        decimal Amount,
        string Currency,
        DateTimeOffset TransactionDate,
        TransactionType Type,
        TransactionStatus Status,
        ClassificationStatus ClassificationStatus,
        ClassificationSource ClassificationSource,
        decimal? ClassificationConfidence,
        DateTimeOffset? ClassifiedAt);

    private sealed record TransactionDetailProjection(
        Guid Id,
        Guid AccountId,
        string AccountName,
        Guid InstitutionId,
        string InstitutionName,
        string ProviderTransactionId,
        string RawDescription,
        string NormalizedDescription,
        Guid? MerchantId,
        string? MerchantName,
        Guid? CategoryId,
        string? CategoryName,
        Guid? SubcategoryId,
        string? SubcategoryName,
        decimal Amount,
        string Currency,
        DateTimeOffset TransactionDate,
        DateTimeOffset ImportedAt,
        TransactionType Type,
        TransactionStatus Status,
        ClassificationStatus ClassificationStatus,
        ClassificationSource ClassificationSource,
        decimal? ClassificationConfidence,
        DateTimeOffset? ClassifiedAt,
        DateTimeOffset? UserCorrectedAt);

}
