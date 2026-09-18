using FinSight.Application.Abstractions.Security;
using FinSight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Identity;

/// <summary>
/// Provides user-scoped projections of security audit activity.
/// </summary>
public sealed class SecurityService(
    FinSightDbContext dbContext)
    : ISecurityService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<SecurityActivityResponse>> GetRecentActivityAsync(
        Guid userId,
        int limit = 25,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var boundedLimit = Math.Clamp(limit, 1, 50);

        return await dbContext.AuditEvents
            .AsNoTracking()
            .Where(auditEvent => auditEvent.UserId == userId)
            .OrderByDescending(auditEvent => auditEvent.OccurredAt)
            .Take(boundedLimit)
            .Select(
                auditEvent =>
                    new SecurityActivityResponse(
                        auditEvent.EventType,
                        auditEvent.OccurredAt))
            .ToListAsync(cancellationToken);
    }
}
