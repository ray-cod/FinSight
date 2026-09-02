using FinSight.Domain.Accounts;

namespace FinSight.Application.Abstractions.Persistence;

/// <summary>
/// Provides persistence operations for bank account connections.
/// </summary>
public interface IAccountConnectionRepository
{
    /// <summary>
    /// Gets a user's connection by identifier.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="connectionId">The connection identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The connection when found.</returns>
    Task<AccountConnection?> GetByIdAsync(
        Guid userId,
        Guid connectionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active connections for a user.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's active connections.</returns>
    Task<IReadOnlyList<AccountConnection>> GetActiveAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a connection using its external provider identifier.
    /// </summary>
    /// <param name="externalConnectionId">
    /// Provider connection identifier.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching connection.</returns>
    Task<AccountConnection?> GetByExternalIdAsync(
        string externalConnectionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new connection.
    /// </summary>
    /// <param name="connection">The connection to add.</param>
    void Add(AccountConnection connection);

    /// <summary>
    /// Gets all active connections across users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All active connections.</returns>
    Task<IReadOnlyList<AccountConnection>> GetAllActiveAsync(
        CancellationToken cancellationToken = default);
}
