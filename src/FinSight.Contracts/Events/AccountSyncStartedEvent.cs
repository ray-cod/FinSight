namespace FinSight.Contracts.Events;

/// <summary>
/// Published when financial account synchronization starts.
/// </summary>
public sealed record AccountSyncStartedEvent
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
    /// Gets the event timestamp.
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
