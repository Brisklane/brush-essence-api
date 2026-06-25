using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Common.Interfaces;

public interface IPasswordResetTokenRepository
{
    /// <summary>Finds a reset token by its hash, including the owning user.</summary>
    Task<PasswordResetToken?> GetByHashWithUserAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
}
