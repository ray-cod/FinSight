using FinSight.Domain.Insights;

namespace FinSight.Application.Abstractions.Persistence;

/// <summary>
/// Provides persistence operations for financial insights.
/// </summary>
public interface IInsightRepository
{
    /// <summary>
    /// Gets a user's current insights.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="includeDismissed">
    /// Whether dismissed insights should be included.
    /// </param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's insights.</returns>
    Task<IReadOnlyList<FinancialInsight>> GetByUserIdAsync(
        Guid userId,
        bool includeDismissed = false,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an insight within a user ownership scope.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="insightId">The insight identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The insight when found.</returns>
    Task<FinancialInsight?> GetByIdAsync(
        Guid userId,
        Guid insightId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether an insight already exists for an anomaly.
    /// </summary>
    /// <param name="anomalyId">The source anomaly identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if an insight already exists.</returns>
    Task<bool> ExistsForAnomalyAsync(
        Guid anomalyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a financial insight.
    /// </summary>
    /// <param name="insight">The insight.</param>
    void Add(FinancialInsight insight);

    /// <summary>
    /// Gets active insights that have expired.
    /// </summary>
    /// <param name="asOf">The evaluation timestamp.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Expired insights.</returns>
    Task<IReadOnlyList<FinancialInsight>> GetExpiredAsync(
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default);
}
