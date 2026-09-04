using System.Text;
using System.Text.Json;
using FinSight.Application.Features.Insights;
using FinSight.Contracts.Events;
using FinSight.Infrastructure.Messaging.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinSight.Workers.Consumers;

/// <summary>
/// Converts detected anomalies into user-facing financial insights.
/// </summary>
public sealed partial class AnomalyDetectedConsumer
    : BackgroundService
{
    private readonly IRabbitMqConnectionProvider
        _connectionProvider;

    private readonly IServiceScopeFactory
        _scopeFactory;

    private readonly ILogger<AnomalyDetectedConsumer>
        _logger;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AnomalyDetectedConsumer"/> class.
    /// </summary>
    public AnomalyDetectedConsumer(
        IRabbitMqConnectionProvider connectionProvider,
        IServiceScopeFactory scopeFactory,
        ILogger<AnomalyDetectedConsumer> logger)
    {
        _connectionProvider =
            connectionProvider;

        _scopeFactory =
            scopeFactory;

        _logger =
            logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var connection =
            await _connectionProvider
                .GetConnectionAsync(
                    stoppingToken);

        var channel =
            await connection
                .CreateChannelAsync(
                    cancellationToken:
                        stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 10,
            global: false,
            cancellationToken:
                stoppingToken);

        var consumer =
            new AsyncEventingBasicConsumer(
                channel);

        consumer.ReceivedAsync +=
            async (_, args) =>
            {
                await HandleAsync(
                    channel,
                    args,
                    stoppingToken);
            };

        await channel.BasicConsumeAsync(
            RabbitMqTopology
                .InsightGenerationQueue,
            autoAck: false,
            consumer,
            stoppingToken);

        LogConsumerStarted();

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }

    private async Task HandleAsync(
        IChannel channel,
        BasicDeliverEventArgs args,
        CancellationToken cancellationToken)
    {
        try
        {
            var json =
                Encoding.UTF8.GetString(
                    args.Body.ToArray());

            var message =
                JsonSerializer.Deserialize<
                    AnomalyDetectedEvent>(
                    json,
                    JsonOptions)
                ?? throw new InvalidOperationException(
                    "Invalid AnomalyDetected event.");

            using var scope =
                _scopeFactory.CreateScope();

            var service =
                scope.ServiceProvider
                    .GetRequiredService<
                        InsightService>();

            await service.GenerateFromAnomalyAsync(
                message.UserId,
                message.AnomalyId,
                cancellationToken);

            await channel.BasicAckAsync(
                args.DeliveryTag,
                multiple: false,
                cancellationToken);
        }
        catch (Exception exception)
        {
            LogInsightGenerationFailed(
                exception);

            await channel.BasicNackAsync(
                args.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken);
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Insight generation consumer started.")]
    private partial void LogConsumerStarted();

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to generate financial insight.")]
    private partial void LogInsightGenerationFailed(
        Exception exception);
}
