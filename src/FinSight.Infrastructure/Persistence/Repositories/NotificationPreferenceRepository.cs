using FinSight.Application.Abstractions.Notifications;
using FinSight.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements notification-preference persistence.
/// </summary>
public sealed class NotificationPreferenceRepository(
    FinSightDbContext dbContext)
    : INotificationPreferenceRepository
{
    /// <inheritdoc />
    public Task<NotificationPreference?>
        GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        return dbContext
            .Set<NotificationPreference>()
            .SingleOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);
    }

    /// <inheritdoc />
    public void Add(
        NotificationPreference preferences)
    {
        dbContext
            .Set<NotificationPreference>()
            .Add(preferences);
    }
}
