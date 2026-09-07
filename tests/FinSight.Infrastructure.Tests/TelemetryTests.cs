using FinSight.Infrastructure.Observability;
using FluentAssertions;

namespace FinSight.Infrastructure.Tests;

/// <summary>
/// Tests FinSight observability primitives.
/// </summary>
public sealed class TelemetryTests
{
    /// <summary>
    /// Verifies that the application activity source is available.
    /// </summary>
    [Fact]
    public void ActivitySourceShouldBeAvailable()
    {
        FinSightTelemetry.ActivitySource
            .Name
            .Should()
            .Be("FinSight");
    }

    /// <summary>
    /// Verifies that the application meter is available.
    /// </summary>
    [Fact]
    public void MeterShouldBeAvailable()
    {
        FinSightTelemetry.Meter
            .Name
            .Should()
            .Be("FinSight");
    }
}
