using FinSight.Domain.Insights;
using FluentAssertions;

namespace FinSight.Domain.Tests.Insights;

/// <summary>
/// Tests financial insight domain behavior.
/// </summary>
public sealed class FinancialInsightTests
{
    /// <summary>
    /// Verifies that an insight is created active.
    /// </summary>
    [Fact]
    public void CreateShouldCreateActiveInsight()
    {
        var insight =
            FinancialInsight.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                InsightType.UnusualTransaction,
                InsightSeverity.High,
                "Unusual transaction",
                "This transaction is unusually large.",
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(30));

        insight.Status
            .Should()
            .Be(InsightStatus.Active);
    }

    /// <summary>
    /// Verifies that an insight can be dismissed.
    /// </summary>
    [Fact]
    public void DismissShouldMarkInsightDismissed()
    {
        var insight =
            FinancialInsight.Create(
                Guid.NewGuid(),
                null,
                Guid.NewGuid(),
                InsightType.NewMerchant,
                InsightSeverity.Low,
                "New merchant",
                "A new merchant was detected.",
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddDays(30));

        insight.Dismiss();

        insight.Status
            .Should()
            .Be(InsightStatus.Dismissed);
    }
}
