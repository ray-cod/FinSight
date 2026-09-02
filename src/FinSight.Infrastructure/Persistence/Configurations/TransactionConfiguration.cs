using FinSight.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for financial transactions.
/// </summary>
public sealed class TransactionConfiguration
    : IEntityTypeConfiguration<Transaction>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                transactionId => transactionId.Value,
                value => new TransactionId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.InstitutionId)
            .IsRequired();

        builder.Property(x => x.ProviderTransactionId)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(
            x => new
            {
                x.AccountId,
                x.ProviderTransactionId
            })
            .IsUnique();

        builder.Property(x => x.RawDescription)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.TransactionDate)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Fingerprint)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => x.Fingerprint);

        builder.HasIndex(
            x => new
            {
                x.UserId,
                x.TransactionDate
            });

        builder.Property(x => x.ImportedAt)
            .IsRequired();
    }
}
