using FinSight.Domain.Common;

namespace FinSight.Domain.Subscriptions;

/// <summary>
/// Represents a recurring financial commitment detected from transaction history.
/// </summary>
public sealed class Subscription
    : AggregateRoot<Guid>
{
    private Subscription()
    {
    }

    private Subscription(
        Guid id,
        Guid userId,
        Guid merchantId,
        string merchantName,
        string currency,
        BillingFrequency frequency,
        decimal amount,
        decimal confidence,
        DateTimeOffset firstDetectedAt,
        DateTimeOffset lastChargeAt,
        DateTimeOffset? nextExpectedChargeAt)
        : base(id)
    {
        UserId = userId;
        MerchantId = merchantId;
        MerchantName = merchantName;
        Currency = currency;
        Frequency = frequency;
        CurrentAmount = amount;
        AverageAmount = amount;
        DetectionConfidence = confidence;
        FirstDetectedAt = firstDetectedAt;
        LastChargeAt = lastChargeAt;
        NextExpectedChargeAt =
            nextExpectedChargeAt;
        Status = SubscriptionStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the user who owns the subscription.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the normalized merchant identifier.
    /// </summary>
    public Guid MerchantId { get; private set; }

    /// <summary>
    /// Gets the merchant name captured when the subscription was detected.
    /// </summary>
    public string MerchantName { get; private set; } = null!;

    /// <summary>
    /// Gets the subscription currency.
    /// </summary>
    public string Currency { get; private set; } = null!;

    /// <summary>
    /// Gets the estimated billing frequency.
    /// </summary>
    public BillingFrequency Frequency { get; private set; }

    /// <summary>
    /// Gets the latest observed subscription amount.
    /// </summary>
    public decimal CurrentAmount { get; private set; }

    /// <summary>
    /// Gets the historical average charge amount.
    /// </summary>
    public decimal AverageAmount { get; private set; }

    /// <summary>
    /// Gets the confidence that this pattern represents a subscription.
    /// </summary>
    public decimal DetectionConfidence { get; private set; }

    /// <summary>
    /// Gets the date on which the subscription pattern was first detected.
    /// </summary>
    public DateTimeOffset FirstDetectedAt { get; private set; }

    /// <summary>
    /// Gets the date of the most recent observed charge.
    /// </summary>
    public DateTimeOffset LastChargeAt { get; private set; }

    /// <summary>
    /// Gets the estimated date of the next charge.
    /// </summary>
    public DateTimeOffset? NextExpectedChargeAt { get; private set; }

    /// <summary>
    /// Gets the date on which the subscription price most recently changed.
    /// </summary>
    public DateTimeOffset? LastPriceChangedAt { get; private set; }

    /// <summary>
    /// Gets the subscription lifecycle status.
    /// </summary>
    public SubscriptionStatus Status { get; private set; }

    /// <summary>
    /// Gets the timestamp at which the subscription record was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp at which the subscription record was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>
    /// Creates a newly detected subscription.
    /// </summary>
    /// <param name="userId">The owning user identifier.</param>
    /// <param name="merchantId">The normalized merchant identifier.</param>
    /// <param name="merchantName">The merchant name.</param>
    /// <param name="currency">The ISO currency code.</param>
    /// <param name="frequency">The billing frequency.</param>
    /// <param name="amount">The observed amount.</param>
    /// <param name="confidence">Detection confidence from 0 to 1.</param>
    /// <param name="firstDetectedAt">
    /// The earliest transaction considered part of the pattern.
    /// </param>
    /// <param name="lastChargeAt">The latest observed charge.</param>
    /// <param name="nextExpectedChargeAt">
    /// The estimated next charge date.
    /// </param>
    /// <returns>A new subscription.</returns>
    public static Subscription Create(
        Guid userId,
        Guid merchantId,
        string merchantName,
        string currency,
        BillingFrequency frequency,
        decimal amount,
        decimal confidence,
        DateTimeOffset firstDetectedAt,
        DateTimeOffset lastChargeAt,
        DateTimeOffset? nextExpectedChargeAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier cannot be empty.",
                nameof(userId));
        }

        if (merchantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Merchant identifier cannot be empty.",
                nameof(merchantId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            merchantName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            currency);

        ValidateConfidence(confidence);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            amount);

        return new Subscription(
            Guid.NewGuid(),
            userId,
            merchantId,
            merchantName.Trim(),
            currency.Trim().ToUpperInvariant(),
            frequency,
            amount,
            confidence,
            firstDetectedAt,
            lastChargeAt,
            nextExpectedChargeAt);
    }

    /// <summary>
    /// Updates the subscription after observing a new recurring charge.
    /// </summary>
    /// <param name="frequency">The inferred billing frequency.</param>
    /// <param name="currentAmount">The current charge amount.</param>
    /// <param name="averageAmount">The updated average charge amount.</param>
    /// <param name="confidence">The updated confidence score.</param>
    /// <param name="chargeDate">The latest charge date.</param>
    /// <param name="nextExpectedChargeAt">
    /// The estimated next charge date.
    /// </param>
    /// <param name="priceChanged">
    /// Whether the observed amount materially changed.
    /// </param>
    public void ObserveCharge(
        BillingFrequency frequency,
        decimal currentAmount,
        decimal averageAmount,
        decimal confidence,
        DateTimeOffset chargeDate,
        DateTimeOffset? nextExpectedChargeAt,
        bool priceChanged)
    {
        ValidateConfidence(confidence);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            currentAmount);

        Frequency = frequency;
        CurrentAmount = currentAmount;
        AverageAmount = averageAmount;
        DetectionConfidence = confidence;
        LastChargeAt = chargeDate;
        NextExpectedChargeAt =
            nextExpectedChargeAt;
        Status = SubscriptionStatus.Active;

        if (priceChanged)
        {
            LastPriceChangedAt =
                DateTimeOffset.UtcNow;
        }

        UpdatedAt =
            DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Dismisses the subscription.
    /// </summary>
    public void Dismiss()
    {
        Status = SubscriptionStatus.Dismissed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the subscription as inactive.
    /// </summary>
    public void MarkInactive()
    {
        if (Status == SubscriptionStatus.Dismissed)
        {
            return;
        }

        Status = SubscriptionStatus.Inactive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Reactivates an inactive subscription.
    /// </summary>
    public void Reactivate()
    {
        Status = SubscriptionStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void ValidateConfidence(
        decimal confidence)
    {
        if (confidence < 0m ||
            confidence > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(confidence));
        }
    }
}
