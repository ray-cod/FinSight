using FinSight.Domain.Common;

namespace FinSight.Domain.Categories;

/// <summary>
/// Represents a more specific category beneath a top-level category.
/// </summary>
public sealed class Subcategory : Entity<Guid>
{
    private Subcategory()
    {
    }

    private Subcategory(
        Guid id,
        Guid categoryId,
        string code,
        string name)
        : base(id)
    {
        CategoryId = categoryId;
        Code = NormalizeCode(code);
        Name = NormalizeName(name);
        IsActive = true;
    }

    /// <summary>
    /// Gets the parent category identifier.
    /// </summary>
    public Guid CategoryId { get; private set; }

    /// <summary>
    /// Gets the stable subcategory code.
    /// </summary>
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets whether the subcategory is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Creates a new subcategory.
    /// </summary>
    /// <param name="categoryId">The parent category identifier.</param>
    /// <param name="code">The stable subcategory code.</param>
    /// <param name="name">The display name.</param>
    /// <returns>The new subcategory.</returns>
    public static Subcategory Create(
        Guid categoryId,
        string code,
        string name)
    {
        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException(
                "Category identifier cannot be empty.",
                nameof(categoryId));
        }

        return new Subcategory(
            Guid.NewGuid(),
            categoryId,
            code,
            name);
    }

    /// <summary>
    /// Deactivates the subcategory.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
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
