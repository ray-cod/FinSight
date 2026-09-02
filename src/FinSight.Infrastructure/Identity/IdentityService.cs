using FinSight.Application.Abstractions.Identity;
using FinSight.Application.Abstractions.Security;
using FinSight.Domain.Security;
using FinSight.Infrastructure.Configuration;
using FinSight.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FinSight.Infrastructure.Identity;

/// <summary>
/// Provides user authentication and account security operations.
/// </summary>
public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    IAuditService auditService,
    FinSightDbContext dbContext,
    IOptions<JwtOptions> jwtOptions)
    : IAuthService
{
    private readonly JwtOptions _jwtOptions =
        jwtOptions.Value;

    /// <inheritdoc />
    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email =
            request.Email.Trim().ToLowerInvariant();

        var existing =
            await userManager.FindByEmailAsync(email);

        if (existing is not null)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var user =
            new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                DisplayName =
                    request.DisplayName.Trim(),
                CreatedAt =
                    DateTimeOffset.UtcNow
            };

        var result =
            await userManager.CreateAsync(
                user,
                request.Password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    " ",
                    result.Errors.Select(
                        error => error.Description)));
        }

        await userManager.AddToRoleAsync(
            user,
            "User");

        await auditService.RecordAsync(
            SecurityEventType.UserRegistered,
            user.Id,
            ipAddress,
            cancellationToken: cancellationToken);

        return await IssueTokensAsync(
            user,
            ipAddress,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email =
            request.Email.Trim().ToLowerInvariant();

        var user =
            await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            await auditService.RecordAsync(
                SecurityEventType.LoginFailed,
                null,
                ipAddress,
                new Dictionary<string, string>
                {
                    ["reason"] = "invalid_credentials"
                },
                cancellationToken);

            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            // We intentionally keep email confirmation optional
            // during this phase.
        }

        var result =
            await signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            await auditService.RecordAsync(
                SecurityEventType.LoginFailed,
                user.Id,
                ipAddress,
                new Dictionary<string, string>
                {
                    ["reason"] =
                        result.IsLockedOut
                            ? "locked_out"
                            : "invalid_credentials"
                },
                cancellationToken);

            throw new UnauthorizedAccessException(
                result.IsLockedOut
                    ? "The account is temporarily locked."
                    : "Invalid email or password.");
        }

        await auditService.RecordAsync(
            SecurityEventType.LoginSucceeded,
            user.Id,
            ipAddress,
            cancellationToken: cancellationToken);

        return await IssueTokensAsync(
            user,
            ipAddress,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AuthResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tokenHash =
            RefreshTokenGenerator.Hash(
                request.RefreshToken);

        var existing =
            await dbContext.RefreshTokens
                .SingleOrDefaultAsync(
                    token =>
                        token.TokenHash == tokenHash,
                    cancellationToken);

        if (existing is null || !existing.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Invalid or expired refresh token.");
        }

        var user =
            await userManager.FindByIdAsync(
                existing.UserId.ToString());

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "The associated account no longer exists.");
        }

        existing.RevokedAt =
            DateTimeOffset.UtcNow;

        var response =
            await IssueTokensAsync(
                user,
                ipAddress,
                cancellationToken);

        existing.ReplacedByTokenId =
            await GetLatestTokenIdAsync(
                user.Id,
                cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await auditService.RecordAsync(
            SecurityEventType.RefreshTokenRevoked,
            user.Id,
            ipAddress,
            cancellationToken: cancellationToken);

        await auditService.RecordAsync(
            SecurityEventType.RefreshTokenIssued,
            user.Id,
            ipAddress,
            cancellationToken: cancellationToken);

        return response;
    }

    /// <inheritdoc />
    public async Task LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tokenHash =
            RefreshTokenGenerator.Hash(
                request.RefreshToken);

        var existing =
            await dbContext.RefreshTokens
                .SingleOrDefaultAsync(
                    token =>
                        token.TokenHash == tokenHash,
                    cancellationToken);

        if (existing is null)
        {
            return;
        }

        if (existing.RevokedAt is null)
        {
            existing.RevokedAt =
                DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(
                cancellationToken);

            await auditService.RecordAsync(
                SecurityEventType.RefreshTokenRevoked,
                existing.UserId,
                null,
                cancellationToken: cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        if (user is null)
        {
            throw new KeyNotFoundException(
                "User was not found.");
        }

        var result =
            await userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    " ",
                    result.Errors.Select(
                        error => error.Description)));
        }

        await userManager.UpdateSecurityStampAsync(user);

        await dbContext.RefreshTokens
            .Where(token =>
                token.UserId == userId &&
                token.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters =>
                    setters.SetProperty(
                        token => token.RevokedAt,
                        DateTimeOffset.UtcNow),
                cancellationToken);

        await auditService.RecordAsync(
            SecurityEventType.PasswordChanged,
            userId,
            null,
            cancellationToken: cancellationToken);
    }

    private async Task<AuthResponse> IssueTokensAsync(
        ApplicationUser user,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var roles =
            await userManager.GetRolesAsync(user);

        var accessToken =
            await tokenService.CreateAccessTokenAsync(
                user.Id,
                user.Email!,
                roles,
                cancellationToken);

        var plaintextRefreshToken =
            RefreshTokenGenerator.Generate();

        var refreshToken =
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash =
                    RefreshTokenGenerator.Hash(
                        plaintextRefreshToken),
                ExpiresAt =
                    DateTimeOffset.UtcNow
                    .Add(
                        _jwtOptions.RefreshTokenLifetime),
                CreatedAt =
                    DateTimeOffset.UtcNow,
                CreatedByIp = ipAddress
            };

        dbContext.RefreshTokens.Add(
            refreshToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return new AuthResponse(
            accessToken.Token,
            plaintextRefreshToken,
            accessToken.ExpiresAt,
            user.Id);
    }

    private async Task<Guid?> GetLatestTokenIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.RefreshTokens
            .Where(token =>
                token.UserId == userId)
            .OrderByDescending(
                token => token.CreatedAt)
            .Select(token => (Guid?)token.Id)
            .FirstOrDefaultAsync(
                cancellationToken);
    }
}
