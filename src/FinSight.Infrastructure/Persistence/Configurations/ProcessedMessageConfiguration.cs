using FinSight.Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures processed integration-message persistence.
/// </summary>
public sealed class ProcessedMessageConfiguration
    : IEntityTypeConfiguration<ProcessedMessage>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.ToTable(
            "processed_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ConsumerName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(
            x => new
            {
                x.MessageId,
                x.ConsumerName
            })
            .IsUnique();
    }
}
