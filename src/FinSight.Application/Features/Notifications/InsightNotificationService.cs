using FinSight.Application.Abstractions.Identity;
using FinSight.Application.Abstractions.Messaging;
using FinSight.Contracts.Events;
using FinSight.Domain.Notifications;

namespace FinSight.Application.Features.Notifications;

/// <summary>
/// Creates user notifications for generated financial insights.
/// </summary>
public sealed class InsightNotificationService
{
    private readonly NotificationService
        _notificationService;

    private readonly IUserContactService
        _userContactService;

    private readonly IEventPublisher
        _eventPublisher;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="InsightNotificationService"/> class.
    /// </summary>
    public InsightNotificationService(
        NotificationService notificationService,
        IUserContactService userContactService,
        IEventPublisher eventPublisher)
    {
        _notificationService =
            notificationService;

        _userContactService =
            userContactService;

        _eventPublisher =
            eventPublisher;
    }

    /// <summary>
    /// Creates notifications for a generated insight.
    /// </summary>
    /// <param name="message">
    /// The generated insight event.
    /// </param>
    /// <param name="description">
    /// The user-readable insight description.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task ProcessAsync(
        InsightGeneratedEvent message,
        string description,
        CancellationToken cancellationToken = default)
    {
        var preferences =
            await _notificationService
                .GetOrCreatePreferencesAsync(
                    message.UserId,
                    cancellationToken);

        if (preferences.InAppEnabled)
        {
            await _notificationService
                .CreateInAppAsync(
                    message.UserId,
                    NotificationType.FinancialInsight,
                    message.Title,
                    description,
                    $"insight:{message.InsightId}:inapp",
                    cancellationToken);
        }

        if (preferences.EmailEnabled)
        {
            var emailAddress =
                await _userContactService
                    .GetEmailAsync(
                        message.UserId,
                        cancellationToken);

            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                await _notificationService
                    .CreateEmailAsync(
                        message.UserId,
                        NotificationType.FinancialInsight,
                        message.Title,
                        description,
                        $"insight:{message.InsightId}:email",
                        emailAddress,
                        cancellationToken);
            }
        }
    }
}
