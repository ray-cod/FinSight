using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Insights;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements financial insight persistence.
/// </summary>
public sealed class InsightRepository(
    FinSightDbContext dbContext)
    : IInsightRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<FinancialInsight>>
        GetByUserIdAsync(
            Guid userId,
            bool includeDismissed = false,
            int limit = 100,
            CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 500);

        var query =
            dbContext
                .Set<FinancialInsight>()
                .AsNoTracking()
                .Where(
                    x =>
                        x.UserId == userId);

        if (!includeDismissed)
        {
            query =
                query.Where(
                    x =>
                        x.Status !=
                        InsightStatus.Dismissed &&
                        x.Status !=
                        InsightStatus.Expired);
        }

        return await query
            .OrderByDescending(
                x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<FinancialInsight?>
        GetByIdAsync(
            Guid userId,
            Guid insightId,
            CancellationToken cancellationToken = default)
    {
        return dbContext
            .Set<FinancialInsight>()
            .SingleOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.Id == insightId,
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsForAnomalyAsync(
        Guid anomalyId,
        CancellationToken cancellationToken = default)
    {
        return dbContext
            .Set<FinancialInsight>()
            .AnyAsync(
                x =>
                    x.AnomalyId == anomalyId,
                cancellationToken);
    }

    /// <inheritdoc />
    public void Add(
        FinancialInsight insight)
    {
        dbContext
            .Set<FinancialInsight>()
            .Add(insight);
    }

    /// <inheritdoc />
    public async Task<
        IReadOnlyList<FinancialInsight>>
        GetExpiredAsync(
            DateTimeOffset asOf,
            CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<FinancialInsight>()
            .Where(
                x =>
                    x.ExpiresAt.HasValue &&
                    x.ExpiresAt <= asOf &&
                    x.Status !=
                    InsightStatus.Dismissed &&
                    x.Status !=
                    InsightStatus.Expired)
            .ToListAsync(
                cancellationToken);
    }
}
