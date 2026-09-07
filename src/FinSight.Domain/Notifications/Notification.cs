using FinSight.Domain.Common;

namespace FinSight.Domain.Notifications;

/// <summary>
/// Represents a notification addressed to a FinSight user.
/// </summary>
public sealed class Notification
    : AggregateRoot<Guid>
{
    private Notification()
    {
    }

    private Notification(
        Guid id,
        Guid userId,
        NotificationType type,
        NotificationChannel channel,
        string title,
        string message,
        string? deduplicationKey)
        : base(id)
    {
        UserId = userId;
        Type = type;
        Channel = channel;
        Title = title;
        Message = message;
        DeduplicationKey =
            deduplicationKey;
        CreatedAt = DateTimeOffset.UtcNow;
        Status = NotificationStatus.Pending;
    }

    /// <summary>
    /// Gets the owning user identifier.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the notification type.
    /// </summary>
    public NotificationType Type { get; private set; }

    /// <summary>
    /// Gets the delivery channel.
    /// </summary>
    public NotificationChannel Channel { get; private set; }

    /// <summary>
    /// Gets the notification title.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Gets the notification body.
    /// </summary>
    public string Message { get; private set; } = null!;

    /// <summary>
    /// Gets the optional deduplication key.
    /// </summary>
    public string? DeduplicationKey { get; private set; }

    /// <summary>
    /// Gets the notification lifecycle status.
    /// </summary>
    public NotificationStatus Status { get; private set; }

    /// <summary>
    /// Gets when the notification was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets when the notification was delivered.
    /// </summary>
    public DateTimeOffset? DeliveredAt { get; private set; }

    /// <summary>
    /// Gets when the notification was read.
    /// </summary>
    public DateTimeOffset? ReadAt { get; private set; }

    /// <summary>
    /// Gets the number of delivery attempts.
    /// </summary>
    public int AttemptCount { get; private set; }

    /// <summary>
    /// Gets the most recent delivery error.
    /// </summary>
    public string? LastError { get; private set; }

    /// <summary>
    /// Creates a notification.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="type">The notification type.</param>
    /// <param name="channel">The delivery channel.</param>
    /// <param name="title">The title.</param>
    /// <param name="message">The message.</param>
    /// <param name="deduplicationKey">
    /// Optional unique delivery key.
    /// </param>
    /// <returns>A new notification.</returns>
    public static Notification Create(
        Guid userId,
        NotificationType type,
        NotificationChannel channel,
        string title,
        string message,
        string? deduplicationKey)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier cannot be empty.",
                nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            title);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            message);

        return new Notification(
            Guid.NewGuid(),
            userId,
            type,
            channel,
            title.Trim(),
            message.Trim(),
            deduplicationKey);
    }

    /// <summary>
    /// Marks the notification as delivered.
    /// </summary>
    public void MarkDelivered()
    {
        Status =
            NotificationStatus.Delivered;

        DeliveredAt =
            DateTimeOffset.UtcNow;

        LastError = null;
    }

    /// <summary>
    /// Marks the notification as read.
    /// </summary>
    public void MarkRead()
    {
        if (Status ==
            NotificationStatus.Delivered)
        {
            Status =
                NotificationStatus.Read;
        }

        ReadAt =
            DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Records a failed delivery attempt.
    /// </summary>
    /// <param name="error">The delivery error.</param>
    public void MarkFailed(
        string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            error);

        AttemptCount++;

        LastError =
            error.Length > 2000
                ? error[..2000]
                : error;

        Status =
            NotificationStatus.Failed;
    }

    /// <summary>
    /// Permanently dead-letters the notification.
    /// </summary>
    /// <param name="error">The terminal error.</param>
    public void MarkDeadLettered(
        string error)
    {
        MarkFailed(error);

        Status =
            NotificationStatus.DeadLettered;
    }
}
