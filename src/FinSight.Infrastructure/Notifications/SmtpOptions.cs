namespace FinSight.Infrastructure.Notifications;

/// <summary>
/// Represents SMTP delivery configuration.
/// </summary>
public sealed class SmtpOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Smtp";

    /// <summary>
    /// Gets the SMTP server host.
    /// </summary>
    public required string Host { get; init; }

    /// <summary>
    /// Gets the SMTP server port.
    /// </summary>
    public int Port { get; init; } = 587;

    /// <summary>
    /// Gets the SMTP username.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets the SMTP password.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Gets the sender email address.
    /// </summary>
    public required string FromAddress { get; init; }

    /// <summary>
    /// Gets the sender display name.
    /// </summary>
    public string FromName { get; init; } = "FinSight";

    /// <summary>
    /// Gets whether TLS should be used.
    /// </summary>
    public bool UseTls { get; init; } = true;
}
