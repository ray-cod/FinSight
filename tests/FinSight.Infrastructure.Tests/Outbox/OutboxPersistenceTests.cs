using FinSight.Domain.Outbox;
using FluentAssertions;

namespace FinSight.Infrastructure.Tests.Outbox;

/// <summary>
/// Tests transactional outbox persistence behavior.
/// </summary>
public sealed class OutboxPersistenceTests
{
    /// <summary>
    /// Verifies that a new outbox message begins pending.
    /// </summary>
    [Fact]
    public void CreateShouldCreatePendingMessage()
    {
        var message =
            OutboxMessage.Create(
                "Test.Event",
                "{}",
                "test.event",
                DateTimeOffset.UtcNow);

        message.Status
            .Should()
            .Be(
                OutboxMessageStatus.Pending);
    }

    /// <summary>
    /// Verifies that a published message records its publication state.
    /// </summary>
    [Fact]
    public void MarkPublishedShouldSetPublishedStatus()
    {
        var message =
            OutboxMessage.Create(
                "Test.Event",
                "{}",
                "test.event",
                DateTimeOffset.UtcNow);

        message.MarkPublished();

        message.Status
            .Should()
            .Be(
                OutboxMessageStatus.Published);

        message.PublishedAt
            .Should()
            .NotBeNull();
    }

    /// <summary>
    /// Verifies retry state is recorded.
    /// </summary>
    [Fact]
    public void RecordFailureShouldIncrementAttemptCount()
    {
        var message =
            OutboxMessage.Create(
                "Test.Event",
                "{}",
                "test.event",
                DateTimeOffset.UtcNow);

        message.RecordFailure(
            "RabbitMQ unavailable",
            DateTimeOffset.UtcNow.AddMinutes(1));

        message.AttemptCount
            .Should()
            .Be(1);

        message.LastError
            .Should()
            .Be(
                "RabbitMQ unavailable");
    }
}
