using FinSight.Application.Abstractions.Persistence;
using FinSight.Application.Features.Accounts;

namespace FinSight.Application.Features.Institutions;

/// <summary>
/// Provides supported financial institution operations.
/// </summary>
public sealed class InstitutionService(
    IInstitutionRepository repository)
{
    /// <summary>
    /// Gets all supported active institutions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The supported institutions.</returns>
    public async Task<IReadOnlyList<InstitutionResponse>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var institutions =
            await repository.GetActiveAsync(
                cancellationToken);

        return institutions
            .Select(
                institution =>
                    new InstitutionResponse(
                        institution.Id,
                        institution.ProviderCode,
                        institution.Name,
                        institution.IsActive))
            .ToArray();
    }
}
