namespace FinSight.Infrastructure.Configuration;

/// <summary>
/// Represents configuration options for database connections.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// The configuration section name within application settings.
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// Gets the connection string used to connect to the database.
    /// </summary>
    public required string ConnectionString { get; init; }
}
