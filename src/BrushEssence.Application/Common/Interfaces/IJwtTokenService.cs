using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Common.Interfaces;

/// <summary>
/// Issues JWT access tokens and opaque refresh/reset tokens, and hashes opaque
/// tokens for storage. All token lifetimes are owned by the implementation
/// (driven by configuration), so callers never compute expiry themselves.
/// </summary>
public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(
        User user,
        IEnumerable<string> roles);

    (string Token, DateTimeOffset ExpiresAt) GenerateRefreshToken();

    /// <summary>Cryptographically-random opaque token (e.g. for password resets).</summary>
    string GenerateSecureToken();

    /// <summary>SHA-256 hex hash of an opaque token, for at-rest storage/lookup.</summary>
    string HashToken(string rawToken);
}
