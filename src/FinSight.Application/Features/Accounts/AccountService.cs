using FinSight.Application.Abstractions.Banking;
using FinSight.Application.Abstractions.Identity;
using FinSight.Application.Abstractions.Messaging;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Contracts.Events;
using FinSight.Domain.Accounts;
using Microsoft.Extensions.Logging;

namespace FinSight.Application.Features.Accounts;

/// <summary>
/// Coordinates financial-account connection and retrieval workflows.
/// </summary>
public sealed partial class AccountService(
    IInstitutionRepository institutionRepository,
    IAccountConnectionRepository connectionRepository,
    IFinancialAccountRepository accountRepository,
    IBankProvider bankProvider,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork,
    ILogger<AccountService> logger)
{
    /// <summary>
    /// Connects a user to a mock financial institution.
    /// </summary>
    /// <param name="userId">The authenticated user identifier.</param>
    /// <param name="request">The institution connection request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created connection identifier.</returns>
    public async Task<Guid> ConnectAsync(
        Guid userId,
        ConnectAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var institution =
            await institutionRepository.GetByProviderCodeAsync(
                request.InstitutionCode,
                cancellationToken);

        if (institution is null ||
            !institution.IsActive)
        {
            throw new KeyNotFoundException(
                "The requested financial institution is not supported.");
        }

        var externalConnectionId =
            await bankProvider.ConnectAsync(
                userId,
                institution.ProviderCode,
                cancellationToken);

        var connection =
            AccountConnection.Create(
                userId,
                institution.Id,
                externalConnectionId);

        connectionRepository.Add(connection);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        await eventPublisher.PublishAsync(
            new AccountConnectedEvent
            {
                EventId = Guid.NewGuid(),
                UserId = userId,
                ConnectionId = connection.Id,
                InstitutionId = institution.Id,
                OccurredAt = DateTimeOffset.UtcNow
            },
            "account.connected",
            cancellationToken);

        LogConnectionCreated(
            connection.Id,
            userId);

        return connection.Id;
    }

    /// <summary>
    /// Gets all financial accounts belonging to a user.
    /// </summary>
    /// <param name="userId">The authenticated user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's accounts.</returns>
    public async Task<IReadOnlyList<AccountResponse>> GetAccountsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var accounts =
            await accountRepository.GetByUserIdAsync(
                userId,
                cancellationToken);

        return accounts
            .Select(
                account =>
                    new AccountResponse(
                        account.Id.Value,
                        account.InstitutionId,
                        account.Name,
                        account.Type,
                        account.Currency,
                        account.CurrentBalance,
                        account.AvailableBalance,
                        account.Status))
            .ToArray();
    }

    /// <summary>
    /// Gets a user's financial account by identifier.
    /// </summary>
    /// <param name="userId">The authenticated user identifier.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested account.</returns>
    public async Task<AccountResponse> GetAccountAsync(
        Guid userId,
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var account =
            await accountRepository.GetByIdAsync(
                userId,
                accountId,
                cancellationToken);

        if (account is null)
        {
            throw new KeyNotFoundException(
                "Financial account was not found.");
        }

        return new AccountResponse(
            account.Id.Value,
            account.InstitutionId,
            account.Name,
            account.Type,
            account.Currency,
            account.CurrentBalance,
            account.AvailableBalance,
            account.Status);
    }

    /// <summary>
    /// Disconnects a financial institution connection.
    /// </summary>
    /// <param name="userId">The authenticated user.</param>
    /// <param name="connectionId">The connection identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DisconnectAsync(
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

        await bankProvider.DisconnectAsync(
            connection.ExternalConnectionId,
            cancellationToken);

        connection.Disconnect();

        await unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Financial institution connection {ConnectionId} created for user {UserId}.")]
    private partial void LogConnectionCreated(Guid connectionId, Guid userId);
}
