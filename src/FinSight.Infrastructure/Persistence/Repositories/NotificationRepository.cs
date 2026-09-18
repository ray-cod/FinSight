using FinSight.Application.Abstractions.Notifications;
using FinSight.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements notification persistence.
/// </summary>
public sealed class NotificationRepository(
    FinSightDbContext dbContext)
    : INotificationRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Notification>>
        GetForUserAsync(
            Guid userId,
            bool includeRead = false,
            int limit = 100,
            CancellationToken cancellationToken = default)
    {
        limit =
            Math.Clamp(
                limit,
                1,
                500);

        var query =
            dbContext.Set<Notification>()
                .AsNoTracking()
                .Where(
                    x =>
                        x.UserId == userId &&
                        x.Channel == NotificationChannel.InApp);

        if (!includeRead)
        {
            query =
                query.Where(
                    x =>
                        x.Status != NotificationStatus.Read &&
                        x.Status != NotificationStatus.Dismissed);
        }

        return await query
            .OrderByDescending(
                x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<Notification?> GetByIdAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        return dbContext
            .Set<Notification>()
            .SingleOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.Id == notificationId,
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool>
        ExistsByDeduplicationKeyAsync(
            Guid userId,
            string deduplicationKey,
            CancellationToken cancellationToken = default)
    {
        return dbContext
            .Set<Notification>()
            .AnyAsync(
                x =>
                    x.UserId == userId &&
                    x.DeduplicationKey ==
                    deduplicationKey,
                cancellationToken);
    }


    /// <inheritdoc />
    public Task<int> CountUnreadInAppAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext
            .Set<Notification>()
            .CountAsync(
                x =>
                    x.UserId == userId &&
                    x.Channel == NotificationChannel.InApp &&
                    x.Status != NotificationStatus.Read,
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> MarkAllReadAsync(
        Guid userId,
        DateTimeOffset readAt,
        CancellationToken cancellationToken = default)
    {
        return dbContext
            .Set<Notification>()
            .Where(
                x =>
                    x.UserId == userId &&
                    x.Channel == NotificationChannel.InApp &&
                    (x.Status == NotificationStatus.Delivered ||
                     x.Status == NotificationStatus.Failed ||
                     x.Status == NotificationStatus.DeadLettered))
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(
                            x => x.Status,
                            NotificationStatus.Read)
                        .SetProperty(
                            x => x.ReadAt,
                            readAt),
                cancellationToken);
    }

    /// <inheritdoc />
    public void Add(
        Notification notification)
    {
        dbContext.Set<Notification>()
            .Add(notification);
    }
}
