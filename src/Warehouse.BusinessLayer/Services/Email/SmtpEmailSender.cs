using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Warehouse.BusinessLayer.Settings;

namespace Warehouse.BusinessLayer.Services.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(IOptions<EmailSettings> settings) => _settings = settings.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        mime.To.Add(new MailboxAddress(message.ToName, message.To));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder { HtmlBody = message.HtmlBody, TextBody = message.TextBody }.ToMessageBody();

        using var client = new SmtpClient();

        var security = _settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(_settings.Host, _settings.Port, security, ct);

        if (!string.IsNullOrWhiteSpace(_settings.Username))
            await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);

        await client.SendAsync(mime, ct);
        await client.DisconnectAsync(true, ct);
    }
}
