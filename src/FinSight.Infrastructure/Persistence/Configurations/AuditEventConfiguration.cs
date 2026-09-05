using FinSight.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinSight.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persisted security audit events.
/// </summary>
public sealed class AuditEventConfiguration
    : IEntityTypeConfiguration<AuditEvent>
{
    /// <inheritdoc />
    public void Configure(
        EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IpAddress)
            .HasMaxLength(64);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(100);

        builder.Property(x => x.TraceId)
            .HasMaxLength(100);

        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb");

        builder.HasIndex(
            x => new
            {
                x.UserId,
                x.OccurredAt
            });

        builder.HasIndex(
            x => new
            {
                x.EventType,
                x.OccurredAt
            });
    }
}
