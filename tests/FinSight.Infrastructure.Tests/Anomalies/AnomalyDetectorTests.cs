using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Anomalies;
using FinSight.Domain.Transactions;
using FinSight.Infrastructure.Intelligence;
using FluentAssertions;

namespace FinSight.Infrastructure.Tests.Anomalies;

/// <summary>
/// Tests deterministic anomaly detection behavior.
/// </summary>
public sealed class AnomalyDetectorTests
{
    /// <summary>
    /// Verifies that a substantially oversized purchase is detected.
    /// </summary>
    [Fact]
    public async Task LargePurchaseShouldBeDetected()
    {
        var userId =
            Guid.NewGuid();

        var accountId =
            Guid.NewGuid();

        var merchantId =
            Guid.NewGuid();

        var currentDate =
            new DateTimeOffset(
                2026,
                8,
                20,
                12,
                0,
                0,
                TimeSpan.Zero);

        var transactions =
            Enumerable
                .Range(1, 8)
                .Select(
                    index =>
                        CreateTransaction(
                            userId,
                            accountId,
                            merchantId,
                            50m,
                            currentDate.AddDays(
                                -index * 10)))
                .Append(
                    CreateTransaction(
                        userId,
                        accountId,
                        merchantId,
                        500m,
                        currentDate))
                .ToArray();

        var repository =
            new FakeTransactionRepository(
                transactions);

        var detector =
            new AnomalyDetector(
                repository);

        var current =
            transactions[^1];

        var results =
            await detector.DetectAsync(
                userId,
                current.Id.Value);

        results
            .Should()
            .Contain(
                x =>
                    x.Type ==
                    AnomalyType.LargeTransaction);
    }

    /// <summary>
    /// Verifies that ordinary purchases are not automatically anomalous.
    /// </summary>
    [Fact]
    public async Task NormalPurchaseShouldNotBeLargeTransactionAnomaly()
    {
        var userId =
            Guid.NewGuid();

        var accountId =
            Guid.NewGuid();

        var merchantId =
            Guid.NewGuid();

        var currentDate =
            DateTimeOffset.UtcNow;

        var transactions =
            Enumerable
                .Range(1, 8)
                .Select(
                    index =>
                        CreateTransaction(
                            userId,
                            accountId,
                            merchantId,
                            50m,
                            currentDate.AddDays(
                                -index * 10)))
                .Append(
                    CreateTransaction(
                        userId,
                        accountId,
                        merchantId,
                        55m,
                        currentDate))
                .ToArray();

        var repository =
            new FakeTransactionRepository(
                transactions);

        var detector =
            new AnomalyDetector(
                repository);

        var current =
            transactions[^1];

        var results =
            await detector.DetectAsync(
                userId,
                current.Id.Value);

        results
            .Should()
            .NotContain(
                x =>
                    x.Type ==
                    AnomalyType.LargeTransaction);
    }

    private static Transaction
        CreateTransaction(
            Guid userId,
            Guid accountId,
            Guid merchantId,
            decimal amount,
            DateTimeOffset date)
    {
        var transaction =
            Transaction.CreateImported(
                userId,
                accountId,
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
            return Task.FromResult<
                IReadOnlyList<Transaction>>(
                _transactions
                    .Where(
                        x =>
                            x.UserId == userId &&
                            x.MerchantId ==
                            merchantId &&
                            x.Currency == currency)
                    .ToArray());
        }

        public Task<IReadOnlyList<Transaction>>
            GetForPeriodAsync(
                Guid userId,
                DateTimeOffset from,
                DateTimeOffset to,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<Transaction>>(
                _transactions
                    .Where(
                        x =>
                            x.UserId == userId &&
                            x.TransactionDate >= from &&
                            x.TransactionDate < to)
                    .ToArray());
        }

        public Task<IReadOnlyList<Transaction>>
            GetPreviousForMerchantAsync(
                Guid userId,
                Guid merchantId,
                DateTimeOffset before,
                string currency,
                int limit = 30,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<Transaction>>(
                _transactions
                    .Where(
                        x =>
                            x.UserId == userId &&
                            x.MerchantId ==
                            merchantId &&
                            x.Currency == currency &&
                            x.TransactionDate < before)
                    .OrderByDescending(
                        x => x.TransactionDate)
                    .Take(limit)
                    .ToArray());
        }

        public Task<IReadOnlyList<Transaction>>
            FindPotentialDuplicatesAsync(
                Guid userId,
                Guid accountId,
                Guid transactionId,
                decimal amount,
                DateTimeOffset transactionDate,
                string currency,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<Transaction>>(
                _transactions
                    .Where(
                        x =>
                            x.UserId == userId &&
                            x.AccountId == accountId &&
                            x.Id.Value != transactionId &&
                            x.Currency == currency &&
                            x.Amount == amount &&
                            Math.Abs(
                                (x.TransactionDate -
                                 transactionDate).TotalHours)
                            <= 24)
                    .ToArray());
        }
    }
}
