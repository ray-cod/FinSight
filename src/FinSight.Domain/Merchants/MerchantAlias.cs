using FinSight.Domain.Common;

namespace FinSight.Domain.Merchants;

/// <summary>
/// Represents a known raw description alias for a normalized merchant.
/// </summary>
public sealed class MerchantAlias : Entity<Guid>
{
    private MerchantAlias()
    {
    }

    private MerchantAlias(
        Guid id,
        Guid merchantId,
        string alias)
        : base(id)
    {
        MerchantId = merchantId;
        Alias = Normalize(alias);
    }

    /// <summary>
    /// Gets the normalized merchant identifier.
    /// </summary>
    public Guid MerchantId { get; private set; }

    /// <summary>
    /// Gets the normalized alias string.
    /// </summary>
    public string Alias { get; private set; } = null!;

    /// <summary>
    /// Creates a merchant alias.
    /// </summary>
    /// <param name="merchantId">The merchant identifier.</param>
    /// <param name="alias">The raw transaction alias.</param>
    /// <returns>The new merchant alias.</returns>
    public static MerchantAlias Create(
        Guid merchantId,
        string alias)
    {
        if (merchantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Merchant identifier cannot be empty.",
                nameof(merchantId));
        }

        return new MerchantAlias(
            Guid.NewGuid(),
            merchantId,
            alias);
    }

    private static string Normalize(
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant();
    }
}
