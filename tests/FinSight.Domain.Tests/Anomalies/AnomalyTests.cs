using FinSight.Domain.Anomalies;
using FluentAssertions;

namespace FinSight.Domain.Tests.Anomalies;

/// <summary>
/// Tests anomaly domain behavior.
/// </summary>
public sealed class AnomalyTests
{
    /// <summary>
    /// Verifies that an anomaly is created open.
    /// </summary>
    [Fact]
    public void CreateShouldCreateOpenAnomaly()
    {
        var anomaly =
            Anomaly.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                AnomalyType.LargeTransaction,
                AnomalySeverity.High,
                0.92m,
                0.95m,
                "Large transaction",
                "The transaction is unusually large.",
                "Current: 500; average: 100.");

        anomaly.Status
            .Should()
            .Be(AnomalyStatus.Open);

        anomaly.Score
            .Should()
            .Be(0.92m);
    }

    /// <summary>
    /// Verifies that an anomaly can be resolved.
    /// </summary>
    [Fact]
    public void ResolveShouldMarkAnomalyResolved()
    {
        var anomaly =
            Anomaly.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                AnomalyType.NewMerchant,
                AnomalySeverity.Low,
                0.60m,
                0.95m,
                "New merchant",
                "A new merchant was detected.",
                "First observed transaction.");

        anomaly.Resolve();

        anomaly.Status
            .Should()
            .Be(AnomalyStatus.Resolved);

        anomaly.ResolvedAt
            .Should()
            .NotBeNull();
    }

    /// <summary>
    /// Verifies that an anomaly can be dismissed.
    /// </summary>
    [Fact]
    public void DismissShouldMarkAnomalyDismissed()
    {
        var anomaly =
            Anomaly.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                AnomalyType.DuplicateTransaction,
                AnomalySeverity.High,
                0.90m,
                0.90m,
                "Possible duplicate",
                "A duplicate may exist.",
                "One matching transaction.");

        anomaly.Dismiss();

        anomaly.Status
            .Should()
            .Be(
                AnomalyStatus.Dismissed);
    }
}
