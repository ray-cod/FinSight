using FinSight.Domain.Common;

namespace FinSight.Domain.Categories;

/// <summary>
/// Represents a top-level financial transaction category.
/// </summary>
public sealed class Category : Entity<Guid>
{
    private Category()
    {
    }

    private Category(
        Guid id,
        string code,
        string name,
        CategoryType type)
        : base(id)
    {
        Code = NormalizeCode(code);
        Name = NormalizeName(name);
        Type = type;
        IsActive = true;
    }

    /// <summary>
    /// Gets the stable category code.
    /// </summary>
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the semantic category type.
    /// </summary>
    public CategoryType Type { get; private set; }

    /// <summary>
    /// Gets whether the category is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Creates a new financial category.
    /// </summary>
    /// <param name="code">The stable category code.</param>
    /// <param name="name">The display name.</param>
    /// <param name="type">The category type.</param>
    /// <returns>The new category.</returns>
    public static Category Create(
        string code,
        string name,
        CategoryType type)
    {
        return new Category(
            Guid.NewGuid(),
            code,
            name,
            type);
    }

    /// <summary>
    /// Deactivates the category.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Activates the category.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
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
