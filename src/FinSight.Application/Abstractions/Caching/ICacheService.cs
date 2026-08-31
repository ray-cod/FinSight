namespace FinSight.Application.Abstractions.Caching;

/// <summary>
/// Defines a contract for distributed key-value caching operations.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Asynchronously retrieves an item from the cache by its key.
    /// </summary>
    /// <typeparam name="T">The expected type of the cached item.</typeparam>
    /// <param name="key">The unique cache key identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The cached item of type <typeparamref name="T"/> if found; otherwise, <see langword="null"/>.</returns>
    Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously sets an item in the cache with an optional expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the item being stored.</typeparam>
    /// <param name="key">The unique cache key identifier.</param>
    /// <param name="value">The object instance to cache.</param>
    /// <param name="expiration">An optional relative expiration duration for the cache entry.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously removes an item from the cache by its key.
    /// </summary>
    /// <param name="key">The unique cache key identifier of the item to remove.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);
}
