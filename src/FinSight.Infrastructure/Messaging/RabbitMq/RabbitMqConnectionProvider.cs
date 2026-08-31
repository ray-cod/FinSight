using FinSight.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FinSight.Infrastructure.Messaging.RabbitMq;

/// <summary>
/// Defines a contract for managing and retrieving an active connection to RabbitMQ.
/// </summary>
public interface IRabbitMqConnectionProvider
{
    /// <summary>
    /// Asynchronously gets an open connection to RabbitMQ, establishing one if necessary.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the connection task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing an active <see cref="IConnection"/>.</returns>
    Task<IConnection> GetConnectionAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides thread-safe connection management and automatic recovery for RabbitMQ.
/// </summary>
/// <param name="options">The configured RabbitMQ connection options.</param>
public sealed class RabbitMqConnectionProvider(
    IOptions<RabbitMqOptions> options)
    : IRabbitMqConnectionProvider, IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;

    private readonly SemaphoreSlim _lock = new(1, 1);

    private IConnection? _connection;

    /// <inheritdoc />
    public async Task<IConnection> GetConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                ClientProvidedName = _options.ConnectionName
            };

            _connection =
                await factory.CreateConnectionAsync(
                    cancellationToken);

            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Asynchronously releases resources used by the <see cref="RabbitMqConnectionProvider"/>.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _lock.Dispose();
    }
}
