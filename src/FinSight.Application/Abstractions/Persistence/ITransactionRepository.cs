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

    /// <summary>
    /// Gets a transaction by its identifier within a user scope.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transaction when found.</returns>
    Task<Transaction?> GetByIdAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent classified purchase transactions for a merchant and currency.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="merchantId">The normalized merchant identifier.</param>
    /// <param name="currency">The transaction currency.</param>
    /// <param name="limit">Maximum number of transactions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Recent matching transactions.</returns>
    Task<IReadOnlyList<Transaction>> GetByMerchantAsync(
        Guid userId,
        Guid merchantId,
        string currency,
        int limit = 36,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions for a user within a time range.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="from">Inclusive start timestamp.</param>
    /// <param name="toPeriod">Exclusive end timestamp.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching transactions.</returns>
    Task<IReadOnlyList<Transaction>> GetForPeriodAsync(
        Guid userId,
        DateTimeOffset from,
        DateTimeOffset toPeriod,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets previous transactions for the same merchant.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="merchantId">The merchant identifier.</param>
    /// <param name="before">Only transactions before this timestamp.</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="limit">Maximum records.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Previous merchant transactions.</returns>
    Task<IReadOnlyList<Transaction>> GetPreviousForMerchantAsync(
        Guid userId,
        Guid merchantId,
        DateTimeOffset before,
        string currency,
        int limit = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions that are close enough to a candidate
    /// transaction to evaluate for duplication.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="accountId">The financial account.</param>
    /// <param name="transactionId">
    /// The transaction to exclude from results.
    /// </param>
    /// <param name="amount">The signed transaction amount.</param>
    /// <param name="transactionDate">The transaction date.</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Potential duplicate transactions.</returns>
    Task<IReadOnlyList<Transaction>> FindPotentialDuplicatesAsync(
        Guid userId,
        Guid accountId,
        Guid transactionId,
        decimal amount,
        DateTimeOffset transactionDate,
        string currency,
        CancellationToken cancellationToken = default);
}
