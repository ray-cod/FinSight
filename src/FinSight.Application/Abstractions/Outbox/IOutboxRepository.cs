using FinSight.Domain.Outbox;

namespace FinSight.Application.Abstractions.Outbox;

/// <summary>
/// Provides persistence operations for transactional outbox messages.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Gets pending messages ready for publication.
    /// </summary>
    /// <param name="now">Current time.</param>
    /// <param name="batchSize">Maximum messages.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Messages ready for publication.</returns>
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an outbox message.
    /// </summary>
    /// <param name="message">The message.</param>
    void Add(OutboxMessage message);
}
