namespace FinSight.Contracts.Events;

/// <summary>
/// Represents a request to deliver a notification.
/// </summary>
public sealed record NotificationCreatedEvent
{
    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    /// Gets the notification identifier.
    /// </summary>
    public required Guid NotificationId { get; init; }

    /// <summary>
    /// Gets the owning user identifier.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the notification channel.
    /// </summary>
    public required string Channel { get; init; }

    /// <summary>
    /// Gets the destination address.
    /// </summary>
    public required string Recipient { get; init; }

    /// <summary>
    /// Gets when the event occurred.
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
