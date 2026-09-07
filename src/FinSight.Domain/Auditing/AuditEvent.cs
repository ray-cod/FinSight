using FinSight.Domain.Common;

namespace FinSight.Domain.Auditing;

/// <summary>
/// Represents a persisted security or audit event.
/// </summary>
public sealed class AuditEvent : Entity<Guid>
{
    private AuditEvent()
    {
    }

    private AuditEvent(
        Guid id,
        Guid? userId,
        string eventType,
        string? ipAddress,
        string? correlationId,
        string? traceId,
        string? metadata)
        : base(id)
    {
        UserId = userId;
        EventType = eventType;
        IpAddress = ipAddress;
        CorrelationId = correlationId;
        TraceId = traceId;
        Metadata = metadata;
        OccurredAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the user associated with the event.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Gets the audit event type.
    /// </summary>
    public string EventType { get; private set; } = null!;

    /// <summary>
    /// Gets the originating IP address.
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// Gets the request correlation identifier.
    /// </summary>
    public string? CorrelationId { get; private set; }

    /// <summary>
    /// Gets the distributed trace identifier.
    /// </summary>
    public string? TraceId { get; private set; }

    /// <summary>
    /// Gets additional non-sensitive metadata.
    /// </summary>
    public string? Metadata { get; private set; }

    /// <summary>
    /// Gets when the audit event occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; private set; }

    /// <summary>
    /// Creates an audit event.
    /// </summary>
    /// <param name="userId">The optional user identifier.</param>
    /// <param name="eventType">The event type.</param>
    /// <param name="ipAddress">The client IP.</param>
    /// <param name="correlationId">The request correlation ID.</param>
    /// <param name="traceId">The distributed trace ID.</param>
    /// <param name="metadata">Non-sensitive metadata.</param>
    /// <returns>A new audit event.</returns>
    public static AuditEvent Create(
        Guid? userId,
        string eventType,
        string? ipAddress,
        string? correlationId,
        string? traceId,
        string? metadata)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            eventType);

        return new AuditEvent(
            Guid.NewGuid(),
            userId,
            eventType.Trim(),
            ipAddress,
            correlationId,
            traceId,
            metadata);
    }
}
