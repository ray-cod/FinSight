using FinSight.Domain.Security;

namespace FinSight.Application.Abstractions.Security;

/// <summary>
/// Records security-sensitive application events.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Records a security event.
    /// </summary>
    /// <param name="eventType">The security event type.</param>
    /// <param name="userId">The affected user, if known.</param>
    /// <param name="ipAddress">The originating IP address.</param>
    /// <param name="metadata">Additional non-sensitive metadata.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordAsync(
        SecurityEventType eventType,
        Guid? userId,
        string? ipAddress,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);
}
