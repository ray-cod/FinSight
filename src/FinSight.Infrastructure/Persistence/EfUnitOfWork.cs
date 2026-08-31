using FinSight.Application.Abstractions.Persistence;

namespace FinSight.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core implementation of the <see cref="IUnitOfWork"/> interface.
/// </summary>
/// <param name="dbContext">The database context instance used to persist entity state changes.</param>
public sealed class EfUnitOfWork(
    FinSightDbContext dbContext)
    : IUnitOfWork
{
    /// <inheritdoc />
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
