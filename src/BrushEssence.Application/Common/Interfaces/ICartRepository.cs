using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Common.Interfaces;

public interface ICartRepository
{
    /// <summary>
    /// Loads a signed-in user's cart with its items and each item's painting
    /// (tracked, for mutation). Returns null when the user has no cart yet.
    /// </summary>
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a guest cart by its opaque token, with items and paintings (tracked).
    /// Returns null when no such cart exists.
    /// </summary>
    Task<Cart?> GetByAnonymousIdAsync(Guid anonymousId, CancellationToken cancellationToken = default);

    Task AddAsync(Cart cart, CancellationToken cancellationToken = default);

    void Remove(Cart cart);
}
