using FinSight.Application.Abstractions.Outbox;
using FinSight.Domain.Outbox;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Persists integration message processing records.
/// </summary>
public sealed class ProcessedMessageStore(
    FinSightDbContext dbContext)
    : IProcessedMessageStore
{
    /// <inheritdoc />
    public Task<bool> ExistsAsync(
        string messageId,
        string consumerName,
        CancellationToken cancellationToken = default)
    {
        return dbContext
            .Set<ProcessedMessage>()
            .AnyAsync(
                x =>
                    x.MessageId ==
                    messageId &&
                    x.ConsumerName ==
                    consumerName,
                cancellationToken);
    }

    /// <inheritdoc />
    public void Add(
        string messageId,
        string consumerName)
    {
        dbContext
            .Set<ProcessedMessage>()
            .Add(
                ProcessedMessage.Create(
                    messageId,
                    consumerName));
    }
}
