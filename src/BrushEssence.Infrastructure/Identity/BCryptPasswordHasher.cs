using BrushEssence.Application.Common.Interfaces;

namespace BrushEssence.Infrastructure.Identity;

/// <summary>BCrypt-based password hasher. Work factor 12 (~250ms/hash).</summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Stored hash is malformed; treat as a failed verification.
            return false;
        }
    }
}
