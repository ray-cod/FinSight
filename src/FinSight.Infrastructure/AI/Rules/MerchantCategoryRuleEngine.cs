using FinSight.Application.Abstractions.Intelligence;

namespace FinSight.Infrastructure.AI.Rules;

/// <summary>
/// Applies deterministic merchant and category rules.
/// </summary>
public sealed class MerchantCategoryRuleEngine
    : ICategoryRuleEngine
{
    private static readonly Rule[] Rules =
    [
        new(
            "NETFLIX",
            "Netflix",
            "ENTERTAINMENT",
            "STREAMING"),

        new(
            "SPOTIFY",
            "Spotify",
            "ENTERTAINMENT",
            "STREAMING"),

        new(
            "AMZN",
            "Amazon",
            "SHOPPING",
            "ONLINE_SHOPPING"),

        new(
            "AMAZON",
            "Amazon",
            "SHOPPING",
            "ONLINE_SHOPPING"),

        new(
            "UBER",
            "Uber",
            "TRANSPORTATION",
            "RIDESHARE"),

        new(
            "JOES COFFEE",
            "Joe's Coffee",
            "FOOD_DINING",
            "COFFEE"),

        new(
            "WOOLWORTHS",
            "Woolworths",
            "FOOD_DINING",
            "GROCERIES")
    ];

    /// <inheritdoc />
    public TransactionRuleResult? TryClassify(
        string normalizedDescription)
    {
        var match =
            Rules.FirstOrDefault(
                rule =>
                    normalizedDescription.Contains(
                        rule.Match,
                        StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            return null;
        }

        return new TransactionRuleResult(
            match.Merchant,
            match.CategoryCode,
            match.SubcategoryCode,
            0.99m);
    }

    private sealed record Rule(
        string Match,
        string Merchant,
        string CategoryCode,
        string? SubcategoryCode);
}
