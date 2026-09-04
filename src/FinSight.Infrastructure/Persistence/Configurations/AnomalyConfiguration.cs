using FinSight.Domain.Anomalies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for financial anomalies.
/// </summary>
public sealed class AnomalyConfiguration
    : IEntityTypeConfiguration<Anomaly>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<Anomaly> builder)
    {
        builder.ToTable("anomalies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TransactionId)
            .IsRequired();

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.Severity)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Score)
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(x => x.Confidence)
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Evidence)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(
            x => new
            {
                x.TransactionId,
                x.Type
            })
            .IsUnique();

        builder.HasIndex(
            x => new
            {
                x.UserId,
                x.Status,
                x.DetectedAt
            });

        builder.HasIndex(
            x => new
            {
                x.UserId,
                x.Severity,
                x.DetectedAt
            });
    }
}
