using System.Globalization;
using FinSight.Application.Abstractions.Banking;
using FinSight.Domain.Accounts;
using FinSight.Domain.Transactions;
using FinSight.Infrastructure.Banking.MockBank.Models;

namespace FinSight.Infrastructure.Banking.MockBank;

/// <summary>
/// Simulates retrieving accounts and transactions from a bank provider.
/// </summary>
public sealed class MockBankTransactionProvider
    : IBankTransactionProvider
{
    /// <inheritdoc />
    public async Task<BankSyncResult> SyncAsync(
        string externalConnectionId,
        string? cursor,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Delay(
            TimeSpan.FromMilliseconds(150),
            cancellationToken);

        var accounts =
            MockBankData.Accounts
                .Select(MapAccount)
                .ToArray();

        var transactions =
            MockBankData.Transactions
                .Select(MapTransaction)
                .ToArray();

        return new BankSyncResult(
            transactions,
            accounts,
            NextCursor: DateTimeOffset.UtcNow
                .ToUnixTimeSeconds()
                .ToString(CultureInfo.InvariantCulture),
            HasMore: false);
    }

    private static BankAccountData MapAccount(
        MockBankAccount account)
    {
        var type =
            Enum.Parse<AccountType>(
                account.Type,
                ignoreCase: true);

        return new BankAccountData(
            account.ExternalAccountId,
            account.Name,
            type,
            account.Currency,
            account.CurrentBalance,
            account.AvailableBalance);
    }

    private static BankTransactionData MapTransaction(
        MockBankTransaction transaction)
    {
        return new BankTransactionData(
            transaction.AccountId,
            transaction.ExternalTransactionId,
            transaction.Description,
            transaction.Amount,
            transaction.Currency,
            transaction.TransactionDate,
            Enum.Parse<TransactionType>(
                transaction.Type,
                ignoreCase: true),
            Enum.Parse<TransactionStatus>(
                transaction.Status,
                ignoreCase: true));
    }
}
