using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    /// <summary>Finds a refresh token by its hash, including the owning user and roles.</summary>
    Task<RefreshToken?> GetByHashWithUserRolesAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all of a user's still-active refresh tokens as revoked (e.g. after a
    /// password reset). Changes are persisted by the caller via the unit of work.
    /// </summary>
    Task RevokeAllActiveForUserAsync(
        Guid userId,
        DateTimeOffset revokedAt,
        CancellationToken cancellationToken = default);
}
