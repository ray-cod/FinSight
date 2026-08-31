using System.Text.Json;
using FinSight.Application.Abstractions.Caching;
using FinSight.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FinSight.Infrastructure.Caching.Redis;

/// <summary>
/// Provides a Redis-backed distributed caching implementation of <see cref="ICacheService"/>.
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IDatabase _database;
    private readonly string _instanceName;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheService"/> class.
    /// </summary>
    /// <param name="connection">The Redis connection multiplexer.</param>
    /// <param name="options">The Redis configuration options.</param>
    public RedisCacheService(
        IConnectionMultiplexer connection,
        IOptions<RedisOptions> options)
    {
        _database = connection.GetDatabase();
        _instanceName = options.Value.InstanceName;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var value = await _database.StringGetAsync(
            BuildKey(key));

        if (!value.HasValue)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(
            (string)value!,
            JsonOptions);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var serialized = JsonSerializer.Serialize(
            value,
            JsonOptions);

        await _database.StringSetAsync(
            BuildKey(key),
            serialized,
            expiry: expiration.HasValue ? expiration.Value : default);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _database.KeyDeleteAsync(
            BuildKey(key));
    }

    private RedisKey BuildKey(string key)
    {
        return $"{_instanceName}{key}";
    }
}
