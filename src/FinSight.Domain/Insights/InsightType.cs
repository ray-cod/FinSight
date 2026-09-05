namespace FinSight.Domain.Insights;

/// <summary>
/// Represents the type of financial insight presented to a user.
/// </summary>
public enum InsightType
{
    /// <summary>
    /// An unusual individual transaction.
    /// </summary>
    UnusualTransaction = 1,

    /// <summary>
    /// A category has materially increased in spending.
    /// </summary>
    CategorySpendingIncrease = 2,

    /// <summary>
    /// A merchant has received an unusually large transaction.
    /// </summary>
    MerchantSpendingIncrease = 3,

    /// <summary>
    /// A new merchant has appeared.
    /// </summary>
    NewMerchant = 4,

    /// <summary>
    /// A likely duplicate transaction has been identified.
    /// </summary>
    PossibleDuplicate = 5,

    /// <summary>
    /// A subscription charge changed materially.
    /// </summary>
    SubscriptionPriceIncrease = 6,

    /// <summary>
    /// A subscription charge decreased materially.
    /// </summary>
    SubscriptionPriceDecrease = 7,

    /// <summary>
    /// A general category trend was detected.
    /// </summary>
    CategoryTrend = 8
}
