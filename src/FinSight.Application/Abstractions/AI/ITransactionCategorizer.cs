using FinSight.Domain.Categories;
using FinSight.Domain.Merchants;

namespace FinSight.Application.Abstractions.AI;

/// <summary>
/// Provides AI-based transaction categorization.
/// </summary>
public interface ITransactionCategorizer
{
    /// <summary>
    /// Categorizes a normalized transaction.
    /// </summary>
    /// <param name="request">Transaction classification input.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The classification result.</returns>
    Task<TransactionClassificationResult> CategorizeAsync(
        TransactionClassificationRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Contains the financial information supplied to the categorizer.
/// </summary>
public sealed record TransactionClassificationRequest(
    string RawDescription,
    string NormalizedDescription,
    decimal Amount,
    string Currency,
    string TransactionType,
    IReadOnlyCollection<CategoryClassificationOption> Categories);

/// <summary>
/// Represents a category available to the classification model.
/// </summary>
public sealed record CategoryClassificationOption(
    Guid CategoryId,
    string CategoryCode,
    string CategoryName,
    string? SubcategoryCode,
    string? SubcategoryName);

/// <summary>
/// Represents the classification returned by the intelligence pipeline.
/// </summary>
public sealed record TransactionClassificationResult(
    string Merchant,
    string CategoryCode,
    string? SubcategoryCode,
    decimal Confidence,
    string ClassificationRationale);
