namespace FinSight.Application.Abstractions.Observability;

/// <summary>
/// Provides application telemetry for domain metrics and operational monitoring.
/// </summary>
public interface IFinSightTelemetry
{
    /// <summary>
    /// Increments the count of successfully delivered notifications.
    /// </summary>
    /// <param name="value">The count increment. Defaults to 1.</param>
    void IncrementNotificationsDelivered(long value = 1);

    /// <summary>
    /// Increments the count of failed notification delivery attempts.
    /// </summary>
    /// <param name="value">The count increment. Defaults to 1.</param>
    void IncrementNotificationFailures(long value = 1);

    /// <summary>
    /// Increments the count of imported financial transactions.
    /// </summary>
    /// <param name="value">The count increment. Defaults to 1.</param>
    void IncrementTransactionsImported(long value = 1);

    /// <summary>
    /// Increments the count of categorized financial transactions.
    /// </summary>
    /// <param name="value">The count increment. Defaults to 1.</param>
    void IncrementTransactionsCategorized(long value = 1);

    /// <summary>
    /// Increments the count of detected financial anomalies.
    /// </summary>
    /// <param name="value">The number of detected anomalies to add.</param>
    void IncrementAnomaliesDetected(long value);

    /// <summary>
    /// Increments the count of detected recurring subscriptions.
    /// </summary>
    /// <param name="value">The count increment. Defaults to 1.</param>
    void IncrementSubscriptionsDetected(long value = 1);

    /// <summary>
    /// Increments the count of generated financial insights.
    /// </summary>
    /// <param name="value">The count increment. Defaults to 1.</param>
    void IncrementInsightsGenerated(long value = 1);

    /// <summary>
    /// Tracks an initiated AI transaction classification request.
    /// </summary>
    void AiClassificationRequest();

    /// <summary>
    /// Tracks a failed AI transaction classification attempt.
    /// </summary>
    void AiClassificationFailure();

    /// <summary>
    /// Records the processing duration of an AI transaction classification operation.
    /// </summary>
    /// <param name="milliseconds">The execution time in milliseconds.</param>
    void AiClassificationDuration(double milliseconds);
}
