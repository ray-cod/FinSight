namespace FinSight.Application.Features.Dashboard;

/// <summary>
/// Represents an aggregated dashboard view for an authenticated user.
/// </summary>
public sealed record DashboardResponse(
    string PrimaryCurrency,
    IReadOnlyList<DashboardBalanceResponse> Balances,
    IReadOnlyList<DashboardTrendResponse> SpendingTrend,
    IReadOnlyList<DashboardCategorySpendResponse> CategorySpending,
    IReadOnlyList<DashboardTransactionResponse> RecentTransactions,
    IReadOnlyList<DashboardSubscriptionResponse> UpcomingSubscriptions,
    IReadOnlyList<DashboardAlertResponse> PriorityAlerts,
    DashboardMetricResponse Metrics);

/// <summary>
/// Represents balances grouped by currency to prevent invalid cross-currency arithmetic.
/// </summary>
public sealed record DashboardBalanceResponse(
    string Currency,
    decimal CurrentBalance,
    decimal AvailableBalance,
    int AccountCount);

/// <summary>
/// Represents income and spending for a calendar month.
/// </summary>
public sealed record DashboardTrendResponse(
    string Label,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Income,
    decimal Spending,
    decimal NetCashFlow);

/// <summary>
/// Represents current-period spending grouped by financial category.
/// </summary>
public sealed record DashboardCategorySpendResponse(
    Guid? CategoryId,
    string CategoryName,
    decimal Amount,
    decimal Percentage);

/// <summary>
/// Represents a recent transaction rendered on the dashboard.
/// </summary>
public sealed record DashboardTransactionResponse(
    Guid Id,
    Guid AccountId,
    string MerchantName,
    string Currency,
    decimal Amount,
    DateTimeOffset TransactionDate,
    string Type,
    string CategoryName,
    string ClassificationStatus);

/// <summary>
/// Represents an upcoming recurring payment.
/// </summary>
public sealed record DashboardSubscriptionResponse(
    Guid Id,
    string MerchantName,
    string Currency,
    decimal CurrentAmount,
    string Frequency,
    DateTimeOffset? NextExpectedChargeAt);

/// <summary>
/// Represents a prioritized financial alert.
/// </summary>
public sealed record DashboardAlertResponse(
    Guid Id,
    string Kind,
    string Severity,
    string Title,
    string Description,
    DateTimeOffset OccurredAt,
    string Href);

/// <summary>
/// Represents dashboard headline metrics.
/// </summary>
public sealed record DashboardMetricResponse(
    decimal CurrentMonthSpending,
    decimal PreviousMonthSpending,
    decimal SpendingChangePercentage,
    decimal CurrentMonthIncome,
    int ActiveSubscriptionCount,
    int OpenAnomalyCount,
    int ActiveInsightCount,
    int UnreadNotificationCount);
