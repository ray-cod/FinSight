namespace FinSight.Domain.Anomalies;

/// <summary>
/// Represents the lifecycle state of an anomaly.
/// </summary>
public enum AnomalyStatus
{
    /// <summary>
    /// The anomaly has been detected but not acted upon.
    /// </summary>
    Open = 1,

    /// <summary>
    /// The user has acknowledged or handled the anomaly.
    /// </summary>
    Resolved = 2,

    /// <summary>
    /// The anomaly was dismissed by the user.
    /// </summary>
    Dismissed = 3
}
