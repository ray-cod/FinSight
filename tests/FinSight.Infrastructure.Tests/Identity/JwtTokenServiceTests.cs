using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FinSight.Infrastructure.Configuration;
using FinSight.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Xunit;

namespace FinSight.Infrastructure.Tests.Identity;

/// <summary>
/// Verifies session identity claims on FinSight access tokens.
/// </summary>
public sealed class JwtTokenServiceTests
{
    /// <summary>
    /// Verifies that the session identifier is included in the access token claims.
    /// </summary>
    [Fact]
    public async Task CreateAccessTokenAsyncIncludesRefreshSessionIdentifier()
    {
        var options = Options.Create(
            new JwtOptions
            {
                Issuer = "FinSight.Tests",
                Audience = "FinSight.Tests",
                SigningKey = new string('x', 64),
                AccessTokenLifetime = TimeSpan.FromMinutes(15),
                RefreshTokenLifetime = TimeSpan.FromDays(30)
            });

        var service = new JwtTokenService(options);
        var sessionId = Guid.NewGuid();

        var result = await service.CreateAccessTokenAsync(
            Guid.NewGuid(),
            "user@example.com",
            ["User"],
            sessionId);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(token.Claims));

        Assert.Equal(
            sessionId.ToString(),
            principal.FindFirstValue(JwtRegisteredClaimNames.Sid));
        Assert.Equal(
            sessionId.ToString(),
            principal.FindFirstValue(ClaimTypes.Sid));
    }
}
