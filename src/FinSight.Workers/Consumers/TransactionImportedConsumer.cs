using System.Text;
using System.Text.Json;
using FinSight.Application.Features.Transactions;
using FinSight.Contracts.Events;
using FinSight.Infrastructure.Messaging.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinSight.Workers.Consumers;

/// <summary>
/// Consumes transaction-import events and triggers AI categorization.
/// </summary>
public sealed partial class TransactionImportedConsumer(
    IRabbitMqConnectionProvider connectionProvider,
    IServiceScopeFactory scopeFactory,
    ILogger<TransactionImportedConsumer> logger)
    : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var connection =
            await connectionProvider
                .GetConnectionAsync(
                    stoppingToken);

        var channel =
            await connection
                .CreateChannelAsync(
                    cancellationToken: stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 10,
            global: false,
            cancellationToken: stoppingToken);

        var consumer =
            new AsyncEventingBasicConsumer(
                channel);

        consumer.ReceivedAsync +=
            async (_, eventArgs) =>
            {
                await HandleAsync(
                    channel,
                    eventArgs,
                    stoppingToken);
            };

        await channel.BasicConsumeAsync(
            RabbitMqTopology
                .TransactionCategorizationQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        LogConsumerStarted();

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }

    private async Task HandleAsync(
        IChannel channel,
        BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken)
    {
        try
        {
            var json =
                Encoding.UTF8.GetString(
                    eventArgs.Body.ToArray());

            var message =
                JsonSerializer.Deserialize<
                    TransactionImportedEvent>(
                    json,
                    JsonOptions)
                ?? throw new InvalidOperationException(
                    "Unable to deserialize transaction event.");

            using var scope =
                scopeFactory.CreateScope();

            var processingService =
                scope.ServiceProvider
                    .GetRequiredService<
                        TransactionProcessingService>();

            await processingService.ProcessAsync(
                message.TransactionId,
                message.UserId,
                cancellationToken);

            await channel.BasicAckAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken);
        }
        catch (Exception exception)
        {
            LogProcessingFailed(
                exception);

            await channel.BasicNackAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken);
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Transaction categorization consumer started.")]
    private partial void LogConsumerStarted();

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to process TransactionImported event.")]
    private partial void LogProcessingFailed(Exception exception);
}
