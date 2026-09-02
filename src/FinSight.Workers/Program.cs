using FinSight.Application;
using FinSight.Infrastructure;
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
    configureAuthentication: false,
    configureIdentity: false);

builder.Services.AddHostedService<
    BankSyncWorker>();

var host =
    builder.Build();

await host.RunAsync();
