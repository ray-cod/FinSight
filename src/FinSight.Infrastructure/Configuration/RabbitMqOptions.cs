namespace FinSight.Infrastructure.Configuration;

/// <summary>
/// Represents configuration options for connecting to a RabbitMQ message broker.
/// </summary>
public sealed class RabbitMqOptions
{
    /// <summary>
    /// The configuration section name within application settings.
    /// </summary>
    public const string SectionName = "RabbitMq";

    /// <summary>
    /// Gets the hostname or IP address of the RabbitMQ server.
    /// </summary>
    public required string Host { get; init; }

    /// <summary>
    /// Gets the port number for the RabbitMQ service. Defaults to <c>5672</c>.
    /// </summary>
    public int Port { get; init; } = 5672;

    /// <summary>
    /// Gets the username used for RabbitMQ authentication.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets the password used for RabbitMQ authentication.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Gets the virtual host name to connect to within RabbitMQ. Defaults to <c>"/"</c>.
    /// </summary>
    public string VirtualHost { get; init; } = "/";

    /// <summary>
    /// Gets the human-readable client connection name registered with RabbitMQ. Defaults to <c>"finsight"</c>.
    /// </summary>
    public string ConnectionName { get; init; } = "finsight";
}
