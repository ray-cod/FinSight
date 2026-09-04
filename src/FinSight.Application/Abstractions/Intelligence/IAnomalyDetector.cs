using FinSight.Domain.Anomalies;

namespace FinSight.Application.Abstractions.Intelligence;

/// <summary>
/// Detects unusual behavior in financial transaction history.
/// </summary>
public interface IAnomalyDetector
{
    /// <summary>
    /// Evaluates a transaction for known anomaly patterns.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="transactionId">The transaction being evaluated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detected anomalies.</returns>
    Task<IReadOnlyList<AnomalyDetectionResult>> DetectAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a candidate anomaly detected by the analysis engine.
/// </summary>
public sealed record AnomalyDetectionResult(
    AnomalyType Type,
    AnomalySeverity Severity,
    decimal Score,
    decimal Confidence,
    string Title,
    string Description,
    string Evidence);
