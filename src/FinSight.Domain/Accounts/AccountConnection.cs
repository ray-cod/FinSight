using FinSight.Domain.Common;

namespace FinSight.Domain.Accounts;

/// <summary>
/// Represents a user's connection to an external financial institution.
/// </summary>
public sealed class AccountConnection
    : AggregateRoot<Guid>
{
    private AccountConnection()
    {
    }

    private AccountConnection(
        Guid id,
        Guid userId,
        Guid institutionId,
        string externalConnectionId)
        : base(id)
    {
        UserId = userId;
        InstitutionId = institutionId;
        ExternalConnectionId =
            NormalizeExternalId(
                externalConnectionId);

        Status = ConnectionStatus.Connected;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the user who owns this connection.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the connected institution identifier.
    /// </summary>
    public Guid InstitutionId { get; private set; }

    /// <summary>
    /// Gets the provider-specific connection identifier.
    /// </summary>
    public string ExternalConnectionId { get; private set; } = null!;

    /// <summary>
    /// Gets the current connection status.
    /// </summary>
    public ConnectionStatus Status { get; private set; }

    /// <summary>
    /// Gets the last successful synchronization timestamp.
    /// </summary>
    public DateTimeOffset? LastSuccessfulSyncAt { get; private set; }

    /// <summary>
    /// Gets the last attempted synchronization timestamp.
    /// </summary>
    public DateTimeOffset? LastSyncAttemptAt { get; private set; }

    /// <summary>
    /// Gets the most recent synchronization error.
    /// </summary>
    public string? LastSyncError { get; private set; }

    /// <summary>
    /// Gets the current provider synchronization cursor.
    /// </summary>
    public string? SyncCursor { get; private set; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the last modification timestamp.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>
    /// Creates a new financial institution connection.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="institutionId">The institution identifier.</param>
    /// <param name="externalConnectionId">
    /// The provider's connection identifier.
    /// </param>
    /// <returns>A new account connection.</returns>
    public static AccountConnection Create(
        Guid userId,
        Guid institutionId,
        string externalConnectionId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier cannot be empty.",
                nameof(userId));
        }

        if (institutionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Institution identifier cannot be empty.",
                nameof(institutionId));
        }

        return new AccountConnection(
            Guid.NewGuid(),
            userId,
            institutionId,
            externalConnectionId);
    }

    /// <summary>
    /// Marks the connection as synchronizing.
    /// </summary>
    public void BeginSync()
    {
        Status = ConnectionStatus.Syncing;
        LastSyncAttemptAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        LastSyncError = null;
    }

    /// <summary>
    /// Marks the synchronization as successful.
    /// </summary>
    /// <param name="nextCursor">
    /// The cursor to use for the next incremental synchronization.
    /// </param>
    public void CompleteSync(
        string? nextCursor)
    {
        Status = ConnectionStatus.Connected;
        LastSuccessfulSyncAt = DateTimeOffset.UtcNow;
        SyncCursor = nextCursor;
        UpdatedAt = DateTimeOffset.UtcNow;
        LastSyncError = null;
    }

    /// <summary>
    /// Marks the connection synchronization as failed.
    /// </summary>
    /// <param name="error">The synchronization error description.</param>
    public void FailSync(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        Status = ConnectionStatus.Failed;
        LastSyncAttemptAt = DateTimeOffset.UtcNow;
        LastSyncError = error;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the connection as requiring reauthorization.
    /// </summary>
    public void RequireReauthorization()
    {
        Status = ConnectionStatus.ReauthorizationRequired;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Disconnects the institution connection.
    /// </summary>
    public void Disconnect()
    {
        Status = ConnectionStatus.Disconnected;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeExternalId(
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim();
    }
}
