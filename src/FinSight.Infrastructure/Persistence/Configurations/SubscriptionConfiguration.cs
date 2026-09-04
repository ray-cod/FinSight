using FinSight.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for detected subscriptions.
/// </summary>
public sealed class SubscriptionConfiguration
    : IEntityTypeConfiguration<Subscription>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.MerchantId)
            .IsRequired();

        builder.Property(x => x.MerchantName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Frequency)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CurrentAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.AverageAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.DetectionConfidence)
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(
            x => new
            {
                x.UserId,
                x.MerchantId,
                x.Currency
            })
            .IsUnique();

        builder.HasIndex(
            x => new
            {
                x.UserId,
                x.Status
            });

        builder.HasIndex(
            x => x.NextExpectedChargeAt);
    }
}
