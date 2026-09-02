using FinSight.Domain.Common;

namespace FinSight.Domain.Users;

/// <summary>
/// Represents a FinSight application user.
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    private User()
    {
    }

    private User(
        UserId id,
        string email,
        string displayName)
        : base(id)
    {
        Email = NormalizeEmail(email);
        DisplayName = NormalizeDisplayName(displayName);
        Status = UserStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the user's normalized email address.
    /// </summary>
    public string Email { get; private set; } = null!;

    /// <summary>
    /// Gets the user's display name.
    /// </summary>
    public string DisplayName { get; private set; } = null!;

    /// <summary>
    /// Gets the current account status.
    /// </summary>
    public UserStatus Status { get; private set; }

    /// <summary>
    /// Gets the time at which the account was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the time at which the account was last modified.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>
    /// Creates a new active user.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <returns>The created user.</returns>
    public static User Create(
        string email,
        string displayName)
    {
        return new User(
            UserId.New(),
            email,
            displayName);
    }

    /// <summary>
    /// Updates the user's display name.
    /// </summary>
    /// <param name="displayName">The new display name.</param>
    public void UpdateDisplayName(
        string displayName)
    {
        DisplayName =
            NormalizeDisplayName(displayName);

        UpdatedAt =
            DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Suspends the user account.
    /// </summary>
    public void Suspend()
    {
        Status = UserStatus.Suspended;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Reactivates the user account.
    /// </summary>
    public void Reactivate()
    {
        Status = UserStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string NormalizeEmail(
        string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return email.Trim().ToLowerInvariant();
    }

    private static string NormalizeDisplayName(
        string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            displayName);

        var normalized =
            displayName.Trim();

        if (normalized.Length > 100)
        {
            throw new ArgumentException(
                "Display name cannot exceed 100 characters.",
                nameof(displayName));
        }

        return normalized;
    }
}
