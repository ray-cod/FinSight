using FinSight.Infrastructure.Banking.MockBank.Models;

namespace FinSight.Infrastructure.Banking.MockBank;

/// <summary>
/// Provides deterministic mock financial data for local development.
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
    /// Gets the mock bank transaction catalogue.
    /// </summary>
    public static IReadOnlyCollection<MockBankTransaction> Transactions =>
    [
        new(
            "mock-tx-001",
            "mock-checking-001",
            "AMZN Mktp US*2K8F91",
            -84.72m,
            "USD",
            DateTimeOffset.UtcNow.AddDays(-1),
            "Purchase",
            "Imported"),

        new(
            "mock-tx-002",
            "mock-checking-001",
            "NETFLIX.COM",
            -15.99m,
            "USD",
            DateTimeOffset.UtcNow.AddDays(-3),
            "Purchase",
            "Imported"),

        new(
            "mock-tx-003",
            "mock-checking-001",
            "UBER *TRIP",
            -22.41m,
            "USD",
            DateTimeOffset.UtcNow.AddDays(-4),
            "Purchase",
            "Imported"),

        new(
            "mock-tx-004",
            "mock-checking-001",
            "SQ *JOES COFFEE",
            -7.85m,
            "USD",
            DateTimeOffset.UtcNow.AddDays(-5),
            "Purchase",
            "Imported"),

        new(
            "mock-tx-005",
            "mock-checking-001",
            "SALARY ACME CORP",
            4200.00m,
            "USD",
            DateTimeOffset.UtcNow.AddDays(-7),
            "Deposit",
            "Imported"),

        new(
            "mock-tx-006",
            "mock-checking-001",
            "SPOTIFY AB",
            -12.99m,
            "USD",
            DateTimeOffset.UtcNow.AddDays(-10),
            "Purchase",
            "Imported"),

        new(
            "mock-tx-007",
            "mock-checking-001",
            "WOOLWORTHS 1234",
            -143.21m,
            "USD",
            DateTimeOffset.UtcNow.AddDays(-11),
            "Purchase",
            "Imported"),

        new(
            "mock-tx-008",
            "mock-checking-001",
            "TRANSFER TO SAVINGS",
            -500.00m,
            "USD",
            DateTimeOffset.UtcNow.AddDays(-15),
            "Transfer",
            "Imported"),

        new(
            "mock-netflix-jan",
            "mock-checking-001",
            "NETFLIX.COM",
            -15.99m,
            "USD",
            new DateTimeOffset(
                2026,
                1,
                5,
                12,
                0,
                0,
                TimeSpan.Zero),
            "Purchase",
            "Imported"),

        new(
            "mock-netflix-feb",
            "mock-checking-001",
            "NETFLIX.COM",
            -15.99m,
            "USD",
            new DateTimeOffset(
                2026,
                2,
                5,
                12,
                0,
                0,
                TimeSpan.Zero),
            "Purchase",
            "Imported"),

        new(
            "mock-netflix-mar",
            "mock-checking-001",
            "NETFLIX.COM",
            -15.99m,
            "USD",
            new DateTimeOffset(
                2026,
                3,
                5,
                12,
                0,
                0,
                TimeSpan.Zero),
            "Purchase",
            "Imported"),

        new(
            "mock-netflix-apr",
            "mock-checking-001",
            "NETFLIX.COM",
            -17.99m,
            "USD",
            new DateTimeOffset(
                2026,
                4,
                5,
                12,
                0,
                0,
                TimeSpan.Zero),
            "Purchase",
            "Imported"),
    ];
}
