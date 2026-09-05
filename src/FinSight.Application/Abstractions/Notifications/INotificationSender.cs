using FinSight.Domain.Notifications;

namespace FinSight.Application.Abstractions.Notifications;

/// <summary>
/// Sends notifications through a specific delivery channel.
/// </summary>
public interface INotificationSender
{
    /// <summary>
    /// Gets the channel supported by this sender.
    /// </summary>
    NotificationChannel Channel { get; }

    /// <summary>
    /// Sends a notification to a user.
    /// </summary>
    /// <param name="notification">The notification.</param>
    /// <param name="recipient">The delivery address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendAsync(
        Notification notification,
        string recipient,
        CancellationToken cancellationToken = default);
}
