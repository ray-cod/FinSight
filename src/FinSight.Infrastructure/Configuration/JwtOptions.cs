namespace FinSight.Infrastructure.Configuration;

/// <summary>
/// Represents JWT authentication configuration.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Gets the token issuer.
    /// </summary>
    public required string Issuer { get; init; }

    /// <summary>
    /// Gets the token audience.
    /// </summary>
    public required string Audience { get; init; }

    /// <summary>
    /// Gets the signing key.
    /// </summary>
    public required string SigningKey { get; init; }

    /// <summary>
    /// Gets the lifetime of an access token.
    /// </summary>
    public TimeSpan AccessTokenLifetime { get; init; } =
        TimeSpan.FromMinutes(15);

    /// <summary>
    /// Gets the lifetime of a refresh token.
    /// </summary>
    public TimeSpan RefreshTokenLifetime { get; init; } =
        TimeSpan.FromDays(30);
}
