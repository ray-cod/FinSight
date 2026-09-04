using FinSight.Domain.Common;

namespace FinSight.Domain.Subscriptions;

/// <summary>
/// Represents an observed subscription charge used to maintain price history.
/// </summary>
public sealed class SubscriptionPriceHistory
    : Entity<Guid>
{
    private SubscriptionPriceHistory()
    {
    }

    private SubscriptionPriceHistory(
        Guid id,
        Guid subscriptionId,
        Guid transactionId,
        decimal amount,
        DateTimeOffset observedAt)
        : base(id)
    {
        SubscriptionId = subscriptionId;
        TransactionId = transactionId;
        Amount = amount;
        ObservedAt = observedAt;
    }

    /// <summary>
    /// Gets the subscription identifier.
    /// </summary>
    public Guid SubscriptionId { get; private set; }

    /// <summary>
    /// Gets the source transaction identifier.
    /// </summary>
    public Guid TransactionId { get; private set; }

    /// <summary>
    /// Gets the observed subscription amount.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Gets when this price was observed.
    /// </summary>
    public DateTimeOffset ObservedAt { get; private set; }

    /// <summary>
    /// Creates a subscription price observation.
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier.</param>
    /// <param name="transactionId">The source transaction identifier.</param>
    /// <param name="amount">The positive charge amount.</param>
    /// <param name="observedAt">The observation timestamp.</param>
    /// <returns>A price history record.</returns>
    public static SubscriptionPriceHistory Create(
        Guid subscriptionId,
        Guid transactionId,
        decimal amount,
        DateTimeOffset observedAt)
    {
        if (subscriptionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Subscription identifier cannot be empty.",
                nameof(subscriptionId));
        }

        if (transactionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Transaction identifier cannot be empty.",
                nameof(transactionId));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            amount);

        return new SubscriptionPriceHistory(
            Guid.NewGuid(),
            subscriptionId,
            transactionId,
            amount,
            observedAt);
    }
}
