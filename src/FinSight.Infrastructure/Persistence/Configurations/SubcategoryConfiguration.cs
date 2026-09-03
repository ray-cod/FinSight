using FinSight.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for transaction subcategories.
/// </summary>
public sealed class SubcategoryConfiguration
    : IEntityTypeConfiguration<Subcategory>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<Subcategory> builder)
    {
        builder.ToTable("subcategories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.CategoryId);
    }
}
