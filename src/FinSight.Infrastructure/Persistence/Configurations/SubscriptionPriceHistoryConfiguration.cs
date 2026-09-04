using FinSight.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence for subscription price observations.
/// </summary>
public sealed class SubscriptionPriceHistoryConfiguration
    : IEntityTypeConfiguration<SubscriptionPriceHistory>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<SubscriptionPriceHistory> builder)
    {
        builder.ToTable(
            "subscription_price_history");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubscriptionId)
            .IsRequired();

        builder.Property(x => x.TransactionId)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.ObservedAt)
            .IsRequired();

        builder.HasIndex(
            x => new
            {
                x.SubscriptionId,
                x.TransactionId
            })
            .IsUnique();

        builder.HasIndex(
            x => new
            {
                x.SubscriptionId,
                x.ObservedAt
            });
    }
}
