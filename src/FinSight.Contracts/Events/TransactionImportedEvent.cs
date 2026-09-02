namespace FinSight.Contracts.Events;

/// <summary>
/// Published when a bank transaction is imported successfully.
/// </summary>
public sealed record TransactionImportedEvent
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
    /// Gets the account identifier.
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Gets the transaction identifier.
    /// </summary>
    public required Guid TransactionId { get; init; }

    /// <summary>
    /// Gets the provider transaction identifier.
    /// </summary>
    public required string ProviderTransactionId { get; init; }

    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
