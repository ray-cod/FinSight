using FinSight.Domain.Notifications;

namespace FinSight.Application.Abstractions.Notifications;

/// <summary>
/// Provides notification preference persistence.
/// </summary>
public interface INotificationPreferenceRepository
{
    /// <summary>
    /// Gets preferences for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's preferences.</returns>
    Task<NotificationPreference?>
        GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds notification preferences.
    /// </summary>
    /// <param name="preferences">The preferences.</param>
    void Add(
        NotificationPreference preferences);
}
