namespace BrushEssence.Infrastructure.Email;

/// <summary>
/// SMTP settings bound from the "Email" configuration section. When
/// <see cref="Host"/> is blank, no real provider is wired up and emails are
/// logged instead (handy for local development).
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>SMTP host. Leave blank to disable real delivery (log instead).</summary>
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Address shown in the From header.</summary>
    public string FromEmail { get; set; } = "no-reply@brushessence.local";

    public string FromName { get; set; } = "Brush Essence";

    /// <summary>Use STARTTLS (true for most providers on port 587).</summary>
    public bool UseStartTls { get; set; } = true;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
