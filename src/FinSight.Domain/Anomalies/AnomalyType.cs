namespace FinSight.Domain.Anomalies;

/// <summary>
/// Represents the reason a financial event was considered unusual.
/// </summary>
public enum AnomalyType
{
    /// <summary>
    /// The transaction amount is unusually large for the user or merchant.
    /// </summary>
    LargeTransaction = 1,

    /// <summary>
    /// Spending with a merchant is unusually high.
    /// </summary>
    MerchantSpendingSpike = 2,

    /// <summary>
    /// Spending in a category is unusually high.
    /// </summary>
    CategorySpendingSpike = 3,

    /// <summary>
    /// The merchant has not previously appeared in the user's history.
    /// </summary>
    NewMerchant = 4,

    /// <summary>
    /// A transaction appears to duplicate another transaction.
    /// </summary>
    DuplicateTransaction = 5,

    /// <summary>
    /// Transaction frequency with a merchant is unusually high.
    /// </summary>
    MerchantFrequencySpike = 6,

    /// <summary>
    /// A recurring subscription charge changed materially.
    /// </summary>
    SubscriptionPriceChange = 7
}
