using Microsoft.EntityFrameworkCore;

namespace FinSight.Infrastructure.Persistence;

/// <summary>
/// The primary Entity Framework Core database context for the FinSight application.
/// </summary>
/// <param name="options">The options to be used by this <see cref="DbContext"/>.</param>
public sealed class FinSightDbContext(
    DbContextOptions<FinSightDbContext> options)
    : DbContext(options)
{
    /// <inheritdoc />
    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FinSightDbContext).Assembly);
    }
}
