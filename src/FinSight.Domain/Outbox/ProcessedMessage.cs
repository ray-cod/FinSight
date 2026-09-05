using FinSight.Domain.Common;

namespace FinSight.Domain.Outbox;

/// <summary>
/// Records an integration event that has already been processed.
/// </summary>
public sealed class ProcessedMessage
    : Entity<Guid>
{
    private ProcessedMessage()
    {
    }

    private ProcessedMessage(
        Guid id,
        string messageId,
        string consumerName)
        : base(id)
    {
        MessageId = messageId;
        ConsumerName = consumerName;
        ProcessedAt =
            DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the RabbitMQ message identifier.
    /// </summary>
    public string MessageId { get; private set; } = null!;

    /// <summary>
    /// Gets the consumer name.
    /// </summary>
    public string ConsumerName { get; private set; } = null!;

    /// <summary>
    /// Gets when processing completed.
    /// </summary>
    public DateTimeOffset ProcessedAt { get; private set; }

    /// <summary>
    /// Creates a processed-message record.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="consumerName">The consumer identifier.</param>
    /// <returns>A new processed message.</returns>
    public static ProcessedMessage Create(
        string messageId,
        string consumerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            messageId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            consumerName);

        return new ProcessedMessage(
            Guid.NewGuid(),
            messageId.Trim(),
            consumerName.Trim());
    }
}
