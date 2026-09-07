using FinSight.Domain.Notifications;
using FluentAssertions;

namespace FinSight.Domain.Tests.Notifications;

/// <summary>
/// Tests notification domain behavior.
/// </summary>
public sealed class NotificationTests
{
    /// <summary>
    /// Verifies that notifications begin pending.
    /// </summary>
    [Fact]
    public void CreateShouldCreatePendingNotification()
    {
        var notification =
            Notification.Create(
                Guid.NewGuid(),
                NotificationType.AnomalyDetected,
                NotificationChannel.InApp,
                "Unusual transaction",
                "A transaction was detected that is much larger than usual.",
                "anomaly:123");

        notification.Status
            .Should()
            .Be(
                NotificationStatus.Pending);
    }

    /// <summary>
    /// Verifies that delivery marks the notification as delivered.
    /// </summary>
    [Fact]
    public void MarkDeliveredShouldSetDeliveredStatus()
    {
        var notification =
            Notification.Create(
                Guid.NewGuid(),
                NotificationType.FinancialInsight,
                NotificationChannel.Email,
                "Insight",
                "A financial insight is available.",
                null);

        notification.MarkDelivered();

        notification.Status
            .Should()
            .Be(
                NotificationStatus.Delivered);
    }

    /// <summary>
    /// Verifies that a notification can be marked as read.
    /// </summary>
    [Fact]
    public void MarkReadShouldSetReadStatus()
    {
        var notification =
            Notification.Create(
                Guid.NewGuid(),
                NotificationType.Security,
                NotificationChannel.InApp,
                "Security",
                "Your password was changed.",
                null);

        notification.MarkDelivered();
        notification.MarkRead();

        notification.Status
            .Should()
            .Be(
                NotificationStatus.Read);
    }
}
