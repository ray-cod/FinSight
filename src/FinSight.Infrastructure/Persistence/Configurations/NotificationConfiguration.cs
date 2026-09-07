using FinSight.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures notification persistence.
/// </summary>
public sealed class NotificationConfiguration
    : IEntityTypeConfiguration<Notification>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.Channel)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(x => x.DeduplicationKey)
            .HasMaxLength(300);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(2000);

        builder.HasIndex(
            x => new
            {
                x.UserId,
                x.Status,
                x.CreatedAt
            });

        builder.HasIndex(
            x => new
            {
                x.UserId,
                x.DeduplicationKey
            });
    }
}
