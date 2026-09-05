using FinSight.Application.Abstractions.Notifications;
using FinSight.Domain.Notifications;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FinSight.Infrastructure.Notifications;

/// <summary>
/// Sends notifications through an SMTP server.
/// </summary>
public sealed class SmtpNotificationSender(
    IOptions<SmtpOptions> options)
    : INotificationSender
{
    private readonly SmtpOptions _options =
        options.Value;

    /// <inheritdoc />
    public NotificationChannel Channel =>
        NotificationChannel.Email;

    /// <inheritdoc />
    public async Task SendAsync(
        Notification notification,
        string recipient,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            recipient);

        var message =
            new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                _options.FromName,
                _options.FromAddress));

        message.To.Add(
            MailboxAddress.Parse(
                recipient));

        message.Subject =
            notification.Title;

        message.Body =
            new TextPart("plain")
            {
                Text =
                    notification.Message
            };

        using var client =
            new SmtpClient();

        var socketSecurity =
            _options.UseTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

        await client.ConnectAsync(
            _options.Host,
            _options.Port,
            socketSecurity,
            cancellationToken);

        await client.AuthenticateAsync(
            _options.Username,
            _options.Password,
            cancellationToken);

        await client.SendAsync(
            message,
            cancellationToken);

        await client.DisconnectAsync(
            true,
            cancellationToken);
    }
}
