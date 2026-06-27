using System.ComponentModel.DataAnnotations.Schema;
using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A single-use email-verification token. Only the SHA-256 hash is persisted;
/// the raw token is delivered to the user (via email) and never stored.
/// </summary>
public class EmailVerificationToken : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary>SHA-256 hash (hex) of the raw verification token.</summary>
    public required string TokenHash { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }

    /// <summary>True when the token is unused and not expired.</summary>
    [NotMapped]
    public bool IsActive => !IsUsed && DateTimeOffset.UtcNow < ExpiresAt;
}
