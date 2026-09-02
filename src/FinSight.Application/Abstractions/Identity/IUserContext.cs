namespace FinSight.Application.Abstractions.Identity;

/// <summary>
/// Provides information about the currently authenticated request.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the current user identifier.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Gets a value indicating whether the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
