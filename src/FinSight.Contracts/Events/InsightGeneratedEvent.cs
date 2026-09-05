namespace FinSight.Contracts.Events;

/// <summary>
/// Published when FinSight generates a financial insight.
/// </summary>
public sealed record InsightGeneratedEvent
{
    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    /// Gets the owning user identifier.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the insight identifier.
    /// </summary>
    public required Guid InsightId { get; init; }

    /// <summary>
    /// Gets the source anomaly identifier.
    /// </summary>
    public Guid? AnomalyId { get; init; }

    /// <summary>
    /// Gets the insight type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets the insight severity.
    /// </summary>
    public required string Severity { get; init; }

    /// <summary>
    /// Gets the insight title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
