using BrushEssence.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BrushEssence.Infrastructure.Email;

/// <summary>
/// Fallback email sender used when no SMTP host is configured. Logs the message
/// (including any links) so flows are fully testable locally without a provider.
/// </summary>
public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "EMAIL NOT SENT (no SMTP configured). To: {Recipient} | Subject: {Subject}\n{Body}",
            toEmail,
            subject,
            htmlBody);
        return Task.CompletedTask;
    }
}
