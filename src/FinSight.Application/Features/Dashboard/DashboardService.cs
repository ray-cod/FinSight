using System.Globalization;
using FinSight.Application.Abstractions.Notifications;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Transactions;

namespace FinSight.Application.Features.Dashboard;

/// <summary>
/// Builds the read-optimized financial overview used by the FinSight dashboard.
/// </summary>
public sealed class DashboardService(
    IFinancialAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository,
    ISubscriptionRepository subscriptionRepository,
    IAnomalyRepository anomalyRepository,
    IInsightRepository insightRepository,
    INotificationRepository notificationRepository)
{
    /// <summary>
    /// Gets the authenticated user's aggregated dashboard data.
    /// </summary>
    /// <param name="userId">The authenticated user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The dashboard read model.</returns>
    public async Task<DashboardResponse> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var currentMonthStart = new DateTimeOffset(
            now.Year,
            now.Month,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);
        var sixMonthStart = currentMonthStart.AddMonths(-5);
        var nextMonthStart = currentMonthStart.AddMonths(1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);

        var accountsTask = accountRepository.GetByUserIdAsync(userId, cancellationToken);
        var transactionsTask = transactionRepository.GetForPeriodAsync(
            userId,
            sixMonthStart,
            nextMonthStart,
            cancellationToken);
        var categoriesTask = categoryRepository.GetActiveCategoriesAsync(cancellationToken);
        var subscriptionsTask = subscriptionRepository.GetByUserIdAsync(
            userId,
            includeDismissed: false,
            cancellationToken);
        var anomaliesTask = anomalyRepository.GetByUserIdAsync(
            userId,
            includeResolved: false,
            limit: 100,
            cancellationToken);
        var insightsTask = insightRepository.GetByUserIdAsync(
            userId,
            includeDismissed: false,
            limit: 100,
            cancellationToken);
        var notificationsTask = notificationRepository.GetForUserAsync(
            userId,
            includeRead: false,
            limit: 100,
            cancellationToken);

        await Task.WhenAll(
            accountsTask,
            transactionsTask,
            categoriesTask,
            subscriptionsTask,
            anomaliesTask,
            insightsTask,
            notificationsTask);

        var accounts = await accountsTask;
        var transactions = await transactionsTask;
        var categories = await categoriesTask;
        var subscriptions = await subscriptionsTask;
        var anomalies = await anomaliesTask;
        var insights = await insightsTask;
        var notifications = await notificationsTask;

        var categoryLookup = categories.ToDictionary(x => x.Id, x => x.Name);
        var balances = accounts
            .GroupBy(x => x.Currency, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(x => x.Count())
            .ThenByDescending(x => x.Sum(account => account.CurrentBalance))
            .ThenBy(x => x.Key)
            .Select(
                group => new DashboardBalanceResponse(
                    group.Key,
                    group.Sum(account => account.CurrentBalance),
                    group.Sum(account => account.AvailableBalance),
                    group.Count()))
            .ToArray();
        var primaryCurrency = balances.FirstOrDefault()?.Currency ?? "USD";
        var primaryCurrencyTransactions = transactions
            .Where(transaction => string.Equals(
                transaction.Currency,
                primaryCurrency,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var trend = Enumerable
            .Range(0, 6)
            .Select(index =>
            {
                var start = sixMonthStart.AddMonths(index);
                var end = start.AddMonths(1);
                var monthTransactions = primaryCurrencyTransactions
                    .Where(transaction =>
                        transaction.TransactionDate >= start &&
                        transaction.TransactionDate < end)
                    .ToArray();

                var income = monthTransactions
                    .Where(IsIncome)
                    .Sum(transaction => Math.Abs(transaction.Amount));
                var spending = monthTransactions
                    .Where(IsSpending)
                    .Sum(transaction => Math.Abs(transaction.Amount));

                return new DashboardTrendResponse(
                    start.ToString("MMM", CultureInfo.InvariantCulture),
                    start,
                    end,
                    income,
                    spending,
                    income - spending);
            })
            .ToArray();

        var currentMonthTransactions = primaryCurrencyTransactions
            .Where(transaction =>
                transaction.TransactionDate >= currentMonthStart &&
                transaction.TransactionDate < nextMonthStart)
            .ToArray();
        var previousMonthTransactions = primaryCurrencyTransactions
            .Where(transaction =>
                transaction.TransactionDate >= previousMonthStart &&
                transaction.TransactionDate < currentMonthStart)
            .ToArray();

        var currentMonthSpending = currentMonthTransactions
            .Where(IsSpending)
            .Sum(transaction => Math.Abs(transaction.Amount));
        var previousMonthSpending = previousMonthTransactions
            .Where(IsSpending)
            .Sum(transaction => Math.Abs(transaction.Amount));
        var currentMonthIncome = currentMonthTransactions
            .Where(IsIncome)
            .Sum(transaction => Math.Abs(transaction.Amount));

        var totalCurrentSpending = currentMonthTransactions
            .Where(IsSpending)
            .Sum(transaction => Math.Abs(transaction.Amount));

        var spendingByCategory = currentMonthTransactions
            .Where(IsSpending)
            .GroupBy(transaction => transaction.CategoryId)
            .Select(group => new
            {
                CategoryId = group.Key,
                Amount = group.Sum(transaction => Math.Abs(transaction.Amount)),
                CategoryName = group.Key is Guid categoryId && categoryLookup.TryGetValue(categoryId, out var name)
                    ? name
                    : "Uncategorized"
            })
            .OrderByDescending(x => x.Amount)
            .Take(6)
            .ToArray();
        var categorySpending = spendingByCategory
            .Select(
                item => new DashboardCategorySpendResponse(
                    item.CategoryId,
                    item.CategoryName,
                    item.Amount,
                    totalCurrentSpending == 0
                        ? 0
                        : decimal.Round(item.Amount / totalCurrentSpending * 100, 1)))
            .ToArray();

        var recentTransactions = transactions
            .OrderByDescending(transaction => transaction.TransactionDate)
            .Take(8)
            .Select(
                transaction => new DashboardTransactionResponse(
                    transaction.Id.Value,
                    transaction.AccountId,
                    transaction.NormalizedDescription,
                    transaction.Currency,
                    transaction.Amount,
                    transaction.TransactionDate,
                    transaction.Type.ToString(),
                    transaction.CategoryId is Guid categoryId && categoryLookup.TryGetValue(categoryId, out var name)
                        ? name
                        : "Uncategorized",
                    transaction.ClassificationStatus.ToString()))
            .ToArray();

        var upcomingSubscriptions = subscriptions
            .Where(subscription => subscription.Status == Domain.Subscriptions.SubscriptionStatus.Active)
            .OrderBy(subscription => subscription.NextExpectedChargeAt ?? DateTimeOffset.MaxValue)
            .ThenBy(subscription => subscription.MerchantName)
            .Take(5)
            .Select(
                subscription => new DashboardSubscriptionResponse(
                    subscription.Id,
                    subscription.MerchantName,
                    subscription.Currency,
                    subscription.CurrentAmount,
                    subscription.Frequency.ToString(),
                    subscription.NextExpectedChargeAt))
            .ToArray();

        var priorityAlerts = anomalies
            .Where(anomaly => anomaly.Status == Domain.Anomalies.AnomalyStatus.Open)
            .OrderByDescending(anomaly => SeverityRank(anomaly.Severity.ToString()))
            .ThenByDescending(anomaly => anomaly.DetectedAt)
            .Take(3)
            .Select(
                anomaly => new DashboardAlertResponse(
                    anomaly.Id,
                    "anomaly",
                    anomaly.Severity.ToString(),
                    anomaly.Title,
                    anomaly.Description,
                    anomaly.DetectedAt,
                    $"/anomalies/{anomaly.Id}"))
            .Concat(
                insights
                    .Where(insight => insight.Status == Domain.Insights.InsightStatus.Active)
                    .OrderByDescending(insight => SeverityRank(insight.Severity.ToString()))
                    .ThenByDescending(insight => insight.CreatedAt)
                    .Take(3)
                    .Select(
                        insight => new DashboardAlertResponse(
                            insight.Id,
                            "insight",
                            insight.Severity.ToString(),
                            insight.Title,
                            insight.Message,
                            insight.OccurredAt,
                            $"/insights/{insight.Id}")))
            .OrderByDescending(alert => SeverityRank(alert.Severity))
            .ThenByDescending(alert => alert.OccurredAt)
            .Take(5)
            .ToArray();

        var spendingChange = previousMonthSpending == 0
            ? currentMonthSpending == 0 ? 0 : 100
            : decimal.Round(
                (currentMonthSpending - previousMonthSpending) /
                previousMonthSpending * 100,
                1);

        return new DashboardResponse(
            primaryCurrency,
            balances,
            trend,
            categorySpending,
            recentTransactions,
            upcomingSubscriptions,
            priorityAlerts,
            new DashboardMetricResponse(
                currentMonthSpending,
                previousMonthSpending,
                spendingChange,
                currentMonthIncome,
                subscriptions.Count(subscription => subscription.Status == Domain.Subscriptions.SubscriptionStatus.Active),
                anomalies.Count(anomaly => anomaly.Status == Domain.Anomalies.AnomalyStatus.Open),
                insights.Count(insight => insight.Status == Domain.Insights.InsightStatus.Active),
                notifications.Count));
    }

    private static int SeverityRank(string severity) => severity switch
    {
        "Critical" => 4,
        "High" => 3,
        "Medium" => 2,
        "Low" => 1,
        _ => 0
    };

    private static bool IsIncome(Domain.Transactions.Transaction transaction) =>
        transaction.Type == TransactionType.Deposit && transaction.Amount > 0;

    private static bool IsSpending(Domain.Transactions.Transaction transaction) =>
        (transaction.Type == TransactionType.Purchase || transaction.Type == TransactionType.Fee) &&
        transaction.Amount != 0;
}
