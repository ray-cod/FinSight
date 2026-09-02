using FinSight.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for financial institutions.
/// </summary>
public sealed class InstitutionConfiguration
    : IEntityTypeConfiguration<Institution>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<Institution> builder)
    {
        builder.ToTable("institutions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.ProviderCode)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();
    }
}
