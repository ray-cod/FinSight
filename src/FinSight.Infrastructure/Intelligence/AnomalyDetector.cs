using FinSight.Application.Abstractions.Intelligence;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Anomalies;
using FinSight.Domain.Transactions;
using FinSight.Infrastructure.Observability;

namespace FinSight.Infrastructure.Intelligence;

/// <summary>
/// Detects unusual financial behavior using deterministic statistical rules.
/// </summary>
public sealed class AnomalyDetector
    : IAnomalyDetector
{
    private readonly ITransactionRepository
        _transactionRepository;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AnomalyDetector"/> class.
    /// </summary>
    /// <param name="transactionRepository">
    /// The transaction repository.
    /// </param>
    public AnomalyDetector(
        ITransactionRepository transactionRepository)
    {
        _transactionRepository =
            transactionRepository;
    }

    /// <inheritdoc />
    public async Task<
        IReadOnlyList<AnomalyDetectionResult>>
        DetectAsync(
            Guid userId,
            Guid transactionId,
            CancellationToken cancellationToken = default)
    {
        using var activity =
            FinSightTelemetry.ActivitySource
                .StartActivity(
                    "FinSight.AnomalyDetection");

        activity?.SetTag(
            "user.id",
            userId.ToString());

        activity?.SetTag(
            "transaction.id",
            transactionId.ToString());

        var transaction =
            await _transactionRepository
                .GetByIdAsync(
                    userId,
                    transactionId,
                    cancellationToken);

        if (transaction is null ||
            transaction.Amount >= 0 ||
            transaction.Type !=
            TransactionType.Purchase)
        {
            return Array.Empty<AnomalyDetectionResult>();
        }

        var results =
            new List<AnomalyDetectionResult>();

        await DetectLargeTransactionAsync(
            userId,
            transaction,
            results,
            cancellationToken);

        await DetectMerchantAnomalyAsync(
            userId,
            transaction,
            results,
            cancellationToken);

        await DetectCategoryAnomalyAsync(
            userId,
            transaction,
            results,
            cancellationToken);

        await DetectNewMerchantAsync(
            userId,
            transaction,
            results,
            cancellationToken);

        await DetectDuplicateAsync(
            userId,
            transaction,
            results,
            cancellationToken);

        return results;
    }

    private async Task DetectLargeTransactionAsync(
        Guid userId,
        Transaction transaction,
        List<AnomalyDetectionResult> results,
        CancellationToken cancellationToken)
    {
        var history =
            await _transactionRepository
                .GetForPeriodAsync(
                    userId,
                    transaction.TransactionDate
                        .AddDays(-180),
                    transaction.TransactionDate,
                    cancellationToken);

        var purchases =
            history
                .Where(
                    x =>
                        x.Type ==
                        TransactionType.Purchase &&
                        x.Amount < 0)
                .Select(
                    x =>
                        Math.Abs(x.Amount))
                .ToArray();

        if (purchases.Length < 5)
        {
            return;
        }

        var current =
            Math.Abs(transaction.Amount);

        var mean =
            purchases.Average();

        var standardDeviation =
            CalculateStandardDeviation(
                purchases,
                mean);

        if (standardDeviation <= 0)
        {
            if (current <= mean * 3)
            {
                return;
            }

            results.Add(
                CreateLargeTransactionResult(
                    current,
                    mean,
                    1m));

            return;
        }

        var zScore =
            (current - mean) /
            (decimal)standardDeviation;

        if (zScore < 2.0m)
        {
            return;
        }

        var score =
            NormalizeZScore(zScore);

        results.Add(
            CreateLargeTransactionResult(
                current,
                mean,
                score));
    }

    private async Task DetectMerchantAnomalyAsync(
        Guid userId,
        Transaction transaction,
        List<AnomalyDetectionResult> results,
        CancellationToken cancellationToken)
    {
        if (!transaction.MerchantId.HasValue)
        {
            return;
        }

        var history =
            await _transactionRepository
                .GetPreviousForMerchantAsync(
                    userId,
                    transaction.MerchantId.Value,
                    transaction.TransactionDate,
                    transaction.Currency,
                    30,
                    cancellationToken);

        var amounts =
            history
                .Select(
                    x =>
                        Math.Abs(x.Amount))
                .ToArray();

        if (amounts.Length < 3)
        {
            return;
        }

        var current =
            Math.Abs(transaction.Amount);

        var mean =
            amounts.Average();

        if (mean <= 0)
        {
            return;
        }

        var deviation =
            amounts
                .Average(
                    amount =>
                        Math.Abs(
                            amount -
                            mean));

        if (deviation <= 0)
        {
            if (current <= mean * 1.50m)
            {
                return;
            }

            results.Add(
                new AnomalyDetectionResult(
                    AnomalyType.MerchantSpendingSpike,
                    AnomalySeverity.Medium,
                    0.80m,
                    0.95m,
                    "Unusually large merchant charge",
                    $"This merchant charged {current:C}, compared with a typical charge of {mean:C}.",
                    $"Current charge: {current:F2}; typical charge: {mean:F2}."));

            return;
        }

        var ratio =
            current / mean;

        if (ratio < 1.75m)
        {
            return;
        }

        var score =
            Math.Clamp(
                ratio / 4m,
                0m,
                1m);

        var severity =
            ratio >= 4m
                ? AnomalySeverity.High
                : AnomalySeverity.Medium;

        results.Add(
            new AnomalyDetectionResult(
                AnomalyType.MerchantSpendingSpike,
                severity,
                score,
                0.90m,
                "Merchant charge is unusually high",
                $"This merchant charged {current:C}, substantially above the typical charge of {mean:C}.",
                $"Current charge: {current:F2}; average historical charge: {mean:F2}; ratio: {ratio:F2}x."));
    }

    private async Task DetectCategoryAnomalyAsync(
        Guid userId,
        Transaction transaction,
        List<AnomalyDetectionResult> results,
        CancellationToken cancellationToken)
    {
        if (!transaction.CategoryId.HasValue)
        {
            return;
        }

        var monthStart =
            new DateTimeOffset(
                transaction.TransactionDate.Year,
                transaction.TransactionDate.Month,
                1,
                0,
                0,
                0,
                TimeSpan.Zero);

        var currentPeriodTransactions =
            await _transactionRepository
                .GetForPeriodAsync(
                    userId,
                    monthStart,
                    monthStart.AddMonths(1),
                    cancellationToken);

        var currentCategorySpend =
            currentPeriodTransactions
                .Where(
                    x =>
                        x.CategoryId ==
                        transaction.CategoryId &&
                        x.Amount < 0)
                .Sum(
                    x =>
                        Math.Abs(x.Amount));

        var historicalMonths =
            new List<decimal>();

        for (var i = 1; i <= 3; i++)
        {
            var start =
                monthStart.AddMonths(-i);

            var end =
                start.AddMonths(1);

            var monthTransactions =
                await _transactionRepository
                    .GetForPeriodAsync(
                        userId,
                        start,
                        end,
                        cancellationToken);

            var spend =
                monthTransactions
                    .Where(
                        x =>
                            x.CategoryId ==
                            transaction.CategoryId &&
                            x.Amount < 0)
                    .Sum(
                        x =>
                            Math.Abs(x.Amount));

            historicalMonths.Add(
                spend);
        }

        var baseline =
            historicalMonths.Average();

        if (baseline <= 0)
        {
            return;
        }

        var ratio =
            currentCategorySpend /
            baseline;

        if (ratio < 1.50m)
        {
            return;
        }

        var score =
            Math.Clamp(
                (ratio - 1m) / 2m,
                0m,
                1m);

        var severity =
            ratio >= 2.5m
                ? AnomalySeverity.High
                : AnomalySeverity.Medium;

        var increasePercentage =
            (ratio - 1m) * 100m;

        results.Add(
            new AnomalyDetectionResult(
                AnomalyType.CategorySpendingSpike,
                severity,
                score,
                0.85m,
                "Category spending is higher than usual",
                $"Spending in this category is {increasePercentage:F0}% above the recent baseline.",
                $"Current month: {currentCategorySpend:F2}; three-month baseline: {baseline:F2}; ratio: {ratio:F2}x."));
    }

    private async Task DetectNewMerchantAsync(
        Guid userId,
        Transaction transaction,
        List<AnomalyDetectionResult> results,
        CancellationToken cancellationToken)
    {
        if (!transaction.MerchantId.HasValue)
        {
            return;
        }

        var previous =
            await _transactionRepository
                .GetPreviousForMerchantAsync(
                    userId,
                    transaction.MerchantId.Value,
                    transaction.TransactionDate,
                    transaction.Currency,
                    1,
                    cancellationToken);

        if (previous.Count > 0)
        {
            return;
        }

        results.Add(
            new AnomalyDetectionResult(
                AnomalyType.NewMerchant,
                AnomalySeverity.Low,
                0.60m,
                0.95m,
                "New merchant detected",
                "This is the first recorded transaction with this merchant.",
                $"First observed transaction amount: {Math.Abs(transaction.Amount):F2} {transaction.Currency}."));
    }

    private async Task DetectDuplicateAsync(
        Guid userId,
        Transaction transaction,
        List<AnomalyDetectionResult> results,
        CancellationToken cancellationToken)
    {
        var duplicates =
            await _transactionRepository
                .FindPotentialDuplicatesAsync(
                    userId,
                    transaction.AccountId,
                    transaction.Id.Value,
                    transaction.Amount,
                    transaction.TransactionDate,
                    transaction.Currency,
                    cancellationToken);

        if (duplicates.Count == 0)
        {
            return;
        }

        results.Add(
            new AnomalyDetectionResult(
                AnomalyType.DuplicateTransaction,
                AnomalySeverity.High,
                0.90m,
                0.92m,
                "Possible duplicate transaction",
                "A matching transaction was found on the same account within a short time window.",
                $"Matching transaction count: {duplicates.Count}."));
    }

    private static AnomalyDetectionResult
        CreateLargeTransactionResult(
            decimal current,
            decimal mean,
            decimal score)
    {
        var ratio =
            mean <= 0
                ? 0
                : current / mean;

        var severity =
            ratio >= 5m
                ? AnomalySeverity.Critical
                : ratio >= 3m
                    ? AnomalySeverity.High
                    : AnomalySeverity.Medium;

        return new AnomalyDetectionResult(
            AnomalyType.LargeTransaction,
            severity,
            score,
            0.95m,
            "Unusually large transaction",
            $"This transaction is {ratio:F1}x larger than your recent average purchase.",
            $"Transaction amount: {current:F2}; recent average: {mean:F2}; ratio: {ratio:F2}x.");
    }

    private static decimal NormalizeZScore(
        decimal zScore)
    {
        return Math.Clamp(
            (zScore - 2m) / 4m,
            0m,
            1m);
    }

    private static double CalculateStandardDeviation(
        IReadOnlyCollection<decimal> values,
        decimal mean)
    {
        var variance =
            values.Average(
                value =>
                {
                    var difference =
                        (double)(
                            value -
                            mean);

                    return difference *
                           difference;
                });

        return Math.Sqrt(variance);
    }
}
