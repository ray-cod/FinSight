namespace FinSight.Domain.Accounts;

/// <summary>
/// Represents the synchronization state of a financial institution connection.
/// </summary>
public enum ConnectionStatus
{
    /// <summary>
    /// The connection is active and available for synchronization.
    /// </summary>
    Connected = 1,

    /// <summary>
    /// The connection is currently synchronizing.
    /// </summary>
    Syncing = 2,

    /// <summary>
    /// The connection needs user reauthorization.
    /// </summary>
    ReauthorizationRequired = 3,

    /// <summary>
    /// The connection has failed.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// The connection has been disconnected.
    /// </summary>
    Disconnected = 5
}
