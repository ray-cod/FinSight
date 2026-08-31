namespace FinSight.Infrastructure.Configuration;

/// <summary>
/// Represents configuration options for Redis caching and connection settings.
/// </summary>
public sealed class RedisOptions
{
    /// <summary>
    /// The configuration section name within application settings.
    /// </summary>
    public const string SectionName = "Redis";

    /// <summary>
    /// Gets the connection string used to connect to the Redis server cluster or instance.
    /// </summary>
    public required string ConnectionString { get; init; }

    /// <summary>
    /// Gets the key prefix instance name used to isolate application keys stored in Redis. Defaults to <c>"FinSight:"</c>.
    /// </summary>
    public string InstanceName { get; init; } = "FinSight:";
}
