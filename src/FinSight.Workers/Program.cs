using FinSight.Application;
using FinSight.Application.Abstractions.Messaging;
using FinSight.Infrastructure;
using FinSight.Infrastructure.Observability;
using FinSight.Workers.Consumers;
using FinSight.Workers.Workers;
using Serilog;

var builder =
    Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(
    (services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(
                builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    });

builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    builder.Configuration,
    builder.Environment,
    configureAuthentication: false,
    configureIdentity: false);

// Workers should publish events directly to RabbitMQ. Replace the
// default `IEventPublisher` (outbox-based) with the concrete
// `RabbitMqEventPublisher` implementation registered by the
// infrastructure layer.
builder.Services.AddSingleton<IEventPublisher>(
    sp => sp.GetRequiredService<FinSight.Infrastructure.Messaging.RabbitMq.RabbitMqEventPublisher>());


builder.Services.AddFinSightTelemetry(
    builder.Configuration,
    "FinSight.Workers");

builder.Services.AddHostedService<
    BankSyncWorker>();

builder.Services.AddHostedService<
    TransactionImportedConsumer>();

builder.Services.AddHostedService<
    TransactionCategorizedConsumer>();

builder.Services.AddHostedService<
    SubscriptionLifecycleWorker>();

builder.Services.AddHostedService<
    TransactionCategorizedAnomalyConsumer>();

builder.Services.AddHostedService<
    AnomalyDetectedConsumer>();

builder.Services.AddHostedService<
    SubscriptionPriceChangedAnomalyConsumer>();

builder.Services.AddHostedService<
    AnomalyLifecycleWorker>();

builder.Services.AddHostedService<
    AuditRetentionWorker>();

builder.Services.AddHostedService<
    NotificationCreatedConsumer>();

builder.Services.AddHostedService<
    OutboxRetentionWorker>();

builder.Services.Configure<HostOptions>(
    options =>
    {
        options.ShutdownTimeout =
            TimeSpan.FromSeconds(30);
    });

var host =
    builder.Build();

try
{
    await host.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
