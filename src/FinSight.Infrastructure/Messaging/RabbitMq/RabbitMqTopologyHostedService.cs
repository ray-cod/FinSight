using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinSight.Infrastructure.Messaging.RabbitMq;

/// <summary>
/// Background hosted service that ensures RabbitMQ exchanges and queues are declared at application startup.
/// </summary>
/// <param name="connectionProvider">The provider used to obtain an active RabbitMQ connection.</param>
/// <param name="logger">The logger instance for messaging topology lifecycle events.</param>
public sealed partial class RabbitMqTopologyHostedService(
    IRabbitMqConnectionProvider connectionProvider,
    ILogger<RabbitMqTopologyHostedService> logger)
    : IHostedService
{
    /// <summary>
    /// Asynchronously executes startup routines to declare RabbitMQ topology components.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that signals when startup has been aborted.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous startup operation.</returns>
    public async Task StartAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var connection =
                await connectionProvider.GetConnectionAsync(
                    cancellationToken);

            await RabbitMqTopology.InitializeAsync(
                connection,
                cancellationToken);

            LogTopologyInitialized(logger);
        }
        catch (Exception exception)
        {
            LogTopologyInitializationFailed(logger, exception);

            throw;
        }
    }

    /// <summary>
    /// Asynchronously handles service shutdown signals.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that signals when shutdown must be forced.</param>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "RabbitMQ topology initialized successfully.")]
    private static partial void LogTopologyInitialized(
        ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to initialize RabbitMQ topology.")]
    private static partial void LogTopologyInitializationFailed(
        ILogger logger,
        Exception exception);
}
