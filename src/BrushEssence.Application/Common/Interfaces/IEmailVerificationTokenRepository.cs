using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Common.Interfaces;

public interface IEmailVerificationTokenRepository
{
    /// <summary>Finds a verification token by its hash, including the owning user.</summary>
    Task<EmailVerificationToken?> GetByHashWithUserAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);

    /// <summary>Invalidates any outstanding tokens for a user (e.g. before issuing a new one).</summary>
    Task InvalidateActiveForUserAsync(
        Guid userId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}
