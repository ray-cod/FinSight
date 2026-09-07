using FinSight.Domain.Common;

namespace FinSight.Domain.Outbox;

/// <summary>
/// Represents an integration event waiting to be published externally.
/// </summary>
public sealed class OutboxMessage : Entity<Guid>
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(
        Guid id,
        string eventType,
        string payload,
        DateTimeOffset occurredAt)
        : base(id)
    {
        EventType = eventType;
        Payload = payload;
        OccurredAt = occurredAt;
        Status = OutboxMessageStatus.Pending;
        AttemptCount = 0;
    }

    /// <summary>
    /// Gets the CLR/event contract type name.
    /// </summary>
    public string EventType { get; private set; } = null!;

    /// <summary>
    /// Gets the serialized event payload.
    /// </summary>
    public string Payload { get; private set; } = null!;

    /// <summary>
    /// Gets the event routing key.
    /// </summary>
    public string RoutingKey { get; private set; } = null!;

    /// <summary>
    /// Gets when the event was created.
    /// </summary>
    public DateTimeOffset OccurredAt { get; private set; }

    /// <summary>
    /// Gets when the event was last attempted.
    /// </summary>
    public DateTimeOffset? LastAttemptedAt { get; private set; }

    /// <summary>
    /// Gets the number of publication attempts.
    /// </summary>
    public int AttemptCount { get; private set; }

    /// <summary>
    /// Gets the publication status.
    /// </summary>
    public OutboxMessageStatus Status { get; private set; }

    /// <summary>
    /// Gets the last publication error.
    /// </summary>
    public string? LastError { get; private set; }

    /// <summary>
    /// Gets when the message was successfully published.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; private set; }

    /// <summary>
    /// Gets when the message may next be retried.
    /// </summary>
    public DateTimeOffset? NextAttemptAt { get; private set; }

    /// <summary>
    /// Creates a pending outbox message.
    /// </summary>
    /// <param name="eventType">The event type.</param>
    /// <param name="payload">The serialized payload.</param>
    /// <param name="routingKey">The RabbitMQ routing key.</param>
    /// <param name="occurredAt">The event creation timestamp.</param>
    /// <returns>A new outbox message.</returns>
    public static OutboxMessage Create(
        string eventType,
        string payload,
        string routingKey,
        DateTimeOffset occurredAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        ArgumentException.ThrowIfNullOrWhiteSpace(routingKey);

        return new OutboxMessage(
            Guid.NewGuid(),
            eventType.Trim(),
            payload,
            occurredAt)
        {
            RoutingKey = routingKey.Trim()
        };
    }

    /// <summary>
    /// Marks the message as successfully published.
    /// </summary>
    public void MarkPublished()
    {
        Status = OutboxMessageStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
        NextAttemptAt = null;
        LastError = null;
    }

    /// <summary>
    /// Records a failed publication attempt.
    /// </summary>
    /// <param name="error">The publication error.</param>
    /// <param name="nextAttemptAt">The next retry timestamp.</param>
    public void RecordFailure(
        string error,
        DateTimeOffset nextAttemptAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        AttemptCount++;
        LastAttemptedAt = DateTimeOffset.UtcNow;
        LastError = error.Length > 2000
            ? error[..2000]
            : error;
        NextAttemptAt = nextAttemptAt;

        Status =
            OutboxMessageStatus.Pending;
    }

    /// <summary>
    /// Marks the message as permanently failed.
    /// </summary>
    /// <param name="error">The terminal failure reason.</param>
    public void MarkDeadLettered(
        string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        AttemptCount++;
        LastAttemptedAt =
            DateTimeOffset.UtcNow;

        LastError =
            error.Length > 2000
                ? error[..2000]
                : error;

        Status =
            OutboxMessageStatus.DeadLettered;

        NextAttemptAt = null;
    }
}

/// <summary>
/// Represents the lifecycle status of an outbox message.
/// </summary>
public enum OutboxMessageStatus
{
    /// <summary>
    /// The message is waiting to be published.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// The message has been published successfully.
    /// </summary>
    Published = 2,

    /// <summary>
    /// The message has permanently failed.
    /// </summary>
    DeadLettered = 3
}
