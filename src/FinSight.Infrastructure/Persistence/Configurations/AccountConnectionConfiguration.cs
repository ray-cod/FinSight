using FinSight.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for account connections.
/// </summary>
public sealed class AccountConnectionConfiguration
    : IEntityTypeConfiguration<AccountConnection>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<AccountConnection> builder)
    {
        builder.ToTable("account_connections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.InstitutionId)
            .IsRequired();

        builder.Property(x => x.ExternalConnectionId)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(x => x.ExternalConnectionId)
            .IsUnique();

        builder.HasIndex(
            x => new
            {
                x.UserId,
                x.InstitutionId
            });

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.LastSyncError)
            .HasMaxLength(1000);

        builder.Property(x => x.SyncCursor)
            .HasMaxLength(500);
    }
}
