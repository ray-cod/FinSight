using FinSight.Domain.Merchants;

namespace FinSight.Application.Abstractions.Persistence;

/// <summary>
/// Provides normalized merchant persistence operations.
/// </summary>
public interface IMerchantRepository
{
    /// <summary>
    /// Finds a merchant by alias.
    /// </summary>
    /// <param name="normalizedAlias">The normalized alias.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching merchant when available.</returns>
    Task<Merchant?> FindByAliasAsync(
        string normalizedAlias,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a merchant by canonical name.
    /// </summary>
    /// <param name="canonicalName">The canonical merchant name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The merchant when available.</returns>
    Task<Merchant?> FindByCanonicalNameAsync(
        string canonicalName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new merchant.
    /// </summary>
    /// <param name="merchant">The merchant to add.</param>
    void Add(Merchant merchant);

    /// <summary>
    /// Adds a merchant alias.
    /// </summary>
    /// <param name="merchantAlias">The merchant alias to add.</param>
    void AddAlias(MerchantAlias merchantAlias);
}
