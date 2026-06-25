namespace BrushEssence.Application.Carts;

/// <summary>
/// Use-case operations for the current caller's cart. The caller is resolved
/// internally from the authenticated user or the guest cart token, so callers
/// never pass an owner. All mutating operations return the updated cart.
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Returns the current caller's cart, creating an empty one when needed.
    /// Prunes lines whose painting has disappeared or gone out of stock and
    /// clamps quantities that now exceed available stock.
    /// </summary>
    Task<CartDto> GetCartAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds units of a painting, or increases an existing line.</summary>
    Task<CartDto> AddItemAsync(AddCartItemRequest request, CancellationToken cancellationToken = default);

    /// <summary>Sets the absolute quantity of a painting already in the cart.</summary>
    Task<CartDto> UpdateItemAsync(Guid paintingId, UpdateCartItemRequest request, CancellationToken cancellationToken = default);

    /// <summary>Removes a painting's line from the cart entirely.</summary>
    Task<CartDto> RemoveItemAsync(Guid paintingId, CancellationToken cancellationToken = default);

    /// <summary>Empties the cart.</summary>
    Task<CartDto> ClearAsync(CancellationToken cancellationToken = default);
}
