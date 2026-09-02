namespace FinSight.Application.Abstractions.Banking;

/// <summary>
/// Provides account and transaction synchronization operations.
/// </summary>
public interface IBankTransactionProvider
{
    /// <summary>
    /// Retrieves accounts and transactions from a banking provider.
    /// </summary>
    /// <param name="externalConnectionId">
    /// Provider connection identifier.
    /// </param>
    /// <param name="cursor">
    /// Previously stored synchronization cursor.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Synchronization data.</returns>
    Task<BankSyncResult> SyncAsync(
        string externalConnectionId,
        string? cursor,
        CancellationToken cancellationToken = default);
}
