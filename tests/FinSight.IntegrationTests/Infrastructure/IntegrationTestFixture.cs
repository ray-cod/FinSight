using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace FinSight.IntegrationTests.Infrastructure;

/// <summary>
/// Provides a shared xUnit test fixture that spins up Testcontainers for PostgreSQL and RabbitMQ during integration testing.
/// </summary>
public sealed class IntegrationTestFixture
    : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("finsight_test")
            .WithUsername("finsight")
            .WithPassword("finsight")
            .Build();

    private readonly RabbitMqContainer _rabbitMq =
        new RabbitMqBuilder("rabbitmq:3.13-alpine")
            .WithUsername("finsight")
            .WithPassword("finsight")
            .Build();

    /// <summary>
    /// Gets the connection string for the active PostgreSQL test container instance.
    /// </summary>
    public string PostgreSqlConnectionString =>
        _postgres.GetConnectionString();

    /// <summary>
    /// Gets the connection string for the active RabbitMQ test container instance.
    /// </summary>
    public string RabbitMqConnectionString =>
        _rabbitMq.GetConnectionString();

    /// <summary>
    /// Asynchronously starts the PostgreSQL and RabbitMQ container instances before test collection execution.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous container startup operation.</returns>
    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _postgres.StartAsync(),
            _rabbitMq.StartAsync());
    }

    /// <summary>
    /// Asynchronously stops and disposes of the PostgreSQL and RabbitMQ test container instances after test execution completes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous cleanup operation.</returns>
    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _postgres.DisposeAsync().AsTask(),
            _rabbitMq.DisposeAsync().AsTask());
    }
}
