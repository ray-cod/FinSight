using FinSight.Domain.Notifications;

namespace FinSight.Application.Abstractions.Notifications;

/// <summary>
/// Provides notification persistence and retrieval operations.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Gets notifications belonging to a user.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="includeRead">
    /// Whether read notifications should be returned.
    /// </param>
    /// <param name="limit">Maximum records.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's notifications.</returns>
    Task<IReadOnlyList<Notification>> GetForUserAsync(
        Guid userId,
        bool includeRead = false,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a notification within a user scope.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="notificationId">The notification identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The notification when found.</returns>
    Task<Notification?> GetByIdAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the deduplication key already exists.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="deduplicationKey">The deduplication key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when an equivalent notification exists.</returns>
    Task<bool> ExistsByDeduplicationKeyAsync(
        Guid userId,
        string deduplicationKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a notification.
    /// </summary>
    /// <param name="notification">The notification.</param>
    void Add(Notification notification);
}
