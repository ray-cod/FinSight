namespace FinSight.Domain.Common;

/// <summary>
/// Abstract record representing an immutable domain event raised within the application domain.
/// </summary>
public abstract record DomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this specific domain event occurrence.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the UTC date and time offset when the domain event occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } =
        DateTimeOffset.UtcNow;
}
