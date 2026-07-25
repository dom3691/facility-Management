namespace FacilityInspection.Infrastructure.Configuration;

public class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary><c>Logging</c> (default) or <c>Smtp</c>.</summary>
    public string Provider { get; set; } = "Logging";

    public string FromAddress { get; set; } = "noreply@facility-inspection.local";

    public string FromName { get; set; } = "Facility Inspection System";

    public SmtpOptions Smtp { get; set; } = new();
}

public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool EnableSsl { get; set; } = true;
}
