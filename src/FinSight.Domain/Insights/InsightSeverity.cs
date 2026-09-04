namespace FinSight.Domain.Insights;

/// <summary>
/// Represents the importance of a financial insight.
/// </summary>
public enum InsightSeverity
{
    /// <summary>
    /// Informational insight.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Insight worth reviewing.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// Significant insight requiring attention.
    /// </summary>
    High = 3
}
