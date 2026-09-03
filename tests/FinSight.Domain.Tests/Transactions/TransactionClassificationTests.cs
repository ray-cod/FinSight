using FinSight.Domain.Transactions;
using FluentAssertions;

namespace FinSight.Domain.Tests.Transactions;

/// <summary>
/// Tests transaction classification behavior.
/// </summary>
public sealed class TransactionClassificationTests
{
    /// <summary>
    /// Verifies that high-confidence classifications become classified.
    /// </summary>
    [Fact]
    public void HighConfidenceClassificationShouldBeClassified()
    {
        var transaction =
            Transaction.CreateImported(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "provider-001",
                "AMZN Mktp US",
                -84.72m,
                "USD",
                DateTimeOffset.UtcNow,
                TransactionType.Purchase,
                TransactionStatus.Imported,
                "fingerprint");

        var categoryId =
            Guid.NewGuid();

        transaction.ApplyClassification(
            Guid.NewGuid(),
            categoryId,
            null,
            ClassificationSource.Ai,
            0.96m);

        transaction.ClassificationStatus
            .Should()
            .Be(ClassificationStatus.Classified);

        transaction.ClassificationConfidence
            .Should()
            .Be(0.96m);
    }

    /// <summary>
    /// Verifies that low-confidence classifications become uncertain.
    /// </summary>
    [Fact]
    public void LowConfidenceClassificationShouldBeUncertain()
    {
        var transaction =
            Transaction.CreateImported(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "provider-002",
                "UNKNOWN MERCHANT",
                -50m,
                "USD",
                DateTimeOffset.UtcNow,
                TransactionType.Purchase,
                TransactionStatus.Imported,
                "fingerprint-2");

        transaction.ApplyClassification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            ClassificationSource.Ai,
            0.41m);

        transaction.ClassificationStatus
            .Should()
            .Be(ClassificationStatus.Uncertain);
    }

    /// <summary>
    /// Verifies that user corrections override automated classifications.
    /// </summary>
    [Fact]
    public void UserCorrectionShouldOverrideAutomatedClassification()
    {
        var transaction =
            Transaction.CreateImported(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "provider-003",
                "PAYPAL XYZ",
                -40m,
                "USD",
                DateTimeOffset.UtcNow,
                TransactionType.Purchase,
                TransactionStatus.Imported,
                "fingerprint-3");

        var categoryId =
            Guid.NewGuid();

        transaction.ApplyUserCorrection(
            null,
            categoryId,
            null);

        transaction.ClassificationStatus
            .Should()
            .Be(
                ClassificationStatus.UserCorrected);

        transaction.ClassificationSource
            .Should()
            .Be(
                ClassificationSource.User);

        transaction.ClassificationConfidence
            .Should()
            .Be(1m);
    }
}
