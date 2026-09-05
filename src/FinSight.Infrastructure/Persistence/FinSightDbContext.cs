using FinSight.Domain.Accounts;
using FinSight.Domain.Anomalies;
using FinSight.Domain.Auditing;
using FinSight.Domain.Common;
using FinSight.Domain.Insights;
using FinSight.Domain.Notifications;
using FinSight.Domain.Outbox;
using FinSight.Domain.Transactions;
using FinSight.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for FinSight.
/// </summary>
public sealed class FinSightDbContext(
    DbContextOptions<FinSightDbContext> options)
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid>(options)
{
    /// <summary>
    /// Gets the persisted refresh tokens.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    /// <summary>
    /// Gets persisted financial anomalies.
    /// </summary>
    public DbSet<Anomaly> Anomalies =>
        Set<Anomaly>();

    /// <summary>
    /// Gets persisted financial insights.
    /// </summary>
    public DbSet<FinancialInsight> FinancialInsights =>
        Set<FinancialInsight>();

    /// <summary>
    /// Gets persisted security audit events.
    /// </summary>
    public DbSet<AuditEvent> AuditEvents =>
        Set<AuditEvent>();

    /// <summary>
    /// Gets persisted transactional outbox messages.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages =>
        Set<OutboxMessage>();

    /// <summary>
    /// Gets persisted notifications.
    /// </summary>
    public DbSet<Notification> Notifications =>
        Set<Notification>();

    /// <summary>
    /// Gets persisted notification preferences.
    /// </summary>
    public DbSet<NotificationPreference>
        NotificationPreferences =>
        Set<NotificationPreference>();

    /// <summary>
    /// Gets processed integration messages.
    /// </summary>
    public DbSet<ProcessedMessage>
        ProcessedMessages =>
        Set<ProcessedMessage>();

    /// <inheritdoc />
    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Ignore<DomainEvent>();

        builder.Entity<FinancialAccount>()
            .Ignore(x => x.DomainEvents);

        builder.Entity<AccountConnection>()
            .Ignore(x => x.DomainEvents);

        builder.Entity<Transaction>()
            .Ignore(x => x.DomainEvents);

        builder.ApplyConfigurationsFromAssembly(
            typeof(FinSightDbContext).Assembly);

        ConfigureIdentity(builder);
        ConfigureRefreshTokens(builder);
    }

    private static void ConfigureIdentity(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(
            entity =>
            {
                entity.Property(x => x.DisplayName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .IsRequired();
            });
    }

    private static void ConfigureRefreshTokens(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(
            entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.TokenHash)
                    .HasMaxLength(128)
                    .IsRequired();

                entity.Property(x => x.CreatedByIp)
                    .HasMaxLength(64);

                entity.HasIndex(x => x.TokenHash)
                    .IsUnique();

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.RevokedAt,
                    x.ExpiresAt
                });
            });
    }
}
