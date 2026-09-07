using FinSight.Api.Extensions;
using FinSight.Application.Features.Notifications;
using FinSight.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides authenticated notification endpoints.
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public sealed class NotificationsController(
    NotificationService notificationService)
    : ControllerBase
{
    /// <summary>
    /// Gets notifications belonging to the current user.
    /// </summary>
    /// <param name="includeRead">
    /// Whether read notifications should be included.
    /// </param>
    /// <param name="limit">
    /// Maximum notifications.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>The user's notifications.</returns>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] bool includeRead = false,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var notifications =
            await notificationService
                .GetForUserAsync(
                    User.GetRequiredUserId(),
                    includeRead,
                    limit,
                    cancellationToken);

        return Ok(
            notifications.Select(
                notification =>
                    new
                    {
                        id =
                            notification.Id,
                        type =
                            notification.Type,
                        channel =
                            notification.Channel,
                        title =
                            notification.Title,
                        message =
                            notification.Message,
                        status =
                            notification.Status,
                        createdAt =
                            notification.CreatedAt,
                        readAt =
                            notification.ReadAt
                    }));
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="notificationId">
    /// The notification identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    [HttpPost("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkRead(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        await notificationService
            .MarkReadAsync(
                User.GetRequiredUserId(),
                notificationId,
                cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Gets notification preferences.
    /// </summary>
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(
        CancellationToken cancellationToken)
    {
        var preferences =
            await notificationService
                .GetOrCreatePreferencesAsync(
                    User.GetRequiredUserId(),
                    cancellationToken);

        return Ok(
            new
            {
                inAppEnabled =
                    preferences.InAppEnabled,
                emailEnabled =
                    preferences.EmailEnabled,
                anomalyNotificationsEnabled =
                    preferences
                        .AnomalyNotificationsEnabled,
                subscriptionNotificationsEnabled =
                    preferences
                        .SubscriptionNotificationsEnabled,
                insightNotificationsEnabled =
                    preferences
                        .InsightNotificationsEnabled
            });
    }

    /// <summary>
    /// Updates notification preferences.
    /// </summary>
    /// <param name="request">
    /// The preference update request.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(
        UpdateNotificationPreferencesRequest request,
        CancellationToken cancellationToken)
    {
        var preferences =
            await notificationService
                .GetOrCreatePreferencesAsync(
                    User.GetRequiredUserId(),
                    cancellationToken);

        preferences.SetEmailEnabled(
            request.EmailEnabled);

        preferences.SetAnomalyNotificationsEnabled(
            request.AnomalyNotificationsEnabled);

        preferences.SetSubscriptionNotificationsEnabled(
            request.SubscriptionNotificationsEnabled);

        preferences.SetInsightNotificationsEnabled(
            request.InsightNotificationsEnabled);

        await notificationService
            .SavePreferencesAsync(
                preferences,
                cancellationToken);

        return NoContent();
    }
}

/// <summary>
/// Represents a notification preference update.
/// </summary>
public sealed record UpdateNotificationPreferencesRequest(
    bool EmailEnabled,
    bool AnomalyNotificationsEnabled,
    bool SubscriptionNotificationsEnabled,
    bool InsightNotificationsEnabled);
