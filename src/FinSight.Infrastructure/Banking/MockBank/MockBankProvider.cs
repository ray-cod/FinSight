using FinSight.Application.Abstractions.Banking;

namespace FinSight.Infrastructure.Banking.MockBank;

/// <summary>
/// Simulates connecting and disconnecting to an external bank provider.
/// </summary>
public sealed class MockBankProvider
    : IBankProvider
{
    /// <summary>
    /// Gets the provider identifier.
    /// </summary>
    public string ProviderCode =>
        "MOCK_BANK";

    /// <inheritdoc />
    public Task<string> ConnectAsync(
        Guid userId,
        string institutionCode,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (institutionCode != "MOCK_BANK")
        {
            throw new InvalidOperationException(
                $"Unsupported institution '{institutionCode}'.");
        }

        var connectionId =
            $"mock-connection-{userId:N}";

        return Task.FromResult(connectionId);
    }

    /// <inheritdoc />
    public Task DisconnectAsync(
        string externalConnectionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.CompletedTask;
    }
}
