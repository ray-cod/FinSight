namespace FinSight.Infrastructure.Observability;

/// <summary>
/// Represents observability configuration.
/// </summary>
public sealed class TelemetryOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName =
        "Telemetry";

    /// <summary>
    /// Gets the OTLP endpoint.
    /// </summary>
    public string? OtlpEndpoint { get; init; }

    /// <summary>
    /// Gets the service name override.
    /// </summary>
    public string? ServiceName { get; init; }

    /// <summary>
    /// Gets the service version.
    /// </summary>
    public string ServiceVersion { get; init; } =
        "1.0.0";
}
