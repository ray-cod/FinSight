namespace FinSight.Domain.Notifications;

/// <summary>
/// Represents the semantic reason a notification was generated.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// A financial anomaly was detected.
    /// </summary>
    AnomalyDetected = 1,

    /// <summary>
    /// A subscription price changed.
    /// </summary>
    SubscriptionPriceChanged = 2,

    /// <summary>
    /// A new subscription was detected.
    /// </summary>
    SubscriptionDetected = 3,

    /// <summary>
    /// A general financial insight was generated.
    /// </summary>
    FinancialInsight = 4,

    /// <summary>
    /// A security-related event notification.
    /// </summary>
    Security = 5
}
