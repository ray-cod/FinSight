using FinSight.Application.Abstractions.Notifications;
using FinSight.Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace FinSight.Infrastructure.Notifications;

/// <summary>
/// Logs notification delivery attempts for local development.
/// </summary>
public sealed partial class DevelopmentNotificationSender(
    ILogger<DevelopmentNotificationSender> logger)
    : INotificationSender
{
    /// <inheritdoc />
    public NotificationChannel Channel =>
        NotificationChannel.Email;

    /// <inheritdoc />
    public Task SendAsync(
        Notification notification,
        string recipient,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LogDevelopmentEmailNotification(
            logger,
            recipient,
            notification.Title,
            notification.Message);

        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Development email notification. Recipient={Recipient}, Title={Title}, Message={Message}")]
    private static partial void LogDevelopmentEmailNotification(
        ILogger logger,
        string recipient,
        string title,
        string message);
}
