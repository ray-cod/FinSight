using FinSight.Application.Abstractions.Messaging;
using FinSight.Application.Abstractions.Notifications;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Contracts.Events;
using FinSight.Domain.Notifications;

namespace FinSight.Application.Features.Notifications;

/// <summary>
/// Coordinates notification creation and user notification lifecycle.
/// </summary>
public sealed class NotificationService
{
    private readonly INotificationRepository
        _notificationRepository;

    private readonly IEventPublisher
        _eventPublisher;

    private readonly INotificationPreferenceRepository
        _preferenceRepository;

    private readonly IUnitOfWork
        _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="NotificationService"/> class.
    /// </summary>
    public NotificationService(
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository preferenceRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher)
    {
        _notificationRepository =
            notificationRepository;

        _preferenceRepository =
            preferenceRepository;

        _unitOfWork =
            unitOfWork;

        _eventPublisher =
            eventPublisher;
    }

    /// <summary>
    /// Creates an in-app notification when permitted by user preferences.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="type">The notification type.</param>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="deduplicationKey">
    /// The optional deduplication key.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created notification, or null when disabled.</returns>
    public async Task<Notification?>
        CreateInAppAsync(
            Guid userId,
            NotificationType type,
            string title,
            string message,
            string? deduplicationKey,
            CancellationToken cancellationToken = default)
    {
        var preferences =
            await GetOrCreatePreferencesAsync(
                userId,
                cancellationToken);

        if (!preferences.InAppEnabled ||
            !IsTypeEnabled(
                preferences,
                type))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(
                deduplicationKey) &&
            await _notificationRepository
                .ExistsByDeduplicationKeyAsync(
                    userId,
                    deduplicationKey,
                    cancellationToken))
        {
            return null;
        }

        var notification =
            Notification.Create(
                userId,
                type,
                NotificationChannel.InApp,
                title,
                message,
                deduplicationKey);

        _notificationRepository.Add(
            notification);

        await _eventPublisher.PublishAsync(
            new NotificationCreatedEvent
            {
                EventId = Guid.NewGuid(),
                NotificationId =
                    notification.Id,
                UserId =
                    notification.UserId,
                Channel =
                    notification.Channel.ToString(),
                Recipient =
                    notification.UserId.ToString(),
                OccurredAt =
                    DateTimeOffset.UtcNow
            },
            "notification.created",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return notification;
    }

    /// <summary>
    /// Creates an email notification when permitted by user preferences.
    /// </summary>
    public async Task<Notification?>
        CreateEmailAsync(
            Guid userId,
            NotificationType type,
            string title,
            string message,
            string? deduplicationKey,
            string recipient,
            CancellationToken cancellationToken = default)
    {
        var preferences =
            await GetOrCreatePreferencesAsync(
                userId,
                cancellationToken);

        if (!preferences.EmailEnabled ||
            !IsTypeEnabled(
                preferences,
                type) ||
            string.IsNullOrWhiteSpace(recipient))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(
                deduplicationKey) &&
            await _notificationRepository
                .ExistsByDeduplicationKeyAsync(
                    userId,
                    deduplicationKey,
                    cancellationToken))
        {
            return null;
        }

        var notification =
            Notification.Create(
                userId,
                type,
                NotificationChannel.Email,
                title,
                message,
                deduplicationKey);

        _notificationRepository.Add(
            notification);

        await _eventPublisher.PublishAsync(
            new NotificationCreatedEvent
            {
                EventId = Guid.NewGuid(),
                NotificationId =
                    notification.Id,
                UserId =
                    notification.UserId,
                Channel =
                    notification.Channel.ToString(),
                Recipient =
                    recipient,
                OccurredAt =
                    DateTimeOffset.UtcNow
            },
            "notification.created",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return notification;
    }

    /// <summary>
    /// Gets notifications belonging to a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="includeRead">Whether read notifications are included.</param>
    /// <param name="limit">Maximum records.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's notifications.</returns>
    public Task<IReadOnlyList<Notification>>
        GetForUserAsync(
            Guid userId,
            bool includeRead = false,
            int limit = 100,
            CancellationToken cancellationToken = default)
    {
        return _notificationRepository
            .GetForUserAsync(
                userId,
                includeRead,
                limit,
                cancellationToken);
    }

    /// <summary>
    /// Marks a user's notification as read.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="notificationId">The notification identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task MarkReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification =
            await _notificationRepository
                .GetByIdAsync(
                    userId,
                    notificationId,
                    cancellationToken);

        if (notification is null)
        {
            throw new KeyNotFoundException(
                "Notification was not found.");
        }

        notification.MarkRead();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    /// <summary>
    /// Gets or creates a user's notification preferences.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's preferences.</returns>
    public async Task<NotificationPreference>
        GetOrCreatePreferencesAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        var existing =
            await _preferenceRepository
                .GetByUserIdAsync(
                    userId,
                    cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var preferences =
            NotificationPreference.Create(
                userId);

        _preferenceRepository.Add(
            preferences);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return preferences;
    }

    /// <summary>
    /// Persists updated notification preferences.
    /// </summary>
    /// <param name="preferences">The preferences.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SavePreferencesAsync(
        NotificationPreference preferences,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    private static bool IsTypeEnabled(
        NotificationPreference preferences,
        NotificationType type)
    {
        return type switch
        {
            NotificationType.AnomalyDetected =>
                preferences.AnomalyNotificationsEnabled,

            NotificationType.SubscriptionDetected =>
                preferences.SubscriptionNotificationsEnabled,

            NotificationType.SubscriptionPriceChanged =>
                preferences.SubscriptionNotificationsEnabled,

            NotificationType.FinancialInsight =>
                preferences.InsightNotificationsEnabled,

            NotificationType.Security =>
                true,

            _ =>
                false
        };
    }
}
