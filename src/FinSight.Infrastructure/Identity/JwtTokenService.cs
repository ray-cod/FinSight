using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinSight.Application.Abstractions.Identity;
using FinSight.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinSight.Infrastructure.Identity;

/// <summary>
/// Creates signed JWT access tokens for authenticated users.
/// </summary>
public sealed class JwtTokenService(
    IOptions<JwtOptions> jwtOptions)
    : ITokenService
{
    private readonly JwtOptions _options =
        jwtOptions.Value;

    /// <inheritdoc />
    public Task<AccessTokenResult> CreateAccessTokenAsync(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var expiresAt =
            DateTimeOffset.UtcNow
            .Add(_options.AccessTokenLifetime);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                userId.ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                email),

            new(
                ClaimTypes.NameIdentifier,
                userId.ToString()),

            new(
                ClaimTypes.Email,
                email)
        };

        claims.AddRange(
            roles.Select(
                role =>
                    new Claim(
                        ClaimTypes.Role,
                        role)));

        var credentials =
            new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        _options.SigningKey)),
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt.UtcDateTime,
                signingCredentials: credentials);

        return Task.FromResult(
            new AccessTokenResult(
                new JwtSecurityTokenHandler()
                    .WriteToken(token),
                expiresAt));
    }
}
