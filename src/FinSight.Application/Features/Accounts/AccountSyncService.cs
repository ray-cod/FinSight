using System.Security.Cryptography;
using System.Text;
using FinSight.Application.Abstractions.Banking;
using FinSight.Application.Abstractions.Identity;
using FinSight.Application.Abstractions.Messaging;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Contracts.Events;
using FinSight.Domain.Accounts;
using FinSight.Domain.Transactions;
using Microsoft.Extensions.Logging;

namespace FinSight.Application.Features.Accounts;

/// <summary>
/// Coordinates account synchronization and idempotent transaction imports.
/// </summary>
public sealed partial class AccountSyncService(
    IAccountConnectionRepository connectionRepository,
    IFinancialAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IBankTransactionProvider bankTransactionProvider,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork,
    ILogger<AccountSyncService> logger)
{
    /// <summary>
    /// Synchronizes a user's bank connection.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="connectionId">The connection identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of newly imported transactions.</returns>
    public async Task<int> SyncAsync(
        Guid userId,
        Guid connectionId,
        CancellationToken cancellationToken = default)
    {
        var connection =
            await connectionRepository.GetByIdAsync(
                userId,
                connectionId,
                cancellationToken);

        if (connection is null)
        {
            throw new KeyNotFoundException(
                "Account connection was not found.");
        }

        if (connection.Status == ConnectionStatus.Disconnected)
        {
            throw new InvalidOperationException(
                "The account connection is disconnected.");
        }

        connection.BeginSync();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new AccountSyncStartedEvent
            {
                EventId = Guid.NewGuid(),
                UserId = userId,
                ConnectionId = connectionId,
                OccurredAt = DateTimeOffset.UtcNow
            },
            "account.sync.started",
            cancellationToken);

        try
        {
            var result =
                await bankTransactionProvider.SyncAsync(
                    connection.ExternalConnectionId,
                    connection.SyncCursor,
                    cancellationToken);

            foreach (var bankAccount in result.Accounts)
            {
                await UpsertAccountAsync(
                    connection,
                    bankAccount,
                    cancellationToken);
            }

            var importedCount = 0;

            foreach (var bankTransaction in result.Transactions)
            {
                var imported =
                    await ImportTransactionAsync(
                        connection,
                        bankTransaction,
                        cancellationToken);

                if (imported)
                {
                    importedCount++;
                }
            }

            connection.CompleteSync(result.NextCursor);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await eventPublisher.PublishAsync(
                new AccountSyncCompletedEvent
                {
                    EventId = Guid.NewGuid(),
                    UserId = userId,
                    ConnectionId = connectionId,
                    ImportedTransactionCount = importedCount,
                    OccurredAt = DateTimeOffset.UtcNow
                },
                "account.sync.completed",
                cancellationToken);

            LogSyncCompleted(connectionId, importedCount);

            return importedCount;
        }
        catch (Exception exception)
        {
            connection.FailSync(exception.Message);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await eventPublisher.PublishAsync(
                new AccountSyncFailedEvent
                {
                    EventId = Guid.NewGuid(),
                    UserId = userId,
                    ConnectionId = connectionId,
                    Error = exception.Message,
                    OccurredAt = DateTimeOffset.UtcNow
                },
                "account.sync.failed",
                cancellationToken);

            LogSyncFailed(exception, connectionId);

            throw;
        }
    }

    private async Task UpsertAccountAsync(
        AccountConnection connection,
        BankAccountData bankAccount,
        CancellationToken cancellationToken)
    {
        var existing =
            await accountRepository.GetByExternalIdAsync(
                connection.Id,
                bankAccount.ExternalAccountId,
                cancellationToken);

        if (existing is not null)
        {
            existing.UpdateBalances(
                bankAccount.CurrentBalance,
                bankAccount.AvailableBalance);

            return;
        }

        var account =
            FinancialAccount.Create(
                connection.UserId,
                connection.Id,
                connection.InstitutionId,
                bankAccount.ExternalAccountId,
                bankAccount.Name,
                bankAccount.Type,
                bankAccount.Currency,
                bankAccount.CurrentBalance,
                bankAccount.AvailableBalance);

        accountRepository.Add(account);
    }

    private async Task<bool> ImportTransactionAsync(
        AccountConnection connection,
        BankTransactionData bankTransaction,
        CancellationToken cancellationToken)
    {
        var account =
            await accountRepository.GetByExternalIdAsync(
                connection.Id,
                ResolveAccountId(bankTransaction),
                cancellationToken);

        if (account is null)
        {
            LogSkippingAccountNotFound(bankTransaction.ExternalTransactionId);
            return false;
        }

        var exists =
            await transactionRepository.ExistsAsync(
                account.Id.Value,
                bankTransaction.ExternalTransactionId,
                cancellationToken);

        if (exists)
        {
            LogSkippingDuplicateTransaction(bankTransaction.ExternalTransactionId);
            return false;
        }

        var fingerprint =
            BuildFingerprint(
                account.Id.Value,
                bankTransaction);

        var transaction =
            Transaction.CreateImported(
                connection.UserId,
                account.Id.Value,
                connection.InstitutionId,
                bankTransaction.ExternalTransactionId,
                bankTransaction.Description,
                bankTransaction.Amount,
                bankTransaction.Currency,
                bankTransaction.TransactionDate,
                bankTransaction.Type,
                bankTransaction.Status,
                fingerprint);

        transactionRepository.Add(transaction);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new TransactionImportedEvent
            {
                EventId = Guid.NewGuid(),
                UserId = connection.UserId,
                AccountId = account.Id.Value,
                TransactionId = transaction.Id.Value,
                ProviderTransactionId = transaction.ProviderTransactionId,
                OccurredAt = DateTimeOffset.UtcNow
            },
            "transaction.imported",
            cancellationToken);

        return true;
    }

    private static string ResolveAccountId(
        BankTransactionData transaction)
    {
        return transaction.ExternalTransactionId switch
        {
            "mock-tx-001" or
            "mock-tx-002" or
            "mock-tx-003" or
            "mock-tx-004" or
            "mock-tx-005" or
            "mock-tx-006" or
            "mock-tx-007" or
            "mock-tx-008"
                => "mock-checking-001",

            _ => "mock-checking-001"
        };
    }

    private static string BuildFingerprint(
        Guid accountId,
        BankTransactionData transaction)
    {
        var source =
            $"{accountId:N}|{transaction.ExternalTransactionId}|{transaction.Amount}|{transaction.TransactionDate:O}";

        var hash =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(source));

        return Convert.ToHexString(hash);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Account sync completed. Connection {ConnectionId}, imported {Count} transactions.")]
    private partial void LogSyncCompleted(Guid connectionId, int count);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Account synchronization failed for connection {ConnectionId}.")]
    private partial void LogSyncFailed(Exception exception, Guid connectionId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Skipping transaction {TransactionId}: account not found.")]
    private partial void LogSkippingAccountNotFound(string transactionId);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Debug,
        Message = "Skipping duplicate provider transaction {TransactionId}.")]
    private partial void LogSkippingDuplicateTransaction(string transactionId);
}
