using FinSight.Application.Abstractions.Persistence;
using FinSight.Application.Features.Subscriptions;
using FinSight.Domain.Transactions;
using FluentAssertions;

namespace FinSight.Application.Tests.Subscriptions;

/// <summary>
/// Tests recurring-payment detection.
/// </summary>
public sealed class SubscriptionDetectionServiceTests
{
    /// <summary>
    /// Verifies that three monthly charges are detected.
    /// </summary>
    [Fact]
    public async Task MonthlyPatternShouldBeDetected()
    {
        var userId =
            Guid.NewGuid();

        var merchantId =
            Guid.NewGuid();

        var baseDate =
            DateTimeOffset.UtcNow.AddMonths(-2);

        var transactions =
            new[]
            {
                CreateTransaction(
                    userId,
                    merchantId,
                    15.99m,
                    baseDate),

                CreateTransaction(
                    userId,
                    merchantId,
                    15.99m,
                    baseDate.AddDays(30)),

                CreateTransaction(
                    userId,
                    merchantId,
                    15.99m,
                    baseDate.AddDays(60))
            };

        var repository =
            new FakeTransactionRepository(
                transactions);

        var service =
            new SubscriptionDetectionService(
                repository);

        var result =
            await service.AnalyzeAsync(
                userId,
                merchantId,
                "USD");

        result.IsSubscription
            .Should()
            .BeTrue();

        result.Frequency
            .Should()
            .Be(
                Domain.Subscriptions
                    .BillingFrequency.Monthly);

        result.CurrentAmount
            .Should()
            .Be(15.99m);
    }

    /// <summary>
    /// Verifies that irregular transactions are not falsely identified.
    /// </summary>
    [Fact]
    public async Task IrregularPatternShouldNotBeDetected()
    {
        var userId =
            Guid.NewGuid();

        var merchantId =
            Guid.NewGuid();

        var start =
            DateTimeOffset.UtcNow.AddDays(-120);

        var transactions =
            new[]
            {
                CreateTransaction(
                    userId,
                    merchantId,
                    15m,
                    start),

                CreateTransaction(
                    userId,
                    merchantId,
                    72m,
                    start.AddDays(14)),

                CreateTransaction(
                    userId,
                    merchantId,
                    31m,
                    start.AddDays(93))
            };

        var repository =
            new FakeTransactionRepository(
                transactions);

        var service =
            new SubscriptionDetectionService(
                repository);

        var result =
            await service.AnalyzeAsync(
                userId,
                merchantId,
                "USD");

        result.IsSubscription
            .Should()
            .BeFalse();
    }

    private static Transaction CreateTransaction(
        Guid userId,
        Guid merchantId,
        decimal amount,
        DateTimeOffset date)
    {
        var transaction =
            Transaction.CreateImported(
                userId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid().ToString(),
                "TEST MERCHANT",
                -amount,
                "USD",
                date,
                TransactionType.Purchase,
                TransactionStatus.Imported,
                Guid.NewGuid().ToString());

        transaction.ApplyClassification(
            merchantId,
            Guid.NewGuid(),
            null,
            ClassificationSource.Rule,
            0.99m);

        return transaction;
    }

    private sealed class FakeTransactionRepository
        : ITransactionRepository
    {
        private readonly IReadOnlyList<Transaction>
            _transactions;

        public FakeTransactionRepository(
            IReadOnlyList<Transaction> transactions)
        {
            _transactions =
                transactions;
        }

        public Task<bool> ExistsAsync(
            Guid accountId,
            string providerTransactionId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public void Add(
            Transaction transaction)
        {
        }

        public Task<Transaction?> GetByIdAsync(
            Guid userId,
            Guid transactionId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _transactions.FirstOrDefault(
                    x =>
                        x.UserId == userId &&
                        x.Id.Value == transactionId));
        }

        public Task<IReadOnlyList<Transaction>>
            GetForAccountAsync(
                Guid userId,
                Guid accountId,
                int limit = 100,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<Transaction>>(
                _transactions);
        }

        public Task<IReadOnlyList<Transaction>>
            GetByMerchantAsync(
                Guid userId,
                Guid merchantId,
                string currency,
                int limit = 36,
                CancellationToken cancellationToken = default)
        {
            var results =
                _transactions
                    .Where(
                        x =>
                            x.UserId == userId &&
                            x.MerchantId ==
                            merchantId &&
                            x.Currency == currency)
                    .OrderByDescending(
                        x => x.TransactionDate)
                    .Take(limit)
                    .ToArray();

            return Task.FromResult<
                IReadOnlyList<Transaction>>(
                results);
        }
    }
}
