using FinSight.Infrastructure.Banking.MockBank.Models;

namespace FinSight.Infrastructure.Banking.MockBank;

/// <summary>
/// Provides deterministic mock financial data used by the local demo environment.
/// </summary>
public static class MockBankData
{
    /// <summary>
    /// Gets the mock bank account catalogue.
    /// </summary>
    public static IReadOnlyCollection<MockBankAccount> Accounts =>
    [
        new(
            "mock-checking-001",
            "Everyday Checking",
            "Checking",
            "USD",
            4280.42m,
            4210.42m),

        new(
            "mock-savings-001",
            "Emergency Savings",
            "Savings",
            "USD",
            12500.00m,
            12500.00m)
    ];

    /// <summary>
    /// Gets the deterministic transaction catalogue for the portfolio demo.
    /// </summary>
    public static IReadOnlyCollection<MockBankTransaction> Transactions =>
        BuildTransactions();

    private static List<MockBankTransaction> BuildTransactions()
    {
        var today = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var transactions = new List<MockBankTransaction>();

        AddMonthlySubscription(
            transactions,
            "mock-netflix",
            "NETFLIX.COM",
            [15.99m, 15.99m, 15.99m, 15.99m, 17.99m, 17.99m, 17.99m, 17.99m],
            today,
            0);

        AddMonthlySubscription(
            transactions,
            "mock-spotify",
            "SPOTIFY AB",
            [12.99m, 12.99m, 12.99m, 12.99m, 12.99m, 12.99m, 12.99m, 12.99m],
            today,
            1);

        AddMonthlySubscription(
            transactions,
            "mock-fitness",
            "JOES FITNESS CLUB",
            [49.00m, 49.00m, 49.00m, 49.00m, 49.00m, 49.00m],
            today,
            4);

        AddMonthlySeries(
            transactions,
            "mock-grocery",
            "WOOLWORTHS 1234",
            [
                (3, 182.44m),
                (2, 214.16m),
                (1, 205.90m),
                (0, 826.74m)
            ],
            today,
            6);

        AddAmazonHistory(transactions, today);

        transactions.AddRange(
            new MockBankTransaction(
                "mock-current-001",
                "mock-checking-001",
                "SALARY ACME CORP",
                4200.00m,
                "USD",
                today.AddDays(-7),
                "Deposit",
                "Imported"),

            new MockBankTransaction(
                "mock-current-002",
                "mock-checking-001",
                "UBER *TRIP",
                -22.41m,
                "USD",
                today.AddDays(-4),
                "Purchase",
                "Imported"),

            new MockBankTransaction(
                "mock-current-003",
                "mock-checking-001",
                "SQ *JOES COFFEE",
                -7.85m,
                "USD",
                today.AddDays(-5),
                "Purchase",
                "Imported"),

            new MockBankTransaction(
                "mock-current-004",
                "mock-checking-001",
                "SQ *JOES COFFEE",
                -7.85m,
                "USD",
                today.AddDays(-5).AddMinutes(3),
                "Purchase",
                "Imported"),

            new MockBankTransaction(
                "mock-current-005",
                "mock-checking-001",
                "UBER *TRIP",
                -31.20m,
                "USD",
                today.AddDays(-9),
                "Purchase",
                "Imported"),

            new MockBankTransaction(
                "mock-current-006",
                "mock-checking-001",
                "AMZN Mktp US*LARGE",
                -975.40m,
                "USD",
                today.AddDays(-2),
                "Purchase",
                "Imported"),

            new MockBankTransaction(
                "mock-current-007",
                "mock-checking-001",
                "TRANSFER TO SAVINGS",
                -500.00m,
                "USD",
                today.AddDays(-15),
                "Transfer",
                "Imported")
        );

        return transactions;
    }

    private static void AddMonthlySubscription(
        List<MockBankTransaction> transactions,
        string idPrefix,
        string description,
        IReadOnlyList<decimal> amounts,
        DateTimeOffset today,
        int dayOffset)
    {
        for (var index = 0; index < amounts.Count; index++)
        {
            var monthsAgo = amounts.Count - 1 - index;
            transactions.Add(
                new MockBankTransaction(
                    $"{idPrefix}-{index + 1:00}",
                    "mock-checking-001",
                    description,
                    -amounts[index],
                    "USD",
                    today.AddMonths(-monthsAgo).AddDays(-dayOffset),
                    "Purchase",
                    "Imported"));
        }
    }

    private static void AddMonthlySeries(
        List<MockBankTransaction> transactions,
        string idPrefix,
        string description,
        IReadOnlyList<(int MonthsAgo, decimal Amount)> series,
        DateTimeOffset today,
        int dayOffset)
    {
        for (var index = 0; index < series.Count; index++)
        {
            var point = series[index];
            transactions.Add(
                new MockBankTransaction(
                    $"{idPrefix}-{index + 1:00}",
                    "mock-checking-001",
                    description,
                    -point.Amount,
                    "USD",
                    today.AddMonths(-point.MonthsAgo).AddDays(-dayOffset),
                    "Purchase",
                    "Imported"));
        }
    }

    private static void AddAmazonHistory(
        List<MockBankTransaction> transactions,
        DateTimeOffset today)
    {
        var charges =
            new[]
            {
                ("mock-amazon-001", 48.20m, -152),
                ("mock-amazon-002", 64.10m, -121),
                ("mock-amazon-003", 55.40m, -94),
                ("mock-amazon-004", 72.15m, -63),
                ("mock-amazon-005", 41.85m, -34),
                ("mock-amazon-006", 59.30m, -21)
            };

        foreach (var charge in charges)
        {
            transactions.Add(
                new MockBankTransaction(
                    charge.Item1,
                    "mock-checking-001",
                    "AMZN Mktp US",
                    -charge.Item2,
                    "USD",
                    today.AddDays(charge.Item3),
                    "Purchase",
                    "Imported"));
        }
    }
}
