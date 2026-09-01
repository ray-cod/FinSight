using FinSight.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
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

    /// <inheritdoc />
    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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
