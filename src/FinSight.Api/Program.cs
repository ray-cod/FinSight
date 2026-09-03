using FinSight.Api.Extensions;
using FinSight.Api.Middleware;
using FinSight.Application;
using FinSight.Infrastructure;
using FinSight.Infrastructure.Identity;
using FinSight.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder =
    WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(
                context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    });

builder.Services
    .AddApplication();

builder.Services
    .AddInfrastructure(
        builder.Configuration);

builder.Services
    .AddFinSightApi();

builder.Services
    .AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var identitySeeder =
        scope.ServiceProvider
            .GetRequiredService<IdentitySeedService>();

    await identitySeeder.SeedAsync();

    var financialSeeder =
        scope.ServiceProvider
            .GetRequiredService<FinancialSeedService>();

    await financialSeeder.SeedAsync();

    var categorySeeder =
        scope.ServiceProvider
            .GetRequiredService<CategorySeedService>();

    await categorySeeder.SeedAsync();
}

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseExceptionHandler();

app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = _ => false
    });

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = check =>
            check.Tags.Contains("ready")
    });

app.MapControllers()
    .RequireRateLimiting("api");

await app.RunAsync();
