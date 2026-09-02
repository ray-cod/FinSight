using FinSight.Domain.Accounts;
using FluentAssertions;

namespace FinSight.Domain.Tests.Accounts;

/// <summary>
/// Tests financial account domain behavior.
/// </summary>
public sealed class FinancialAccountTests
{
    /// <summary>
    /// Verifies that a valid account can be created.
    /// </summary>
    [Fact]
    public void CreateShouldCreateActiveAccount()
    {
        var account =
            FinancialAccount.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "mock-account-001",
                "Checking",
                AccountType.Checking,
                "USD",
                1000m,
                900m);

        account.Status
            .Should()
            .Be(AccountStatus.Active);

        account.Currency
            .Should()
            .Be("USD");

        account.CurrentBalance
            .Should()
            .Be(1000m);
    }

    /// <summary>
    /// Verifies that balances can be updated.
    /// </summary>
    [Fact]
    public void UpdateBalancesShouldUpdateAccountBalances()
    {
        var account =
            FinancialAccount.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "mock-account-001",
                "Checking",
                AccountType.Checking,
                "USD",
                1000m,
                900m);

        account.UpdateBalances(
            1500m,
            1400m);

        account.CurrentBalance
            .Should()
            .Be(1500m);

        account.AvailableBalance
            .Should()
            .Be(1400m);
    }
}
