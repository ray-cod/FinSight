using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implements financial category persistence.
/// </summary>
public sealed class CategoryRepository(
    FinSightDbContext dbContext)
    : ICategoryRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Category>>
        GetActiveCategoriesAsync(
            CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Category>()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Subcategory>>
        GetActiveSubcategoriesAsync(
            CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Subcategory>()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Category?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var normalized =
            code.Trim().ToUpperInvariant();

        return dbContext.Set<Category>()
            .SingleOrDefaultAsync(
                x => x.Code == normalized,
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<Subcategory?>
        GetSubcategoryByCodeAsync(
            string code,
            CancellationToken cancellationToken = default)
    {
        var normalized =
            code.Trim().ToUpperInvariant();

        return dbContext.Set<Subcategory>()
            .SingleOrDefaultAsync(
                x => x.Code == normalized,
                cancellationToken);
    }
}
