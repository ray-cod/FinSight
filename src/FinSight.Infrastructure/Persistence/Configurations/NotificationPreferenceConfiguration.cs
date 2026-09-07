using FinSight.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures notification preference persistence.
/// </summary>
public sealed class NotificationPreferenceConfiguration
    : IEntityTypeConfiguration<NotificationPreference>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable(
            "notification_preferences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.Property(x => x.InAppEnabled)
            .IsRequired();

        builder.Property(x => x.EmailEnabled)
            .IsRequired();

        builder.Property(
            x =>
                x.AnomalyNotificationsEnabled)
            .IsRequired();

        builder.Property(
            x =>
                x.SubscriptionNotificationsEnabled)
            .IsRequired();

        builder.Property(
            x =>
                x.InsightNotificationsEnabled)
            .IsRequired();
    }
}
