using FinSight.Application.Abstractions.Notifications;
using FinSight.Application.Abstractions.Observability;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace FinSight.Application.Features.Notifications;

/// <summary>
/// Delivers persisted notifications through configured notification channels.
/// </summary>
public sealed partial class NotificationDeliveryService
{
    private const int MaximumAttempts = 5;

    private readonly INotificationRepository
        _repository;

    private readonly IEnumerable<INotificationSender>
        _senders;

    private readonly IUnitOfWork
        _unitOfWork;

    private readonly ILogger<
        NotificationDeliveryService>
        _logger;

    private readonly IFinSightTelemetry _telemetry;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="NotificationDeliveryService"/> class.
    /// </summary>
    public NotificationDeliveryService(
        INotificationRepository repository,
        IEnumerable<INotificationSender> senders,
        IUnitOfWork unitOfWork,
        ILogger<NotificationDeliveryService> logger,
        IFinSightTelemetry telemetry)
    {
        _repository = repository;
        _senders = senders;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _telemetry = telemetry;
    }

    /// <summary>
    /// Delivers a notification.
    /// </summary>
    /// <param name="notificationId">
    /// The notification identifier.
    /// </param>
    /// <param name="userId">The owning user.</param>
    /// <param name="recipient">The destination.</param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task DeliverAsync(
        Guid notificationId,
        Guid userId,
        string recipient,
        CancellationToken cancellationToken = default)
    {
        var notification =
            await _repository.GetByIdAsync(
                userId,
                notificationId,
                cancellationToken);

        if (notification is null)
        {
            throw new KeyNotFoundException(
                "Notification was not found.");
        }

        if (notification.Status ==
            NotificationStatus.Read ||
            notification.Status ==
            NotificationStatus.Delivered)
        {
            return;
        }

        var sender =
            _senders.FirstOrDefault(
                x =>
                    x.Channel ==
                    notification.Channel);

        if (sender is null)
        {
            throw new InvalidOperationException(
                $"No notification sender is configured for {notification.Channel}.");
        }

        try
        {
            await sender.SendAsync(
                notification,
                recipient,
                cancellationToken);

            notification.MarkDelivered();

            await _unitOfWork
                .SaveChangesAsync(
                    cancellationToken);

            _telemetry.IncrementNotificationsDelivered(1);
        }
        catch (Exception exception)
        {
            notification.MarkFailed(
                exception.Message);

            if (
                notification.AttemptCount >=
                MaximumAttempts)
            {
                notification.MarkDeadLettered(
                    exception.Message);
            }

            await _unitOfWork
                .SaveChangesAsync(
                    cancellationToken);

            _telemetry.IncrementNotificationFailures(1);

            LogNotificationDeliveryFailed(
                exception,
                notificationId);

            throw;
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Notification delivery failed for {NotificationId}.")]
    private partial void LogNotificationDeliveryFailed(Exception exception, Guid notificationId);
}
