using FinSight.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for financial accounts.
/// </summary>
public sealed class FinancialAccountConfiguration
    : IEntityTypeConfiguration<FinancialAccount>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<FinancialAccount> builder)
    {
        builder.ToTable("financial_accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                accountId => accountId.Value,
                value => new AccountId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ConnectionId)
            .IsRequired();

        builder.Property(x => x.InstitutionId)
            .IsRequired();

        builder.Property(x => x.ExternalAccountId)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(
            x => new
            {
                x.ConnectionId,
                x.ExternalAccountId
            })
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.CurrentBalance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.AvailableBalance)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);
    }
}
