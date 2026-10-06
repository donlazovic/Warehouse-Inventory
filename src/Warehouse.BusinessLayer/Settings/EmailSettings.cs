namespace Warehouse.BusinessLayer.Settings;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 2525;
    public bool UseStartTls { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "no-reply@skladisnik.local";
    public string FromName { get; set; } = "Skladišnik";
}

public class ClientSettings
{
    public const string SectionName = "Client";

    public string BaseUrl { get; set; } = "http://localhost:5173";
}
