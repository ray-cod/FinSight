using FinSight.Domain.Accounts;

namespace FinSight.Application.Abstractions.Persistence;

/// <summary>
/// Provides persistence operations for supported financial institutions.
/// </summary>
public interface IInstitutionRepository
{
    /// <summary>
    /// Gets an active institution by provider code.
    /// </summary>
    /// <param name="providerCode">The provider code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The institution, when available.</returns>
    Task<Institution?> GetByProviderCodeAsync(
        string providerCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active supported institutions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Supported institutions.</returns>
    Task<IReadOnlyList<Institution>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}
