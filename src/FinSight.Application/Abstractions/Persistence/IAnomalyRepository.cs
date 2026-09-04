using FinSight.Domain.Anomalies;

namespace FinSight.Application.Abstractions.Persistence;

/// <summary>
/// Provides persistence operations for financial anomalies.
/// </summary>
public interface IAnomalyRepository
{
    /// <summary>
    /// Gets anomalies belonging to a user.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="includeResolved">
    /// Whether resolved and dismissed anomalies should be included.
    /// </param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's anomalies.</returns>
    Task<IReadOnlyList<Anomaly>> GetByUserIdAsync(
        Guid userId,
        bool includeResolved = false,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an anomaly by identifier within a user ownership scope.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="anomalyId">The anomaly identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The anomaly when found.</returns>
    Task<Anomaly?> GetByIdAsync(
        Guid userId,
        Guid anomalyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether an anomaly already exists for a transaction and type.
    /// </summary>
    /// <param name="transactionId">The source transaction.</param>
    /// <param name="type">The anomaly type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when already detected.</returns>
    Task<bool> ExistsForTransactionAsync(
        Guid transactionId,
        AnomalyType type,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets open anomalies that have passed their review lifetime.
    /// </summary>
    /// <param name="asOf">The evaluation timestamp.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Expired anomalies.</returns>
    Task<IReadOnlyList<Anomaly>> GetOpenBeforeAsync(
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an anomaly.
    /// </summary>
    /// <param name="anomaly">The anomaly.</param>
    void Add(Anomaly anomaly);
}
