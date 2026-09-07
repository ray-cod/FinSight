using System.Text.Json;
using FinSight.Application.Abstractions.Security;
using FinSight.Domain.Auditing;
using FinSight.Domain.Security;
using FinSight.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinSight.Infrastructure.Audit;

/// <summary>
/// Persists security audit events and mirrors them to structured logs.
/// </summary>
public sealed partial class PersistentAuditService(
    FinSightDbContext dbContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<PersistentAuditService> logger)
    : IAuditService
{
    /// <inheritdoc />
    public async Task RecordAsync(
        SecurityEventType eventType,
        Guid? userId,
        string? ipAddress,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var httpContext =
            httpContextAccessor.HttpContext;

        var auditEvent =
            AuditEvent.Create(
                userId,
                eventType.ToString(),
                ipAddress,
                httpContext?
                    .Items["X-Correlation-ID"]?
                    .ToString(),
                System.Diagnostics.Activity
                    .Current?
                    .TraceId
                    .ToString(),
                metadata is null
                    ? null
                    : JsonSerializer.Serialize(
                        metadata));

        dbContext.Set<AuditEvent>()
            .Add(auditEvent);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        LogSecurityAuditEventPersisted(
            logger,
            eventType,
            userId);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Security audit event persisted. Type={EventType}, UserId={UserId}")]
    private static partial void LogSecurityAuditEventPersisted(
        ILogger logger,
        SecurityEventType eventType,
        Guid? userId);
}
