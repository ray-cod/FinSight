namespace FinSight.Application.Abstractions.Identity;

/// <summary>
/// Provides password-reset operations.
/// </summary>
public interface IPasswordResetService
{
    /// <summary>
    /// Requests a password reset for an email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RequestResetAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a user's password using a reset token.
    /// </summary>
    /// <param name="request">Password reset request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ResetAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);
}
