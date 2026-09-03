namespace FinSight.Contracts.Events;

/// <summary>
/// Published when FinSight detects a recurring subscription.
/// </summary>
public sealed record SubscriptionDetectedEvent
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
    /// Gets the merchant display name.
    /// </summary>
    public required string MerchantName { get; init; }

    /// <summary>
    /// Gets the latest subscription amount.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Gets the ISO currency code.
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Gets the billing frequency.
    /// </summary>
    public required string Frequency { get; init; }

    /// <summary>
    /// Gets the detection confidence.
    /// </summary>
    public required decimal Confidence { get; init; }

    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
