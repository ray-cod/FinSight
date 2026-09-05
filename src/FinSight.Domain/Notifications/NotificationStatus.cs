namespace FinSight.Domain.Notifications;

/// <summary>
/// Represents the delivery state of a notification.
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// Notification is waiting for delivery.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Notification was successfully delivered.
    /// </summary>
    Delivered = 2,

    /// <summary>
    /// Notification delivery failed temporarily.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Notification delivery permanently failed.
    /// </summary>
    DeadLettered = 4,

    /// <summary>
    /// User has read the notification.
    /// </summary>
    Read = 5
}
