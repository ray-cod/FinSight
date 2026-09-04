using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements subscription persistence using Entity Framework Core.
/// </summary>
public sealed class SubscriptionRepository(
    FinSightDbContext dbContext)
    : ISubscriptionRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Subscription>>
        GetByUserIdAsync(
            Guid userId,
            bool includeDismissed = false,
            CancellationToken cancellationToken = default)
    {
        var query =
            dbContext.Set<Subscription>()
                .AsNoTracking()
                .Where(
                    x => x.UserId == userId);

        if (!includeDismissed)
        {
            query =
                query.Where(
                    x =>
                        x.Status !=
                        SubscriptionStatus.Dismissed);
        }

        return await query
            .OrderBy(
                x => x.Status)
            .ThenBy(
                x => x.MerchantName)
            .ToListAsync(
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<Subscription?> GetByIdAsync(
        Guid userId,
        Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Subscription>()
            .SingleOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.Id == subscriptionId,
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<Subscription?>
        GetByMerchantAsync(
            Guid userId,
            Guid merchantId,
            string currency,
            CancellationToken cancellationToken = default)
    {
        var normalizedCurrency =
            currency.Trim().ToUpperInvariant();

        return dbContext.Set<Subscription>()
            .SingleOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.MerchantId == merchantId &&
                    x.Currency ==
                    normalizedCurrency,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Subscription>>
        GetOverdueAsync(
            DateTimeOffset asOf,
            CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Subscription>()
            .Where(
                x =>
                    x.Status ==
                    SubscriptionStatus.Active &&
                    x.NextExpectedChargeAt.HasValue &&
                    x.NextExpectedChargeAt.Value < asOf)
            .ToListAsync(
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> HasPriceObservationAsync(
        Guid subscriptionId,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        return dbContext
            .Set<SubscriptionPriceHistory>()
            .AnyAsync(
                x =>
                    x.SubscriptionId ==
                    subscriptionId &&
                    x.TransactionId ==
                    transactionId,
                cancellationToken);
    }

    /// <inheritdoc />
    public void Add(
        Subscription subscription)
    {
        dbContext.Set<Subscription>()
            .Add(subscription);
    }

    /// <inheritdoc />
    public void AddPriceHistory(
        SubscriptionPriceHistory history)
    {
        dbContext.Set<SubscriptionPriceHistory>()
            .Add(history);
    }

    /// <inheritdoc />
    public async Task<
        IReadOnlyList<SubscriptionPriceHistory>>
        GetPriceHistoryAsync(
            Guid subscriptionId,
            int limit = 24,
            CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 100);

        return await dbContext
            .Set<SubscriptionPriceHistory>()
            .AsNoTracking()
            .Where(
                x =>
                    x.SubscriptionId ==
                    subscriptionId)
            .OrderByDescending(
                x => x.ObservedAt)
            .Take(limit)
            .ToListAsync(
                cancellationToken);
    }
}
