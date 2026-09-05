using FinSight.Application.Abstractions.Intelligence;
using FinSight.Domain.Anomalies;
using FinSight.Domain.Insights;

namespace FinSight.Infrastructure.Intelligence;

/// <summary>
/// Generates deterministic user-readable financial insights from anomalies.
/// </summary>
public sealed class InsightGenerator
    : IInsightGenerator
{
    /// <inheritdoc />
    public FinancialInsight Generate(
        Anomaly anomaly)
    {
        var type =
            anomaly.Type switch
            {
                AnomalyType.LargeTransaction =>
                    InsightType.UnusualTransaction,

                AnomalyType.MerchantSpendingSpike =>
                    InsightType.MerchantSpendingIncrease,

                AnomalyType.CategorySpendingSpike =>
                    InsightType.CategorySpendingIncrease,

                AnomalyType.NewMerchant =>
                    InsightType.NewMerchant,

                AnomalyType.DuplicateTransaction =>
                    InsightType.PossibleDuplicate,

                _ =>
                    InsightType.UnusualTransaction
            };

        var severity =
            anomaly.Severity switch
            {
                AnomalySeverity.Critical =>
                    InsightSeverity.High,

                AnomalySeverity.High =>
                    InsightSeverity.High,

                AnomalySeverity.Medium =>
                    InsightSeverity.Medium,

                _ =>
                    InsightSeverity.Low
            };

        return FinancialInsight.Create(
            anomaly.UserId,
            anomaly.Id,
            anomaly.TransactionId,
            type,
            severity,
            anomaly.Title,
            anomaly.Description,
            anomaly.DetectedAt,
            anomaly.DetectedAt.AddDays(30));
    }
}
