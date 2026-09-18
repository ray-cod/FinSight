using FinSight.Domain.Transactions;

namespace FinSight.Application.Features.Transactions;

/// <summary>
/// Defines the supported transaction list filters and ordering rules.
/// </summary>
public sealed record TransactionQuery
{
    /// <summary>Gets the one-based page number.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Gets the requested page size.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Gets the free-text search expression.</summary>
    public string? Search { get; init; }

    /// <summary>Gets the optional financial account filter.</summary>
    public Guid? AccountId { get; init; }

    /// <summary>Gets the optional category filter.</summary>
    public Guid? CategoryId { get; init; }

    /// <summary>Gets the optional transaction type filter.</summary>
    public TransactionType? Type { get; init; }

    /// <summary>Gets the optional processing status filter.</summary>
    public TransactionStatus? Status { get; init; }

    /// <summary>Gets the optional classification status filter.</summary>
    public ClassificationStatus? ClassificationStatus { get; init; }

    /// <summary>Gets the inclusive start date.</summary>
    public DateTimeOffset? From { get; init; }

    /// <summary>Gets the exclusive end date.</summary>
    public DateTimeOffset? To { get; init; }

    /// <summary>Gets the optional minimum absolute transaction amount.</summary>
    public decimal? MinAmount { get; init; }

    /// <summary>Gets the optional maximum absolute transaction amount.</summary>
    public decimal? MaxAmount { get; init; }

    /// <summary>Gets the supported sort field.</summary>
    public TransactionSortField SortBy { get; init; } = TransactionSortField.Date;

    /// <summary>Gets whether results should be sorted descending.</summary>
    public bool Descending { get; init; } = true;
}

/// <summary>
/// Identifies a supported transaction sort field.
/// </summary>
public enum TransactionSortField
{
    /// <summary>Sort by transaction date.</summary>
    Date,

    /// <summary>Sort by transaction amount magnitude.</summary>
    Amount,

    /// <summary>Sort by display merchant/description.</summary>
    Merchant,
}

/// <summary>
/// Represents one row in the transaction list read model.
/// </summary>
public sealed record TransactionListItemResponse(
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
    string Type,
    string Status,
    string ClassificationStatus,
    string ClassificationSource,
    decimal? ClassificationConfidence,
    DateTimeOffset? ClassifiedAt);

/// <summary>
/// Represents the full transaction details read model.
/// </summary>
public sealed record TransactionDetailResponse(
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
    string Type,
    string Status,
    string ClassificationStatus,
    string ClassificationSource,
    decimal? ClassificationConfidence,
    DateTimeOffset? ClassifiedAt,
    DateTimeOffset? UserCorrectedAt);

/// <summary>
/// Represents one page of transaction results.
/// </summary>
public sealed record TransactionQueryResult(
    IReadOnlyList<TransactionListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    /// <summary>Gets whether a subsequent page exists.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Gets whether a previous page exists.</summary>
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Represents a validated HTTP transaction query.
/// </summary>
public sealed record TransactionListRequest
{
    /// <summary>Gets the one-based page number.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Gets the requested page size.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Gets the free-text search expression.</summary>
    public string? Search { get; init; }

    /// <summary>Gets the optional financial account filter.</summary>
    public Guid? AccountId { get; init; }

    /// <summary>Gets the optional category filter.</summary>
    public Guid? CategoryId { get; init; }

    /// <summary>Gets the optional transaction type filter.</summary>
    public TransactionType? Type { get; init; }

    /// <summary>Gets the optional processing status filter.</summary>
    public TransactionStatus? Status { get; init; }

    /// <summary>Gets the optional classification status filter.</summary>
    public ClassificationStatus? ClassificationStatus { get; init; }

    /// <summary>Gets the inclusive start date.</summary>
    public DateTimeOffset? From { get; init; }

    /// <summary>Gets the exclusive end date.</summary>
    public DateTimeOffset? To { get; init; }

    /// <summary>Gets the optional minimum absolute transaction amount.</summary>
    public decimal? MinAmount { get; init; }

    /// <summary>Gets the optional maximum absolute transaction amount.</summary>
    public decimal? MaxAmount { get; init; }

    /// <summary>Gets the supported sort field.</summary>
    public TransactionSortField SortBy { get; init; } = TransactionSortField.Date;

    /// <summary>Gets whether results should be sorted descending.</summary>
    public bool Descending { get; init; } = true;

    /// <summary>Converts the request into the application query after applying safe bounds.</summary>
    /// <returns>The normalized transaction query.</returns>
    public TransactionQuery ToQuery()
    {
        return new TransactionQuery
        {
            Page = Math.Max(1, Page),
            PageSize = Math.Clamp(PageSize, 10, 100),
            Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
            AccountId = AccountId,
            CategoryId = CategoryId,
            Type = Type,
            Status = Status,
            ClassificationStatus = ClassificationStatus,
            From = From,
            To = To,
            MinAmount = MinAmount,
            MaxAmount = MaxAmount,
            SortBy = SortBy,
            Descending = Descending,
        };
    }
}
