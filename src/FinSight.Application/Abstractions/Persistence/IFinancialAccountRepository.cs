using FinSight.Domain.Accounts;

namespace FinSight.Application.Abstractions.Persistence;

/// <summary>
/// Provides persistence operations for financial accounts.
/// </summary>
public interface IFinancialAccountRepository
{
    /// <summary>
    /// Gets all accounts belonging to a user.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's accounts.</returns>
    Task<IReadOnlyList<FinancialAccount>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's account by identifier.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The account when found.</returns>
    Task<FinancialAccount?> GetByIdAsync(
        Guid userId,
        Guid accountId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an account by its provider identifier.
    /// </summary>
    /// <param name="connectionId">The connection identifier.</param>
    /// <param name="externalAccountId">
    /// The provider account identifier.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching account, if any.</returns>
    Task<FinancialAccount?> GetByExternalIdAsync(
        Guid connectionId,
        string externalAccountId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new financial account.
    /// </summary>
    /// <param name="account">The account to add.</param>
    void Add(FinancialAccount account);
}
