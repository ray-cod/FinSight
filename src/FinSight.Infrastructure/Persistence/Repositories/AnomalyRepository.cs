using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Anomalies;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements anomaly persistence with Entity Framework Core.
/// </summary>
public sealed class AnomalyRepository(
    FinSightDbContext dbContext)
    : IAnomalyRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Anomaly>>
        GetByUserIdAsync(
            Guid userId,
            bool includeResolved = false,
            int limit = 100,
            CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 500);

        var query =
            dbContext.Set<Anomaly>()
                .AsNoTracking()
                .Where(
                    x =>
                        x.UserId == userId);

        if (!includeResolved)
        {
            query =
                query.Where(
                    x =>
                        x.Status ==
                        AnomalyStatus.Open);
        }

        return await query
            .OrderByDescending(
                x => x.DetectedAt)
            .Take(limit)
            .ToListAsync(
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<Anomaly?> GetByIdAsync(
        Guid userId,
        Guid anomalyId,
        CancellationToken cancellationToken = default)
    {
        return dbContext
            .Set<Anomaly>()
            .SingleOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.Id == anomalyId,
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsForTransactionAsync(
        Guid transactionId,
        AnomalyType type,
        CancellationToken cancellationToken = default)
    {
        return dbContext
            .Set<Anomaly>()
            .AnyAsync(
                x =>
                    x.TransactionId == transactionId &&
                    x.Type == type,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Anomaly>>
        GetOpenBeforeAsync(
            DateTimeOffset asOf,
            CancellationToken cancellationToken = default)
    {
        var cutoff =
            asOf.AddDays(-90);

        return await dbContext
            .Set<Anomaly>()
            .Where(
                x =>
                    x.Status ==
                    AnomalyStatus.Open &&
                    x.DetectedAt < cutoff)
            .ToListAsync(
                cancellationToken);
    }

    /// <inheritdoc />
    public void Add(
        Anomaly anomaly)
    {
        dbContext.Set<Anomaly>()
            .Add(anomaly);
    }
}
