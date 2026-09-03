using FinSight.Domain.Categories;

namespace FinSight.Application.Abstractions.Persistence;

/// <summary>
/// Provides financial category persistence operations.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Gets all active categories and subcategories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Active classification categories.</returns>
    Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active subcategories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Active subcategories.</returns>
    Task<IReadOnlyList<Subcategory>> GetActiveSubcategoriesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a category by its stable code.
    /// </summary>
    /// <param name="code">The category code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching category.</returns>
    Task<Category?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a subcategory by its stable code.
    /// </summary>
    /// <param name="code">The subcategory code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching subcategory.</returns>
    Task<Subcategory?> GetSubcategoryByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);
}
