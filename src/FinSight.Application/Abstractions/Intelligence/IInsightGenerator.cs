using FinSight.Domain.Insights;

namespace FinSight.Application.Abstractions.Intelligence;

/// <summary>
/// Converts detected financial signals into user-readable insights.
/// </summary>
public interface IInsightGenerator
{
    /// <summary>
    /// Generates an insight from an anomaly.
    /// </summary>
    /// <param name="anomaly">The source anomaly.</param>
    /// <returns>A generated financial insight.</returns>
    FinancialInsight Generate(
        Domain.Anomalies.Anomaly anomaly);
}
