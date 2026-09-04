namespace FinSight.Domain.Anomalies;

/// <summary>
/// Represents the seriousness of an identified anomaly.
/// </summary>
public enum AnomalySeverity
{
    /// <summary>
    /// The anomaly is informational.
    /// </summary>
    Low = 1,

    /// <summary>
    /// The anomaly deserves user attention.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// The anomaly is substantially outside the normal pattern.
    /// </summary>
    High = 3,

    /// <summary>
    /// The anomaly is highly unusual and potentially significant.
    /// </summary>
    Critical = 4
}
