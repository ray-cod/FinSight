namespace FinSight.Domain.Subscriptions;

/// <summary>
/// Represents the lifecycle state of a detected subscription.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>
    /// The subscription is currently considered active.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The subscription appears to have stopped charging.
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// The user explicitly dismissed the detected subscription.
    /// </summary>
    Dismissed = 3
}
