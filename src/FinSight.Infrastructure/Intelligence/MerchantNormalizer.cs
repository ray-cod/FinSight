using System.Text.RegularExpressions;
using FinSight.Application.Abstractions.Intelligence;

namespace FinSight.Infrastructure.Intelligence;

/// <summary>
/// Normalizes raw banking descriptions into matching-friendly text.
/// </summary>
public sealed partial class MerchantNormalizer
    : IMerchantNormalizer
{
    /// <inheritdoc />
    public string Normalize(
        string rawDescription)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            rawDescription);

        var normalized =
            rawDescription
                .Trim()
                .ToUpperInvariant();

        normalized =
            DigitSuffixRegex()
                .Replace(normalized, " ");

        normalized =
            NonAlphaNumericRegex()
                .Replace(normalized, " ");

        normalized =
            MultipleSpacesRegex()
                .Replace(normalized, " ")
                .Trim();

        return normalized;
    }

    [GeneratedRegex(
        @"\b\d{3,}\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex DigitSuffixRegex();

    [GeneratedRegex(
        @"[^A-Z0-9]+",
        RegexOptions.CultureInvariant)]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex(
        @"\s+",
        RegexOptions.CultureInvariant)]
    private static partial Regex MultipleSpacesRegex();
}
