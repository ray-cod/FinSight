using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FinSight.Infrastructure.Observability;

/// <summary>
/// Provides shared OpenTelemetry registration.
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>
    /// Adds OpenTelemetry tracing and metrics.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="serviceName">The service name.</param>
    public static void AddFinSightTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var options =
            configuration
                .GetSection(
                    TelemetryOptions.SectionName)
                .Get<TelemetryOptions>()
            ?? new TelemetryOptions();

        var resolvedServiceName =
            options.ServiceName ??
            serviceName;

        services
            .AddOpenTelemetry()
            .ConfigureResource(
                resource =>
                {
                    resource.AddService(
                        resolvedServiceName,
                        serviceVersion:
                            options.ServiceVersion);
                })
            .WithTracing(
                tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSource(
                            "FinSight.*");

                    if (!string.IsNullOrWhiteSpace(
                            options.OtlpEndpoint))
                    {
                        tracing.AddOtlpExporter(
                            exporterOptions =>
                            {
                                exporterOptions
                                    .Endpoint =
                                    new Uri(
                                        options
                                            .OtlpEndpoint);
                            });
                    }
                })
            .WithMetrics(
                metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();

                    if (!string.IsNullOrWhiteSpace(
                            options.OtlpEndpoint))
                    {
                        metrics.AddOtlpExporter(
                            exporterOptions =>
                            {
                                exporterOptions
                                    .Endpoint =
                                    new Uri(
                                        options
                                            .OtlpEndpoint);
                            });
                    }
                });
    }
}
