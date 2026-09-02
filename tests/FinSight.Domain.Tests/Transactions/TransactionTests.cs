using FinSight.Domain.Transactions;
using FluentAssertions;

namespace FinSight.Domain.Tests.Transactions;

/// <summary>
/// Tests financial transaction domain behavior.
/// </summary>
public sealed class TransactionTests
{
    /// <summary>
    /// Verifies that an imported transaction preserves provider data.
    /// </summary>
    [Fact]
    public void CreateImportedShouldPreserveProviderData()
    {
        var transaction =
            Transaction.CreateImported(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "provider-tx-001",
                "AMZN Mktp US",
                -84.72m,
                "USD",
                DateTimeOffset.UtcNow,
                TransactionType.Purchase,
                TransactionStatus.Imported,
                "ABC123");

        transaction
            .ProviderTransactionId
            .Should()
            .Be("provider-tx-001");

        transaction
            .RawDescription
            .Should()
            .Be("AMZN Mktp US");

        transaction.Amount
            .Should()
            .Be(-84.72m);
    }
}
