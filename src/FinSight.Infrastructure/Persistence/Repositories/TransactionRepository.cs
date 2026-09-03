using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements transaction persistence with Entity Framework Core.
/// </summary>
public sealed class TransactionRepository(
    FinSightDbContext dbContext)
    : ITransactionRepository
{
    /// <inheritdoc />
    public Task<bool> ExistsAsync(
        Guid accountId,
        string providerTransactionId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Transaction>()
            .AnyAsync(
                x =>
                    x.AccountId == accountId &&
                    x.ProviderTransactionId ==
                    providerTransactionId,
                cancellationToken);
    }

    /// <inheritdoc />
    public void Add(
        Transaction transaction)
    {
        dbContext.Set<Transaction>()
            .Add(transaction);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Transaction>> GetForAccountAsync(
        Guid userId,
        Guid accountId,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Transaction>()
            .AsNoTracking()
            .Where(
                x =>
                    x.UserId == userId &&
                    x.AccountId == accountId)
            .OrderByDescending(x => x.TransactionDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Transaction?> GetByIdAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Transaction>()
            .SingleOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.Id.Value == transactionId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Transaction>>
        GetByMerchantAsync(
            Guid userId,
            Guid merchantId,
            string currency,
            int limit = 36,
            CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 2, 100);

        return await dbContext.Set<Transaction>()
            .AsNoTracking()
            .Where(
                x =>
                    x.UserId == userId &&
                    x.MerchantId == merchantId &&
                    x.Currency == currency &&
                    x.Type == TransactionType.Purchase &&
                    x.Amount < 0)
            .OrderByDescending(
                x => x.TransactionDate)
            .Take(limit)
            .ToListAsync(
                cancellationToken);
    }
}
