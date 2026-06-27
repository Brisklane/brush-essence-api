namespace BrushEssence.Application.Common.Interfaces;

/// <summary>
/// Sends transactional emails. The concrete implementation (real SMTP vs. a
/// logging no-op for local dev) is chosen in the composition root.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
