using System.Security.Cryptography;
using System.Text;

namespace FinSight.Infrastructure.Identity;

/// <summary>
/// Generates and hashes cryptographically secure refresh tokens.
/// </summary>
public static class RefreshTokenGenerator
{
    /// <summary>
    /// Generates a cryptographically secure refresh token.
    /// </summary>
    /// <returns>A base64url-encoded random token.</returns>
    public static string Generate()
    {
        var bytes =
            RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", string.Empty);
    }

    /// <summary>
    /// Computes a SHA-256 hash for a refresh token.
    /// </summary>
    /// <param name="token">The plaintext token.</param>
    /// <returns>The token hash.</returns>
    public static string Hash(
        string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var hash =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(hash);
    }
}
