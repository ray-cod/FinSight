using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FinSight.Infrastructure.Observability;

/// <summary>
/// Provides application-level tracing and metrics instruments.
/// </summary>
public static class FinSightTelemetry
{
    /// <summary>
    /// Gets the FinSight activity source.
    /// </summary>
    public static readonly ActivitySource ActivitySource =
        new("FinSight");

    /// <summary>
    /// Gets the FinSight meter.
    /// </summary>
    public static readonly Meter Meter =
        new("FinSight");

    /// <summary>
    /// Counts imported transactions.
    /// </summary>
    public static readonly Counter<long>
        TransactionsImported =
        Meter.CreateCounter<long>(
            "finsight.transactions.imported");

    /// <summary>
    /// Counts categorized transactions.
    /// </summary>
    public static readonly Counter<long>
        TransactionsCategorized =
        Meter.CreateCounter<long>(
            "finsight.transactions.categorized");

    /// <summary>
    /// Counts detected anomalies.
    /// </summary>
    public static readonly Counter<long>
        AnomaliesDetected =
        Meter.CreateCounter<long>(
            "finsight.anomalies.detected");

    /// <summary>
    /// Counts detected subscriptions.
    /// </summary>
    public static readonly Counter<long>
        SubscriptionsDetected =
        Meter.CreateCounter<long>(
            "finsight.subscriptions.detected");

    /// <summary>
    /// Counts generated insights.
    /// </summary>
    public static readonly Counter<long>
        InsightsGenerated =
        Meter.CreateCounter<long>(
            "finsight.insights.generated");

    /// <summary>
    /// Counts AI classification requests.
    /// </summary>
    public static readonly Counter<long>
        AiClassificationRequests =
        Meter.CreateCounter<long>(
            "finsight.ai.classification.requests");

    /// <summary>
    /// Counts AI classification failures.
    /// </summary>
    public static readonly Counter<long>
        AiClassificationFailures =
        Meter.CreateCounter<long>(
            "finsight.ai.classification.failures");

    /// <summary>
    /// Measures AI classification duration in milliseconds.
    /// </summary>
    public static readonly Histogram<double>
        AiClassificationDuration =
        Meter.CreateHistogram<double>(
            "finsight.ai.classification.duration",
            "ms");

    /// <summary>
    /// Counts notification deliveries.
    /// </summary>
    public static readonly Counter<long>
        NotificationsDelivered =
        Meter.CreateCounter<long>(
            "finsight.notifications.delivered");

    /// <summary>
    /// Counts notification delivery failures.
    /// </summary>
    public static readonly Counter<long>
        NotificationFailures =
        Meter.CreateCounter<long>(
            "finsight.notifications.failures");

    /// <summary>
    /// Measures event processing duration.
    /// </summary>
    public static readonly Histogram<double>
        EventProcessingDuration =
        Meter.CreateHistogram<double>(
            "finsight.events.processing.duration",
            "ms");
}
