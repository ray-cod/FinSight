using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements financial institution persistence.
/// </summary>
public sealed class InstitutionRepository(
    FinSightDbContext dbContext)
    : IInstitutionRepository
{
    /// <inheritdoc />
    public async Task<Institution?> GetByProviderCodeAsync(
        string providerCode,
        CancellationToken cancellationToken = default)
    {
        var normalized =
            providerCode.Trim().ToUpperInvariant();

        return await dbContext.Set<Institution>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.ProviderCode == normalized &&
                    x.IsActive,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Institution>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Institution>()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
