namespace FinSight.Contracts.Events;

/// <summary>
/// Published when a recurring subscription price changes materially.
/// </summary>
public sealed record SubscriptionPriceChangedEvent
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
    /// Gets the subscription identifier.
    /// </summary>
    public required Guid SubscriptionId { get; init; }

    /// <summary>
    /// Gets the merchant identifier.
    /// </summary>
    public required Guid MerchantId { get; init; }

    /// <summary>
    /// Gets the merchant name.
    /// </summary>
    public required string MerchantName { get; init; }

    /// <summary>
    /// Gets the previous subscription amount.
    /// </summary>
    public required decimal PreviousAmount { get; init; }

    /// <summary>
    /// Gets the new subscription amount.
    /// </summary>
    public required decimal CurrentAmount { get; init; }

    /// <summary>
    /// Gets the fractional percentage change.
    /// </summary>
    public required decimal ChangePercentage { get; init; }

    /// <summary>
    /// Gets the currency code.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Gets the source transaction identifier.
    /// </summary>
    public required Guid TransactionId { get; init; }

    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
