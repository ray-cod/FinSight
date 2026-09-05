using FinSight.Domain.Anomalies;
using FinSight.Infrastructure.Intelligence;
using FluentAssertions;

namespace FinSight.Infrastructure.Tests.Anomalies;

/// <summary>
/// Tests conversion of anomalies into financial insights.
/// </summary>
public sealed class InsightGeneratorTests
{
    /// <summary>
    /// Verifies that large transactions become unusual-transaction insights.
    /// </summary>
    [Fact]
    public void LargeTransactionShouldGenerateUnusualTransactionInsight()
    {
        var anomaly =
            Anomaly.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                AnomalyType.LargeTransaction,
                AnomalySeverity.High,
                0.90m,
                0.95m,
                "Unusually large transaction",
                "The transaction is much larger than normal.",
                "Current: 500; average: 100.");

        var generator =
            new InsightGenerator();

        var insight =
            generator.Generate(anomaly);

        insight.Type
            .Should()
            .Be(
                Domain.Insights
                    .InsightType.UnusualTransaction);

        insight.UserId
            .Should()
            .Be(anomaly.UserId);

        insight.AnomalyId
            .Should()
            .Be(anomaly.Id);
    }
}
