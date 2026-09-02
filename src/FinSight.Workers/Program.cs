using FinSight.Application;
using FinSight.Infrastructure;
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

builder.Services
    .AddApplication();

builder.Services
    .AddInfrastructure(
        builder.Configuration);

var host = builder.Build();

await host.RunAsync();
