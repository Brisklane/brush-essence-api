namespace BrushEssence.Application.Common.Interfaces;

/// <summary>
/// Builds absolute, user-facing links into the web app (e.g. for email bodies).
/// Implemented in Infrastructure where the public base URL is configured.
/// </summary>
public interface IAppLinks
{
    /// <summary>Link the user clicks to verify their email address.</summary>
    string VerifyEmailUrl(string token);

    /// <summary>Link the user clicks to choose a new password.</summary>
    string ResetPasswordUrl(string token);
}
