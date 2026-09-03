using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Merchants;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements normalized merchant persistence.
/// </summary>
public sealed class MerchantRepository(
    FinSightDbContext dbContext)
    : IMerchantRepository
{
    /// <inheritdoc />
    public async Task<Merchant?> FindByAliasAsync(
        string normalizedAlias,
        CancellationToken cancellationToken = default)
    {
        var normalized =
            normalizedAlias.Trim().ToUpperInvariant();

        return await dbContext.Set<MerchantAlias>()
            .AsNoTracking()
            .Where(x => x.Alias == normalized)
            .Select(x => x.MerchantId)
            .Join(
                dbContext.Set<Merchant>(),
                merchantId => merchantId,
                merchant => merchant.Id,
                (_, merchant) => merchant)
            .SingleOrDefaultAsync(
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<Merchant?> FindByCanonicalNameAsync(
        string canonicalName,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Merchant>()
            .SingleOrDefaultAsync(
                x =>
                    x.CanonicalName ==
                    canonicalName,
                cancellationToken);
    }

    /// <inheritdoc />
    public void Add(
        Merchant merchant)
    {
        dbContext.Set<Merchant>()
            .Add(merchant);
    }

    /// <inheritdoc />
    public void AddAlias(
        MerchantAlias merchantAlias)
    {
        dbContext.Set<MerchantAlias>()
            .Add(merchantAlias);
    }

    /// <inheritdoc />
    public Task<Merchant?> GetByIdAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Merchant>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == merchantId,
                cancellationToken);
    }
}
