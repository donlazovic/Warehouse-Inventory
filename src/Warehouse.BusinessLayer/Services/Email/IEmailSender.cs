namespace Warehouse.BusinessLayer.Services.Email;

public record EmailMessage(string To, string ToName, string Subject, string HtmlBody, string TextBody);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
