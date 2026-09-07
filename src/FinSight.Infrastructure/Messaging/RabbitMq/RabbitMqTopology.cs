using RabbitMQ.Client;

namespace FinSight.Infrastructure.Messaging.RabbitMq;

/// <summary>
/// Provides helper methods and constants for declaring RabbitMQ exchanges, queues, and bindings.
/// </summary>
public static class RabbitMqTopology
{
    /// <summary>
    /// The primary topic exchange name for application domain events.
    /// </summary>
    public const string ExchangeName =
        "finsight.events";

    /// <summary>
    /// The retry topic exchange name for messages that need to be retried.
    /// </summary>
    public const string RetryExchangeName =
    "finsight.events.retry";

    /// <summary>
    /// The dead-letter topic exchange name for unprocessable messages.
    /// </summary>
    public const string DeadLetterExchangeName =
        "finsight.events.dlx";

    /// <summary>
    /// The queue name for dead-lettered messages.
    /// </summary>
    public const string DeadLetterQueue =
        "finsight.events.dead-letter";

    /// <summary>
    /// The queue name for transaction categorization messages.
    /// </summary>
    public const string TransactionCategorizationQueue =
        "finsight.transaction-categorization";

    /// <summary>
    /// The queue name for subscription detection messages.
    /// </summary>
    public const string SubscriptionDetectionQueue =
    "finsight.subscription-detection";

    /// <summary>
    /// The queue name for anomaly detection messages.
    /// </summary>
    public const string AnomalyDetectionQueue =
        "finsight.anomaly-detection";

    /// <summary>
    /// The queue name for insight generation messages.
    /// </summary>
    public const string InsightGenerationQueue =
        "finsight.insight-generation";

    /// <summary>
    /// The queue name for subscription price anomaly messages.
    /// </summary>
    public const string SubscriptionPriceAnomalyQueue =
        "finsight.subscription-price-anomaly";

    /// <summary>
    /// The queue name for notification delivery messages.
    /// </summary>
    public const string NotificationQueue =
    "finsight.notification-delivery";

    /// <summary>
    /// Asynchronously initializes exchange declarations, dead-letter queues, and bindings on the RabbitMQ broker.
    /// </summary>
    /// <param name="connection">The active RabbitMQ connection used to create topology channels.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for channel operations to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task InitializeAsync(
        IConnection connection,
        CancellationToken cancellationToken = default)
    {
        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            ExchangeName,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            DeadLetterExchangeName,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum"
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            DeadLetterQueue,
            DeadLetterExchangeName,
            "#",
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            TransactionCategorizationQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum",
                ["x-dead-letter-exchange"] = DeadLetterExchangeName
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            TransactionCategorizationQueue,
            ExchangeName,
            "transaction.imported",
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            SubscriptionDetectionQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum",
                ["x-dead-letter-exchange"] = DeadLetterExchangeName
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            SubscriptionDetectionQueue,
            ExchangeName,
            "transaction.categorized",
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            AnomalyDetectionQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum",
                ["x-dead-letter-exchange"] = DeadLetterExchangeName
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            AnomalyDetectionQueue,
            ExchangeName,
            "transaction.categorized",
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            InsightGenerationQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum",
                ["x-dead-letter-exchange"] = DeadLetterExchangeName
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            InsightGenerationQueue,
            ExchangeName,
            "anomaly.detected",
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            SubscriptionPriceAnomalyQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum",
                ["x-dead-letter-exchange"] = DeadLetterExchangeName
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            SubscriptionPriceAnomalyQueue,
            ExchangeName,
            "subscription.price.changed",
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            NotificationQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum",
                ["x-dead-letter-exchange"] = DeadLetterExchangeName
            },
            cancellationToken:
                cancellationToken);

        await channel.QueueBindAsync(
            NotificationQueue,
            ExchangeName,
            "notification.created",
            cancellationToken:
                cancellationToken);
    }
}
