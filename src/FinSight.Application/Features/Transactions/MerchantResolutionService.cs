using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Merchants;

namespace FinSight.Application.Features.Transactions;

/// <summary>
/// Resolves AI or rule-provided merchant names into persisted merchants.
/// </summary>
public sealed class MerchantResolutionService(
    IMerchantRepository merchantRepository,
    IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Resolves a merchant by canonical name, creating it when required.
    /// </summary>
    /// <param name="merchantName">The canonical merchant name.</param>
    /// <param name="alias">The transaction alias.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The merchant identifier.</returns>
    public async Task<Guid> ResolveAsync(
        string merchantName,
        string alias,
        CancellationToken cancellationToken = default)
    {
        var existingAlias =
            await merchantRepository.FindByAliasAsync(
                alias,
                cancellationToken);

        if (existingAlias is not null)
        {
            return existingAlias.Id;
        }

        var merchant =
            await merchantRepository.FindByCanonicalNameAsync(
                merchantName,
                cancellationToken);

        if (merchant is null)
        {
            merchant =
                Merchant.Create(
                    merchantName);

            merchantRepository.Add(
                merchant);
        }

        var merchantAlias =
            MerchantAlias.Create(
                merchant.Id,
                alias);

        merchantRepository.AddAlias(
            merchantAlias);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return merchant.Id;
    }
}
