namespace FinSight.Contracts.Events;

/// <summary>
/// Published when financial account synchronization fails.
/// </summary>
public sealed record AccountSyncFailedEvent
{
    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the connection identifier.
    /// </summary>
    public required Guid ConnectionId { get; init; }

    /// <summary>
    /// Gets the failure description.
    /// </summary>
    public required string Error { get; init; }

    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
