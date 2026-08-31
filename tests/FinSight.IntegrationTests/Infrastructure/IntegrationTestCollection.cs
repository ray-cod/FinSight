using Xunit;

namespace FinSight.IntegrationTests.Infrastructure;

/// <summary>
/// Defines the xUnit collection fixture for integration tests requiring shared Testcontainers resources.
/// </summary>
[CollectionDefinition("Integration")]
public sealed class IntegrationTestCollectionDefinition
    : ICollectionFixture<IntegrationTestFixture>;
