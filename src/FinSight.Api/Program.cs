using FinSight.Api.Extensions;
using FinSight.Api.Middleware;
using FinSight.Application;
using FinSight.Infrastructure;
using FinSight.Infrastructure.Identity;
using FinSight.Infrastructure.Observability;
using FinSight.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder =
    WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(
    options =>
    {
        options.Limits.MaxRequestBodySize =
            10 * 1024 * 1024;
    });

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
        builder.Configuration,
        builder.Environment);

builder.Services.AddFinSightTelemetry(
    builder.Configuration,
    "FinSight.Api");

builder.Services
    .AddFinSightApi();

builder.Services
    .AddOpenApi();

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ??
    Array.Empty<string>();

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "Frontend",
            policy =>
            {
                // If no origins are configured, explicitly disallow cross-origin
                // requests instead of calling WithOrigins with an empty array
                // (which can throw at runtime).
                if (allowedOrigins.Length > 0)
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
                else
                {
                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(_ => false);
                }
            });
    });

var app = builder.Build();

app.Lifetime.ApplicationStopping.Register(
    () =>
    {
        Log.Information(
            "FinSight API shutdown initiated.");
    });

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

app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseExceptionHandler();

app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseCors("Frontend");

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

try
{
    await app.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
