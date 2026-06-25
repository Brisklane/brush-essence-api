namespace BrushEssence.Application.Common.Interfaces;

/// <summary>Hashes and verifies user passwords. Implemented with BCrypt.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}
