namespace FinSight.Application.Abstractions.Banking;

/// <summary>
/// Represents an external banking institution provider.
/// </summary>
public interface IBankProvider
{
    /// <summary>
    /// Gets the provider identifier.
    /// </summary>
    string ProviderCode { get; }

    /// <summary>
    /// Establishes a provider connection for a user.
    /// </summary>
    /// <param name="userId">The FinSight user identifier.</param>
    /// <param name="institutionCode">The provider institution code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The provider connection identifier.</returns>
    Task<string> ConnectAsync(
        Guid userId,
        string institutionCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects a provider connection.
    /// </summary>
    /// <param name="externalConnectionId">
    /// Provider connection identifier.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DisconnectAsync(
        string externalConnectionId,
        CancellationToken cancellationToken = default);
}
