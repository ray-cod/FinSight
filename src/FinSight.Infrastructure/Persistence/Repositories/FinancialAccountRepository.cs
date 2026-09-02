using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements financial-account persistence with Entity Framework Core.
/// </summary>
public sealed class FinancialAccountRepository(
    FinSightDbContext dbContext)
    : IFinancialAccountRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<FinancialAccount>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<FinancialAccount>()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FinancialAccount?> GetByIdAsync(
        Guid userId,
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<FinancialAccount>()
            .SingleOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.Id.Value == accountId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FinancialAccount?> GetByExternalIdAsync(
        Guid connectionId,
        string externalAccountId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<FinancialAccount>()
            .SingleOrDefaultAsync(
                x =>
                    x.ConnectionId == connectionId &&
                    x.ExternalAccountId ==
                    externalAccountId,
                cancellationToken);
    }

    /// <inheritdoc />
    public void Add(
        FinancialAccount account)
    {
        dbContext.Set<FinancialAccount>()
            .Add(account);
    }
}
