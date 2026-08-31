namespace FinSight.Application.Abstractions.Persistence;

/// <summary>
/// Represents the unit of work pattern for managing transactional operations and committing state changes to persistent storage.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Asynchronously persists all pending entity state changes within the current unit of work to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
