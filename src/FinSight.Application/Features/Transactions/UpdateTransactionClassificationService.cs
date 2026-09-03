using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Transactions;

namespace FinSight.Application.Features.Transactions;

/// <summary>
/// Applies explicit user corrections to transaction classifications.
/// </summary>
public sealed class UpdateTransactionClassificationService(
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Applies a user-selected category to a transaction.
    /// </summary>
    /// <param name="userId">The owning user identifier.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="categoryId">The category identifier.</param>
    /// <param name="subcategoryId">
    /// The optional subcategory identifier.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task CorrectAsync(
        Guid userId,
        Guid transactionId,
        Guid categoryId,
        Guid? subcategoryId,
        CancellationToken cancellationToken = default)
    {
        var transaction =
            await transactionRepository
                .GetByIdAsync(
                    userId,
                    transactionId,
                    cancellationToken);

        if (transaction is null)
        {
            throw new KeyNotFoundException(
                "Transaction was not found.");
        }

        var category =
            await categoryRepository
                .GetActiveCategoriesAsync(
                    cancellationToken);

        if (!category.Any(
            x => x.Id == categoryId))
        {
            throw new ArgumentException(
                "Invalid category identifier.",
                nameof(categoryId));
        }

        if (subcategoryId.HasValue)
        {
            var subcategories =
                await categoryRepository
                    .GetActiveSubcategoriesAsync(
                        cancellationToken);

            var subcategory =
                subcategories.FirstOrDefault(
                    x => x.Id == subcategoryId.Value);

            if (subcategory is null ||
                subcategory.CategoryId != categoryId)
            {
                throw new ArgumentException(
                    "Invalid subcategory identifier.",
                    nameof(subcategoryId));
            }
        }

        transaction.ApplyUserCorrection(
            transaction.MerchantId,
            categoryId,
            subcategoryId);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}

/// <summary>
/// Represents a user classification correction.
/// </summary>
public sealed record UpdateTransactionClassificationRequest(
    Guid CategoryId,
    Guid? SubcategoryId);
