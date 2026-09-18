using FinSight.Application.Abstractions.Persistence;

namespace FinSight.Application.Features.Transactions;

/// <summary>
/// Coordinates optimized transaction list and detail read operations.
/// </summary>
public sealed class TransactionQueryService(
    ITransactionQueryRepository repository)
{
    /// <summary>
    /// Gets a filtered and paginated transaction list for a user.
    /// </summary>
    /// <param name="userId">The authenticated user identifier.</param>
    /// <param name="request">The requested filters and pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transaction page.</returns>
    public Task<TransactionQueryResult> GetPageAsync(
        Guid userId,
        TransactionListRequest request,
        CancellationToken cancellationToken = default)
    {
        return repository.GetPageAsync(
            userId,
            request.ToQuery(),
            cancellationToken);
    }

    /// <summary>
    /// Gets full transaction details for a user-owned transaction.
    /// </summary>
    /// <param name="userId">The authenticated user identifier.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The detail read model when found.</returns>
    public Task<TransactionDetailResponse?> GetDetailAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        return repository.GetDetailAsync(
            userId,
            transactionId,
            cancellationToken);
    }
}
