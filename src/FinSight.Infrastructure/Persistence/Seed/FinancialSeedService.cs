using FinSight.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds financial institution data required by FinSight development environments.
/// </summary>
public sealed class FinancialSeedService(
    FinSightDbContext dbContext)
{
    /// <summary>
    /// Ensures supported institutions exist.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Set<Institution>()
            .AnyAsync(
                cancellationToken))
        {
            return;
        }

        dbContext.Set<Institution>().AddRange(
            Institution.Create(
                "MOCK_BANK",
                "FinSight Demo Bank"));

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
