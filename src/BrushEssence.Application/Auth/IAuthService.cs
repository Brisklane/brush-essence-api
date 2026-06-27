namespace BrushEssence.Application.Auth;

/// <summary>
/// Authentication use cases: registration, login, token refresh/rotation,
/// logout, password reset, and profile lookup.
/// </summary>
public interface IAuthService
{
    Task<AuthResult> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<AuthResult> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<AuthResult> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Confirms a user's email from a verification token.</summary>
    Task VerifyEmailAsync(
        VerifyEmailRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Re-sends the verification email to an unverified user (idempotent).</summary>
    Task ResendVerificationAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}
