using FinSight.Domain.Common;

namespace FinSight.Domain.Notifications;

/// <summary>
/// Represents a user's notification delivery preferences.
/// </summary>
public sealed class NotificationPreference
    : Entity<Guid>
{
    private NotificationPreference()
    {
    }

    private NotificationPreference(
        Guid id,
        Guid userId)
        : base(id)
    {
        UserId = userId;
    }

    /// <summary>
    /// Gets the owning user identifier.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets whether in-app notifications are enabled.
    /// </summary>
    public bool InAppEnabled { get; private set; } = true;

    /// <summary>
    /// Gets whether email notifications are enabled.
    /// </summary>
    public bool EmailEnabled { get; private set; }

    /// <summary>
    /// Gets whether anomaly notifications are enabled.
    /// </summary>
    public bool AnomalyNotificationsEnabled { get; private set; } = true;

    /// <summary>
    /// Gets whether subscription notifications are enabled.
    /// </summary>
    public bool SubscriptionNotificationsEnabled { get; private set; } = true;

    /// <summary>
    /// Gets whether financial insight notifications are enabled.
    /// </summary>
    public bool InsightNotificationsEnabled { get; private set; } = true;

    /// <summary>
    /// Creates default notification preferences for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>Default notification preferences.</returns>
    public static NotificationPreference Create(
        Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier cannot be empty.",
                nameof(userId));
        }

        return new NotificationPreference(
            Guid.NewGuid(),
            userId);
    }

    /// <summary>
    /// Enables or disables email notifications.
    /// </summary>
    /// <param name="enabled">Whether email is enabled.</param>
    public void SetEmailEnabled(
        bool enabled)
    {
        EmailEnabled = enabled;
    }

    /// <summary>
    /// Enables or disables anomaly notifications.
    /// </summary>
    /// <param name="enabled">Whether anomaly notifications are enabled.</param>
    public void SetAnomalyNotificationsEnabled(
        bool enabled)
    {
        AnomalyNotificationsEnabled = enabled;
    }

    /// <summary>
    /// Enables or disables subscription notifications.
    /// </summary>
    /// <param name="enabled">
    /// Whether subscription notifications are enabled.
    /// </param>
    public void SetSubscriptionNotificationsEnabled(
        bool enabled)
    {
        SubscriptionNotificationsEnabled =
            enabled;
    }

    /// <summary>
    /// Enables or disables financial insight notifications.
    /// </summary>
    /// <param name="enabled">
    /// Whether insight notifications are enabled.
    /// </param>
    public void SetInsightNotificationsEnabled(
        bool enabled)
    {
        InsightNotificationsEnabled =
            enabled;
    }
}
