namespace FinSight.Contracts.Events;

/// <summary>
/// Published when a user resolves an anomaly.
/// </summary>
public sealed record AnomalyResolvedEvent
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
    /// Gets the anomaly identifier.
    /// </summary>
    public required Guid AnomalyId { get; init; }

    /// <summary>
    /// Gets the resulting status.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Gets the resolution timestamp.
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
