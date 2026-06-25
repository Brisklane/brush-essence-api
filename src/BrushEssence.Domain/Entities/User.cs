using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// An application user who can authenticate. Passwords are never stored in
/// plaintext — only the BCrypt hash in <see cref="PasswordHash"/>.
/// </summary>
public class User : AuditableEntity
{
    /// <summary>Login identifier; unique and stored normalized (lower-cased).</summary>
    public required string Email { get; set; }

    /// <summary>BCrypt hash of the user's password.</summary>
    public required string PasswordHash { get; set; }

    public string? FullName { get; set; }

    /// <summary>Whether the account may sign in.</summary>
    public bool IsActive { get; set; } = true;

    public bool IsEmailVerified { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
