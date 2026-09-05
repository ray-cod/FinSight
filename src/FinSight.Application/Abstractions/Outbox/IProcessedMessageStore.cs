using FinSight.Domain.Outbox;

namespace FinSight.Application.Abstractions.Outbox;

/// <summary>
/// Tracks integration messages that have already been processed.
/// </summary>
public interface IProcessedMessageStore
{
    /// <summary>
    /// Determines whether a message has already been processed.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="consumerName">The consumer identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when processed.</returns>
    Task<bool> ExistsAsync(
        string messageId,
        string consumerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as processed.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="consumerName">The consumer identifier.</param>
    void Add(
        string messageId,
        string consumerName);
}
