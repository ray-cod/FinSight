using FinSight.Api.Extensions;
using FinSight.Application.Features.Accounts;
using FinSight.Application.Features.Transactions;
using FinSight.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides financial account management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/accounts")]
[Authorize(
    Policy =
        AuthorizationPolicies.Authenticated)]
public sealed class AccountsController(
    AccountService accountService,
    AccountSyncService syncService,
    TransactionService transactionService)
    : ControllerBase
{
    /// <summary>
    /// Gets all financial accounts owned by the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's financial accounts.</returns>
    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyList<AccountResponse>>> GetAccounts(
        CancellationToken cancellationToken)
    {
        var response =
            await accountService.GetAccountsAsync(
                User.GetRequiredUserId(),
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Gets a financial account owned by the authenticated user.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested account.</returns>
    [HttpGet("{accountId:guid}")]
    public async Task<ActionResult<AccountResponse>> GetAccount(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var response =
            await accountService.GetAccountAsync(
                User.GetRequiredUserId(),
                accountId,
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Connects the authenticated user to a financial institution.
    /// </summary>
    /// <param name="request">The institution connection request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created connection identifier.</returns>
    [HttpPost("connections")]
    public async Task<ActionResult<Guid>> Connect(
        ConnectAccountRequest request,
        CancellationToken cancellationToken)
    {
        var connectionId =
            await accountService.ConnectAsync(
                User.GetRequiredUserId(),
                request,
                cancellationToken);

        return Created(
            $"/api/v1/accounts/connections/{connectionId}",
            connectionId);
    }

    /// <summary>
    /// Synchronizes a connected financial institution.
    /// </summary>
    /// <param name="connectionId">
    /// The account connection identifier.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of newly imported transactions.</returns>
    [HttpPost("connections/{connectionId:guid}/sync")]
    public async Task<ActionResult<object>> Sync(
        Guid connectionId,
        CancellationToken cancellationToken)
    {
        var imported =
            await syncService.SyncAsync(
                User.GetRequiredUserId(),
                connectionId,
                cancellationToken);

        return Ok(
            new
            {
                importedTransactions = imported
            });
    }

    /// <summary>
    /// Disconnects a financial institution connection.
    /// </summary>
    /// <param name="connectionId">
    /// The account connection identifier.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("connections/{connectionId:guid}")]
    public async Task<IActionResult> Disconnect(
        Guid connectionId,
        CancellationToken cancellationToken)
    {
        await accountService.DisconnectAsync(
            User.GetRequiredUserId(),
            connectionId,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Gets imported transactions for a financial account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="limit">Maximum number of transactions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The account transactions.</returns>
    [HttpGet("{accountId:guid}/transactions")]
    public async Task<ActionResult<
        IReadOnlyList<TransactionResponse>>> GetTransactions(
        Guid accountId,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var response =
            await transactionService.GetForAccountAsync(
                User.GetRequiredUserId(),
                accountId,
                limit,
                cancellationToken);

        return Ok(response);
    }
}
