using FinSight.Application.Abstractions.Security;
using FinSight.Domain.Security;
using Microsoft.Extensions.Logging;

namespace FinSight.Infrastructure.Audit;

/// <summary>
/// Records security events using structured application logging.
/// </summary>
public sealed partial class AuditService(
    ILogger<AuditService> logger)
    : IAuditService
{
    /// <inheritdoc />
    public Task RecordAsync(
        SecurityEventType eventType,
        Guid? userId,
        string? ipAddress,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LogSecurityEvent(
            eventType,
            userId,
            ipAddress,
            metadata);

        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Security event {SecurityEventType}. UserId: {UserId}. IP: {IpAddress}. Metadata: {@Metadata}")]
    private partial void LogSecurityEvent(
        SecurityEventType securityEventType,
        Guid? userId,
        string? ipAddress,
        IReadOnlyDictionary<string, string>? metadata);
}
