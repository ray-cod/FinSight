using FinSight.Application.Abstractions.Outbox;
using FinSight.Domain.Outbox;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements transactional outbox persistence.
/// </summary>
public sealed class OutboxRepository(
    FinSightDbContext dbContext)
    : IOutboxRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<OutboxMessage>>
        GetPendingAsync(
            DateTimeOffset now,
            int batchSize,
            CancellationToken cancellationToken = default)
    {
        batchSize =
            Math.Clamp(
                batchSize,
                1,
                500);

        return await dbContext
            .Set<OutboxMessage>()
            .Where(
                x =>
                    x.Status ==
                    OutboxMessageStatus.Pending &&
                    (
                        x.NextAttemptAt == null ||
                        x.NextAttemptAt <= now))
            .OrderBy(
                x => x.OccurredAt)
            .Take(batchSize)
            .ToListAsync(
                cancellationToken);
    }

    /// <inheritdoc />
    public void Add(
        OutboxMessage message)
    {
        dbContext.Set<OutboxMessage>()
            .Add(message);
    }
}
