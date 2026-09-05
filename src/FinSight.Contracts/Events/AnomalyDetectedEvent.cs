namespace FinSight.Contracts.Events;

/// <summary>
/// Published when FinSight detects an anomaly.
/// </summary>
public sealed record AnomalyDetectedEvent
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
    /// Gets the triggering transaction identifier.
    /// </summary>
    public required Guid TransactionId { get; init; }

    /// <summary>
    /// Gets the anomaly type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets the anomaly severity.
    /// </summary>
    public required string Severity { get; init; }

    /// <summary>
    /// Gets the anomaly score.
    /// </summary>
    public required decimal Score { get; init; }

    /// <summary>
    /// Gets the anomaly confidence.
    /// </summary>
    public required decimal Confidence { get; init; }

    /// <summary>
    /// Gets the anomaly title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the anomaly description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the detection timestamp.
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
