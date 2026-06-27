namespace BrushEssence.Application.Auth;

/// <summary>Public profile projection of a user, returned to API clients.</summary>
public sealed class UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? FullName { get; init; }
    public bool IsEmailVerified { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Result of a successful authentication. The raw refresh token is included for
/// the caller (the Next.js BFF) to store in an httpOnly cookie; it is never
/// persisted in plaintext server-side.
/// </summary>
public sealed class AuthResult
{
    public required string AccessToken { get; init; }
    public required DateTimeOffset AccessTokenExpiresAt { get; init; }
    public required string RefreshToken { get; init; }
    public required UserDto User { get; init; }
}

public sealed class RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? FullName { get; init; }
}

public sealed class LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed class RefreshRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}

public sealed class LogoutRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}

public sealed class ForgotPasswordRequest
{
    public string Email { get; init; } = string.Empty;
}

public sealed class ResetPasswordRequest
{
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public sealed class VerifyEmailRequest
{
    public string Token { get; init; } = string.Empty;
}
