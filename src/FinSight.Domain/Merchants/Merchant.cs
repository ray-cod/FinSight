using FinSight.Domain.Common;

namespace FinSight.Domain.Merchants;

/// <summary>
/// Represents a normalized merchant known to FinSight.
/// </summary>
public sealed class Merchant : Entity<Guid>
{
    private Merchant()
    {
    }

    private Merchant(
        Guid id,
        string canonicalName)
        : base(id)
    {
        CanonicalName =
            NormalizeName(canonicalName);

        CreatedAt =
            DateTimeOffset.UtcNow;

        IsActive = true;
    }

    /// <summary>
    /// Gets the normalized merchant name.
    /// </summary>
    public string CanonicalName { get; private set; } = null!;

    /// <summary>
    /// Gets the merchant creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets whether the merchant is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Creates a normalized merchant.
    /// </summary>
    /// <param name="canonicalName">The canonical merchant name.</param>
    /// <returns>The newly created merchant.</returns>
    public static Merchant Create(
        string canonicalName)
    {
        return new Merchant(
            Guid.NewGuid(),
            canonicalName);
    }

    private static string NormalizeName(
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim();
    }
}
