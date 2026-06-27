namespace BrushEssence.Application.Auth;

/// <summary>Plain, dependency-free HTML bodies for authentication emails.</summary>
public static class AuthEmailTemplates
{
    public const string VerifyEmailSubject = "Verify your email · Brush Essence";
    public const string PasswordResetSubject = "Reset your password · Brush Essence";

    public static string VerifyEmail(string? name, string verifyUrl) => Wrap(
        "Welcome to Brush Essence",
        $"""
        <p>Hi{Greeting(name)},</p>
        <p>Thanks for creating an account. Please confirm your email address to finish setting up.</p>
        {Button("Verify email", verifyUrl)}
        <p style="color:#6b7280;font-size:13px">This link expires in 24 hours. If you didn't create an account, you can safely ignore this email.</p>
        """);

    public static string PasswordReset(string resetUrl) => Wrap(
        "Reset your password",
        $"""
        <p>We received a request to reset your Brush Essence password.</p>
        {Button("Choose a new password", resetUrl)}
        <p style="color:#6b7280;font-size:13px">This link expires in 1 hour. If you didn't request this, you can safely ignore this email and your password will stay the same.</p>
        """);

    private static string Greeting(string? name)
        => string.IsNullOrWhiteSpace(name) ? string.Empty : $" {System.Net.WebUtility.HtmlEncode(name)}";

    private static string Button(string label, string url) =>
        $"""<p style="margin:24px 0"><a href="{url}" style="background:#1e3a5f;color:#fff;padding:12px 22px;border-radius:8px;text-decoration:none;display:inline-block;font-weight:600">{label}</a></p><p style="color:#6b7280;font-size:13px">Or paste this link into your browser:<br><a href="{url}" style="color:#1e3a5f">{url}</a></p>""";

    private static string Wrap(string heading, string inner) =>
        $"""
        <div style="font-family:Arial,Helvetica,sans-serif;max-width:520px;margin:0 auto;padding:24px;color:#111827">
          <h1 style="font-size:20px;color:#1e3a5f;margin:0 0 16px">{heading}</h1>
          {inner}
          <hr style="border:none;border-top:1px solid #e5e7eb;margin:24px 0">
          <p style="color:#9ca3af;font-size:12px">Brush Essence — original oil paintings</p>
        </div>
        """;
}
