using FinSight.Domain.Merchants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for merchant aliases.
/// </summary>
public sealed class MerchantAliasConfiguration
    : IEntityTypeConfiguration<MerchantAlias>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<MerchantAlias> builder)
    {
        builder.ToTable("merchant_aliases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Alias)
            .HasMaxLength(300)
            .IsRequired();

        builder.HasIndex(x => x.Alias)
            .IsUnique();

        builder.HasIndex(x => x.MerchantId);
    }
}
