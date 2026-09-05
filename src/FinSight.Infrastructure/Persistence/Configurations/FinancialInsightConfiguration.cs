using FinSight.Domain.Insights;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for financial insights.
/// </summary>
public sealed class FinancialInsightConfiguration
    : IEntityTypeConfiguration<FinancialInsight>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<FinancialInsight> builder)
    {
        builder.ToTable("financial_insights");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.AnomalyId);

        builder.Property(x => x.TransactionId);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.Severity)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(1500)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(
            x => x.AnomalyId);

        builder.HasIndex(
            x => new
            {
                x.UserId,
                x.Status,
                x.CreatedAt
            });
    }
}
