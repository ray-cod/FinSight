using FinSight.Domain.Transactions;

namespace FinSight.Application.Abstractions.Persistence;

/// <summary>
/// Provides persistence operations for financial transactions.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Determines whether a transaction already exists.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="providerTransactionId">
    /// Provider transaction identifier.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when the transaction exists.</returns>
    Task<bool> ExistsAsync(
        Guid accountId,
        string providerTransactionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an imported transaction.
    /// </summary>
    /// <param name="transaction">The transaction to add.</param>
    void Add(Transaction transaction);

    /// <summary>
    /// Gets transactions belonging to a user account.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="limit">Maximum records to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's transactions.</returns>
    Task<IReadOnlyList<Transaction>> GetForAccountAsync(
        Guid userId,
        Guid accountId,
        int limit = 100,
        CancellationToken cancellationToken = default);
}
