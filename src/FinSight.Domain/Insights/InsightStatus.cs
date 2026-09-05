namespace FinSight.Domain.Insights;

/// <summary>
/// Represents the lifecycle state of a financial insight.
/// </summary>
public enum InsightStatus
{
    /// <summary>
    /// The insight is available to the user.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The user has seen the insight.
    /// </summary>
    Seen = 2,

    /// <summary>
    /// The user dismissed the insight.
    /// </summary>
    Dismissed = 3,

    /// <summary>
    /// The insight is no longer considered current.
    /// </summary>
    Expired = 4
}
