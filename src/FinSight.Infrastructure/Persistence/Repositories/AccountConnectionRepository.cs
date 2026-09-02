using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements account-connection persistence with Entity Framework Core.
/// </summary>
public sealed class AccountConnectionRepository(
    FinSightDbContext dbContext)
    : IAccountConnectionRepository
{
    /// <inheritdoc />
    public async Task<AccountConnection?> GetByIdAsync(
        Guid userId,
        Guid connectionId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<AccountConnection>()
            .SingleOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.Id == connectionId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AccountConnection>> GetActiveAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<AccountConnection>()
            .Where(
                x =>
                    x.UserId == userId &&
                    x.Status !=
                    ConnectionStatus.Disconnected)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AccountConnection?> GetByExternalIdAsync(
        string externalConnectionId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<AccountConnection>()
            .SingleOrDefaultAsync(
                x =>
                    x.ExternalConnectionId ==
                    externalConnectionId,
                cancellationToken);
    }

    /// <inheritdoc />
    public void Add(
        AccountConnection connection)
    {
        dbContext.Set<AccountConnection>()
            .Add(connection);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AccountConnection>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<AccountConnection>()
            .Where(
                x =>
                    x.Status == ConnectionStatus.Connected ||
                    x.Status == ConnectionStatus.Failed)
            .OrderBy(x => x.LastSuccessfulSyncAt)
            .Take(100)
            .ToListAsync(cancellationToken);
    }
}
