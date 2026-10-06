using System.Net;

namespace Warehouse.BusinessLayer.Services.Email;

internal static class EmailTemplates
{
    private const string Accent = "#1F5F4B";
    private const string Ink = "#16191C";
    private const string Muted = "#5A6169";

    public static (string Html, string Text) PasswordReset(string firstName, string link, int minutes)
    {
        var name = WebUtility.HtmlEncode(firstName);
        var href = WebUtility.HtmlEncode(link);

        var body = $"""
            <p style="margin:0 0 16px">Zdravo {name},</p>
            <p style="margin:0 0 16px">Zatražena je promena lozinke za vaš nalog u aplikaciji Skladišnik.
            Kliknite na dugme ispod da postavite novu lozinku.</p>
            <p style="margin:24px 0">
              <a href="{href}" style="background:{Accent};color:#ffffff;text-decoration:none;padding:12px 22px;border-radius:6px;display:inline-block;font-weight:600">Postavi novu lozinku</a>
            </p>
            <p style="margin:0 0 16px;color:{Muted}">Link važi {minutes} minuta i može se iskoristiti samo jednom.
            Ako niste vi zatražili promenu, ignorišite ovaj mejl — lozinka ostaje ista.</p>
            <p style="margin:0;color:{Muted};font-size:12px;word-break:break-all">Ako dugme ne radi, otvorite ovu adresu: {href}</p>
            """;

        var text =
            $"Zdravo {firstName},\n\n" +
            "Zatražena je promena lozinke za vaš nalog u aplikaciji Skladišnik.\n" +
            $"Otvorite ovaj link da postavite novu lozinku:\n{link}\n\n" +
            $"Link važi {minutes} minuta i može se iskoristiti samo jednom.\n" +
            "Ako niste vi zatražili promenu, ignorišite ovaj mejl.";

        return (Layout("Promena lozinke", body), text);
    }

    public static (string Html, string Text) PasswordChanged(string firstName, DateTime changedAtLocal)
    {
        var name = WebUtility.HtmlEncode(firstName);
        var when = changedAtLocal.ToString("dd.MM.yyyy. u HH:mm");

        var body = $"""
            <p style="margin:0 0 16px">Zdravo {name},</p>
            <p style="margin:0 0 16px">Lozinka za vaš nalog u aplikaciji Skladišnik promenjena je {when}.
            Sve aktivne sesije su prekinute, pa se ponovo prijavite novom lozinkom.</p>
            <p style="margin:0;color:{Muted}">Ako niste vi promenili lozinku, odmah se obratite administratoru sistema.</p>
            """;

        var text =
            $"Zdravo {firstName},\n\n" +
            $"Lozinka za vaš nalog u aplikaciji Skladišnik promenjena je {when}.\n" +
            "Sve aktivne sesije su prekinute.\n\n" +
            "Ako niste vi promenili lozinku, odmah se obratite administratoru sistema.";

        return (Layout("Lozinka je promenjena", body), text);
    }

    private static string Layout(string title, string body) => $"""
        <!doctype html>
        <html lang="sr">
        <body style="margin:0;padding:24px;background:#F2F4F5;font-family:Segoe UI,Arial,sans-serif;color:{Ink};font-size:15px;line-height:1.6">
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
            <tr><td align="center">
              <table role="presentation" width="560" cellpadding="0" cellspacing="0" style="background:#ffffff;border:1px solid #DDE2E5;border-radius:8px">
                <tr><td style="padding:20px 28px;border-bottom:3px solid {Accent}">
                  <span style="font-size:18px;font-weight:600;color:{Accent}">Skladišnik</span>
                </td></tr>
                <tr><td style="padding:28px">
                  <h1 style="font-size:20px;margin:0 0 20px">{title}</h1>
                  {body}
                </td></tr>
              </table>
              <p style="color:{Muted};font-size:12px;margin:16px 0 0">Automatska poruka — ne odgovarajte na ovaj mejl.</p>
            </td></tr>
          </table>
        </body>
        </html>
        """;
}
