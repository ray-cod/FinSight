using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Subscriptions;
using FinSight.Domain.Transactions;

namespace FinSight.Application.Features.Subscriptions;

/// <summary>
/// Detects recurring payment patterns from transaction history.
/// </summary>
public sealed class SubscriptionDetectionService
{
    private const int MinimumRecurringCharges = 3;

    private const decimal MaterialPriceChangePercentage = 0.05m;

    private readonly ITransactionRepository _transactionRepository;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SubscriptionDetectionService"/> class.
    /// </summary>
    /// <param name="transactionRepository">
    /// The transaction repository.
    /// </param>
    public SubscriptionDetectionService(
        ITransactionRepository transactionRepository)
    {
        _transactionRepository =
            transactionRepository;
    }

    /// <summary>
    /// Analyzes transactions for a merchant and determines
    /// whether they represent a recurring subscription.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="merchantId">The normalized merchant identifier.</param>
    /// <param name="currency">The transaction currency.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The recurring-payment analysis result.</returns>
    public async Task<SubscriptionDetectionResult>
        AnalyzeAsync(
            Guid userId,
            Guid merchantId,
            string currency,
            CancellationToken cancellationToken = default)
    {
        var transactions =
            await _transactionRepository
                .GetByMerchantAsync(
                    userId,
                    merchantId,
                    currency,
                    36,
                    cancellationToken);

        if (transactions.Count < 2)
        {
            return NotDetected();
        }

        var ordered =
            transactions
                .OrderBy(
                    x => x.TransactionDate)
                .ToArray();

        var intervals =
            ordered
                .Zip(
                    ordered.Skip(1),
                    (previous, current) =>
                        (current.TransactionDate -
                         previous.TransactionDate).TotalDays)
                .ToArray();

        if (intervals.Length == 0)
        {
            return NotDetected();
        }

        var frequency =
            InferFrequency(
                intervals);

        if (frequency ==
            BillingFrequency.Unknown)
        {
            return NotDetected();
        }

        var minimumCharges =
            frequency ==
            BillingFrequency.Annual
                ? 2
                : MinimumRecurringCharges;

        if (ordered.Length < minimumCharges)
        {
            return NotDetected();
        }

        var cadenceConsistency =
            CalculateCadenceConsistency(
                intervals,
                frequency);

        if (cadenceConsistency < 0.60m)
        {
            return NotDetected();
        }

        var amounts =
            ordered
                .Select(
                    transaction =>
                        Math.Abs(transaction.Amount))
                .ToArray();

        var averageAmount =
            amounts.Average();

        var currentAmount =
            amounts[^1];

        var amountConsistency =
            CalculateAmountConsistency(
                amounts);

        var confidence =
            CalculateConfidence(
                ordered.Length,
                cadenceConsistency,
                amountConsistency,
                frequency);

        if (confidence < 0.60m)
        {
            return NotDetected();
        }

        var previousAmount =
            amounts.Length > 1
                ? amounts[^2]
                : (decimal?)null;

        decimal? priceChangePercentage = null;

        if (previousAmount.HasValue &&
            previousAmount.Value > 0)
        {
            var change =
                (currentAmount -
                 previousAmount.Value) /
                previousAmount.Value;

            if (Math.Abs(change) >=
                MaterialPriceChangePercentage)
            {
                priceChangePercentage =
                    change;
            }
        }

        var medianInterval =
            CalculateMedian(
                intervals);

        var nextExpected =
            ordered[^1]
                .TransactionDate
                .AddDays(
                    medianInterval);

        return new SubscriptionDetectionResult(
            true,
            frequency,
            confidence,
            decimal.Round(
                averageAmount,
                2),
            decimal.Round(
                currentAmount,
                2),
            ordered[0].TransactionDate,
            ordered[^1].TransactionDate,
            nextExpected,
            previousAmount,
            priceChangePercentage);
    }

    private static SubscriptionDetectionResult
        NotDetected()
    {
        return new SubscriptionDetectionResult(
            false,
            BillingFrequency.Unknown,
            0m,
            0m,
            0m,
            DateTimeOffset.MinValue,
            DateTimeOffset.MinValue,
            null,
            null,
            null);
    }

    private static BillingFrequency InferFrequency(
        IReadOnlyList<double> intervals)
    {
        var median =
            CalculateMedian(intervals);

        return median switch
        {
            >= 5 and <= 9 =>
                BillingFrequency.Weekly,

            >= 12 and <= 18 =>
                BillingFrequency.BiWeekly,

            >= 24 and <= 38 =>
                BillingFrequency.Monthly,

            >= 75 and <= 110 =>
                BillingFrequency.Quarterly,

            >= 150 and <= 210 =>
                BillingFrequency.SemiAnnual,

            >= 330 and <= 400 =>
                BillingFrequency.Annual,

            _ =>
                BillingFrequency.Unknown
        };
    }

    private static decimal CalculateCadenceConsistency(
        IReadOnlyList<double> intervals,
        BillingFrequency frequency)
    {
        var expected =
            ExpectedDays(frequency);

        if (expected <= 0)
        {
            return 0m;
        }

        var errors =
            intervals
                .Select(
                    interval =>
                        Math.Abs(
                            interval -
                            expected) /
                        expected)
                .ToArray();

        var meanError =
            errors.Average();

        return Math.Clamp(
            1m -
            Convert.ToDecimal(meanError),
            0m,
            1m);
    }

    private static decimal CalculateAmountConsistency(
        decimal[] amounts)
    {
        if (amounts.Length <= 1)
        {
            return 1m;
        }

        var average =
            amounts.Average();

        if (average <= 0)
        {
            return 0m;
        }

        var meanDeviation =
            amounts
                .Average(
                    amount =>
                        Math.Abs(
                            amount -
                            average)) /
                average;

        return Math.Clamp(
            1m -
            meanDeviation,
            0m,
            1m);
    }

    private static decimal CalculateConfidence(
        int transactionCount,
        decimal cadenceConsistency,
        decimal amountConsistency,
        BillingFrequency frequency)
    {
        var historyScore =
            Math.Clamp(
                (transactionCount - 2) /
                6m,
                0m,
                1m);

        if (frequency ==
            BillingFrequency.Annual)
        {
            historyScore = 0.55m;
        }

        var confidence =
            0.35m +
            (0.35m *
             cadenceConsistency) +
            (0.20m *
             amountConsistency) +
            (0.10m *
             historyScore);

        return Math.Clamp(
            confidence,
            0m,
            1m);
    }

    private static double ExpectedDays(
        BillingFrequency frequency)
    {
        return frequency switch
        {
            BillingFrequency.Weekly => 7,
            BillingFrequency.BiWeekly => 14,
            BillingFrequency.Monthly => 30,
            BillingFrequency.Quarterly => 91,
            BillingFrequency.SemiAnnual => 182,
            BillingFrequency.Annual => 365,
            _ => 0
        };
    }

    private static double CalculateMedian(
        IReadOnlyList<double> values)
    {
        var ordered =
            values
                .OrderBy(x => x)
                .ToArray();

        var middle =
            ordered.Length / 2;

        return ordered.Length % 2 == 0
            ? (ordered[middle - 1] +
               ordered[middle]) / 2d
            : ordered[middle];
    }
}
