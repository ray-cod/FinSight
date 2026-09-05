using FinSight.Application.Abstractions.Identity;

namespace FinSight.Infrastructure.Identity;

/// <summary>
/// A null implementation of <see cref="IUserContactService"/> used when
/// ASP.NET Core Identity is not configured (e.g., in background workers).
/// Always returns null for email addresses.
/// </summary>
public sealed class NullUserContactService : IUserContactService
{
    /// <inheritdoc />
    public Task<string?> GetEmailAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Null object pattern: always return null when identity is not available
        return Task.FromResult<string?>(null);
    }
}
