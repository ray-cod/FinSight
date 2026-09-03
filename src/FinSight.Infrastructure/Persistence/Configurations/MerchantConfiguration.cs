using FinSight.Domain.Merchants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for normalized merchants.
/// </summary>
public sealed class MerchantConfiguration
    : IEntityTypeConfiguration<Merchant>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("merchants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CanonicalName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.CanonicalName)
            .IsUnique();
    }
}
