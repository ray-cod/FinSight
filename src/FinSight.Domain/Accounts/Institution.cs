using FinSight.Domain.Common;

namespace FinSight.Domain.Accounts;

/// <summary>
/// Represents a financial institution supported by FinSight.
/// </summary>
public sealed class Institution : Entity<Guid>
{
    private Institution()
    {
    }

    private Institution(
        Guid id,
        string providerCode,
        string name)
        : base(id)
    {
        ProviderCode = NormalizeCode(providerCode);
        Name = NormalizeName(name);
        IsActive = true;
    }

    /// <summary>
    /// Gets the stable provider identifier used by the banking integration.
    /// </summary>
    public string ProviderCode { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of the financial institution.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether this institution can be connected.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Creates a new financial institution.
    /// </summary>
    /// <param name="providerCode">The external provider identifier.</param>
    /// <param name="name">The institution name.</param>
    /// <returns>A new institution.</returns>
    public static Institution Create(
        string providerCode,
        string name)
    {
        return new Institution(
            Guid.NewGuid(),
            providerCode,
            name);
    }

    /// <summary>
    /// Deactivates this institution.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Activates this institution.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    private static string NormalizeCode(
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant();
    }

    private static string NormalizeName(
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim();
    }
}
