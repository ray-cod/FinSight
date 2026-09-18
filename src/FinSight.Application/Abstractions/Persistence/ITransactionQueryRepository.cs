using FinSight.Application.Features.Transactions;

namespace FinSight.Application.Abstractions.Persistence;

/// <summary>
/// Provides optimized read operations for user-owned financial transactions.
/// </summary>
public interface ITransactionQueryRepository
{
    /// <summary>
    /// Gets a filtered, sorted, paginated transaction result for a user.
    /// </summary>
    /// <param name="userId">The authenticated user identifier.</param>
    /// <param name="query">The transaction query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The paginated transaction result.</returns>
    Task<TransactionQueryResult> GetPageAsync(
        Guid userId,
        TransactionQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a transaction read model within the authenticated user's ownership scope.
    /// </summary>
    /// <param name="userId">The authenticated user identifier.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transaction read model when found; otherwise <see langword="null"/>.</returns>
    Task<TransactionDetailResponse?> GetDetailAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default);
}
