namespace FinSight.Contracts.Events;

/// <summary>
/// Published when a transaction receives a classification.
/// </summary>
public sealed record TransactionCategorizedEvent
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
    /// Gets the transaction identifier.
    /// </summary>
    public required Guid TransactionId { get; init; }

    /// <summary>
    /// Gets the merchant identifier.
    /// </summary>
    public required Guid MerchantId { get; init; }

    /// <summary>
    /// Gets the category identifier.
    /// </summary>
    public required Guid CategoryId { get; init; }

    /// <summary>
    /// Gets the optional subcategory identifier.
    /// </summary>
    public Guid? SubcategoryId { get; init; }

    /// <summary>
    /// Gets the classification source.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Gets the classification confidence.
    /// </summary>
    public required decimal Confidence { get; init; }

    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
