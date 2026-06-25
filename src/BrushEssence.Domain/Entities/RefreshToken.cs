using System.ComponentModel.DataAnnotations.Schema;
using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A persisted refresh token. Only the SHA-256 hash of the token is stored, so a
/// database leak does not expose usable tokens. Tokens are rotated on every use:
/// the old one is revoked and linked to its replacement.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary>SHA-256 hash (hex) of the raw refresh token.</summary>
    public required string TokenHash { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>Hash of the token that replaced this one (set on rotation).</summary>
    public string? ReplacedByTokenHash { get; set; }

    public string? CreatedByIp { get; set; }

    // Navigation properties
    public User? User { get; set; }

    /// <summary>True when the token is neither revoked nor expired.</summary>
    [NotMapped]
    public bool IsActive => !IsRevoked && DateTimeOffset.UtcNow < ExpiresAt;
}
