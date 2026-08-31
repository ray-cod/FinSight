using Xunit;

namespace FinSight.IntegrationTests.Infrastructure;

/// <summary>
/// Integration tests validating infrastructure test fixture initialization and container availability.
/// </summary>
/// <param name="fixture">The shared integration test fixture providing container connections.</param>
[Collection("Integration")]
public sealed class InfrastructureTests(
    IntegrationTestFixture fixture)
{
    /// <summary>
    /// Verifies that the PostgreSQL connection string is configured and non-empty.
    /// </summary>
    [Fact]
    public void PostgreSqlConnectionStringIsAvailable()
    {
        Assert.False(
            string.IsNullOrWhiteSpace(
                fixture.PostgreSqlConnectionString));
    }

    /// <summary>
    /// Verifies that the RabbitMQ connection string is configured and non-empty.
    /// </summary>
    [Fact]
    public void RabbitMqConnectionStringIsAvailable()
    {
        Assert.False(
            string.IsNullOrWhiteSpace(
                fixture.RabbitMqConnectionString));
    }
}
