namespace FinSight.Infrastructure.Identity;

/// <summary>
/// Represents a persisted refresh token.
/// </summary>
public sealed class RefreshToken
{
    /// <summary>
    /// Gets or sets the refresh token identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the cryptographic hash of the token.
    /// </summary>
    public string TokenHash { get; set; } = null!;

    /// <summary>
    /// Gets or sets the token expiration timestamp.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the token creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the originating IP address.
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// Gets or sets the revocation timestamp.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets the replacement token identifier.
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the refresh token is currently active.
    /// </summary>
    public bool IsActive =>
        RevokedAt is null &&
        ExpiresAt > DateTimeOffset.UtcNow;
}
