using FinSight.Application.Abstractions.AI;

namespace FinSight.Application.Abstractions.Intelligence;

/// <summary>
/// Applies deterministic transaction categorization rules.
/// </summary>
public interface ICategoryRuleEngine
{
    /// <summary>
    /// Attempts to classify a transaction without using AI.
    /// </summary>
    /// <param name="normalizedDescription">
    /// Normalized transaction description.
    /// </param>
    /// <returns>A deterministic classification when one is available.</returns>
    TransactionRuleResult? TryClassify(
        string normalizedDescription);
}

/// <summary>
/// Represents a deterministic transaction classification.
/// </summary>
public sealed record TransactionRuleResult(
    string Merchant,
    string CategoryCode,
    string? SubcategoryCode,
    decimal Confidence);
