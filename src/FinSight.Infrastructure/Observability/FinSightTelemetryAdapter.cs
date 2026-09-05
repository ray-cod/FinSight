using FinSight.Application.Abstractions.Observability;

namespace FinSight.Infrastructure.Observability;

/// <summary>
/// Adapts domain telemetry calls to underlying metric meters and counters.
/// </summary>
public sealed class FinSightTelemetryAdapter : IFinSightTelemetry
{
    /// <inheritdoc />
    public void IncrementNotificationsDelivered(long value = 1)
    {
        FinSightTelemetry.NotificationsDelivered.Add(value);
    }

    /// <inheritdoc />
    public void IncrementNotificationFailures(long value = 1)
    {
        FinSightTelemetry.NotificationFailures.Add(value);
    }

    /// <inheritdoc />
    public void IncrementTransactionsImported(long value = 1)
    {
        FinSightTelemetry.TransactionsImported.Add(value);
    }

    /// <inheritdoc />
    public void IncrementTransactionsCategorized(long value = 1)
    {
        FinSightTelemetry.TransactionsCategorized.Add(value);
    }

    /// <inheritdoc />
    public void IncrementAnomaliesDetected(long value)
    {
        FinSightTelemetry.AnomaliesDetected.Add(value);
    }

    /// <inheritdoc />
    public void IncrementSubscriptionsDetected(long value = 1)
    {
        FinSightTelemetry.SubscriptionsDetected.Add(value);
    }

    /// <inheritdoc />
    public void IncrementInsightsGenerated(long value = 1)
    {
        FinSightTelemetry.InsightsGenerated.Add(value);
    }

    /// <inheritdoc />
    public void AiClassificationRequest()
    {
        FinSightTelemetry.AiClassificationRequests.Add(1);
    }

    /// <inheritdoc />
    public void AiClassificationFailure()
    {
        FinSightTelemetry.AiClassificationFailures.Add(1);
    }

    /// <inheritdoc />
    public void AiClassificationDuration(double milliseconds)
    {
        FinSightTelemetry.AiClassificationDuration.Record(milliseconds);
    }
}
