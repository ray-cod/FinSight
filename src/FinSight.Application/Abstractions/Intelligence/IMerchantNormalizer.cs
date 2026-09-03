namespace FinSight.Application.Abstractions.Intelligence;

/// <summary>
/// Normalizes raw banking descriptions for merchant matching.
/// </summary>
public interface IMerchantNormalizer
{
    /// <summary>
    /// Normalizes a raw transaction description.
    /// </summary>
    /// <param name="rawDescription">The raw description.</param>
    /// <returns>A normalized description.</returns>
    string Normalize(
        string rawDescription);
}
