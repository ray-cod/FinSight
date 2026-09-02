namespace FinSight.Contracts.Events;

/// <summary>
/// Published when a financial institution connection is created.
/// </summary>
public sealed record AccountConnectedEvent
{
    /// <summary>
    /// Gets the unique event identifier.
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
    /// Gets the institution identifier.
    /// </summary>
    public required Guid InstitutionId { get; init; }

    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
