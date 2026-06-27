using BrushEssence.Application.Common.Exceptions;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Carts;

/// <summary>
/// Cart use cases. The owner of the cart is resolved from the authenticated user
/// (<see cref="ICurrentUser"/>) or, for guests, the opaque token on
/// <see cref="ICartSession"/>. When a guest with items signs in, their cart is
/// merged into the user's cart on the next access, so nothing is lost at login.
/// </summary>
public sealed class CartService(
    ICartRepository carts,
    IPaintingRepository paintings,
    IPromotionRepository promotions,
    ICurrentUser currentUser,
    ICartSession cartSession,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) : ICartService
{
    /// <summary>Maps a cart to its DTO with current promotional pricing applied.</summary>
    private async Task<CartDto> ToDtoAsync(Cart cart, CancellationToken cancellationToken)
    {
        var live = await promotions.GetLiveAsync(timeProvider.GetUtcNow(), cancellationToken);
        return cart.ToDto(live);
    }

    public async Task<CartDto> GetCartAsync(CancellationToken cancellationToken = default)
    {
        var cart = await ResolveCartAsync(createIfMissing: false, cancellationToken);
        if (cart is null)
        {
            // No identity and no stored cart yet: hand back an empty, unsaved cart.
            return await ToDtoAsync(new Cart(), cancellationToken);
        }

        if (PruneUnavailableItems(cart))
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await ToDtoAsync(cart, cancellationToken);
    }

    public async Task<CartDto> AddItemAsync(
        AddCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var painting = await GetPurchasablePaintingAsync(request.PaintingId, cancellationToken);

        var cart = await ResolveCartAsync(createIfMissing: true, cancellationToken);
        var existing = cart!.FindItem(request.PaintingId);
        var desiredQuantity = (existing?.Quantity ?? 0) + request.Quantity;

        EnsureWithinStock(desiredQuantity, painting);

        if (existing is not null)
        {
            existing.Quantity = desiredQuantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                PaintingId = painting.Id,
                Quantity = request.Quantity,
            });
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(cart, cancellationToken);
    }

    public async Task<CartDto> UpdateItemAsync(
        Guid paintingId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var cart = await ResolveCartAsync(createIfMissing: false, cancellationToken)
            ?? throw new NotFoundException("Cart item not found.");

        var item = cart.FindItem(paintingId)
            ?? throw new NotFoundException("Cart item not found.");

        var painting = await GetPurchasablePaintingAsync(paintingId, cancellationToken);
        EnsureWithinStock(request.Quantity, painting);

        item.Quantity = request.Quantity;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(cart, cancellationToken);
    }

    public async Task<CartDto> RemoveItemAsync(
        Guid paintingId,
        CancellationToken cancellationToken = default)
    {
        var cart = await ResolveCartAsync(createIfMissing: false, cancellationToken);
        var item = cart?.FindItem(paintingId);
        if (cart is not null && item is not null)
        {
            cart.Items.Remove(item);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await ToDtoAsync(cart ?? new Cart(), cancellationToken);
    }

    public async Task<CartDto> ClearAsync(CancellationToken cancellationToken = default)
    {
        var cart = await ResolveCartAsync(createIfMissing: false, cancellationToken);
        if (cart is { Items.Count: > 0 })
        {
            cart.Items.Clear();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await ToDtoAsync(cart ?? new Cart(), cancellationToken);
    }

    // ----- Ownership resolution -----

    /// <summary>
    /// Finds (or optionally creates) the caller's cart. For authenticated callers
    /// this also folds in any guest cart identified by the request token.
    /// </summary>
    private async Task<Cart?> ResolveCartAsync(bool createIfMissing, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is { } userId)
        {
            return await ResolveUserCartAsync(userId, createIfMissing, cancellationToken);
        }

        if (cartSession.AnonymousCartId is { } anonymousId)
        {
            var guestCart = await carts.GetByAnonymousIdAsync(anonymousId, cancellationToken);
            if (guestCart is null && createIfMissing)
            {
                guestCart = new Cart { AnonymousId = anonymousId };
                await carts.AddAsync(guestCart, cancellationToken);
            }

            return guestCart;
        }

        // A guest who hasn't presented a token cannot have a persisted cart.
        if (createIfMissing)
        {
            throw new BadRequestException(
                "A cart session token is required. Send one on the 'X-Cart-Token' header.");
        }

        return null;
    }

    private async Task<Cart> ResolveUserCartAsync(
        Guid userId,
        bool createIfMissing,
        CancellationToken cancellationToken)
    {
        var userCart = await carts.GetByUserIdAsync(userId, cancellationToken);

        // Fold a guest cart (built before signing in) into the user's cart once.
        if (cartSession.AnonymousCartId is { } anonymousId)
        {
            var guestCart = await carts.GetByAnonymousIdAsync(anonymousId, cancellationToken);
            if (guestCart is not null)
            {
                userCart ??= await CreateUserCartAsync(userId, cancellationToken);
                MergeInto(userCart, guestCart);
                carts.Remove(guestCart);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        if (userCart is null && createIfMissing)
        {
            userCart = await CreateUserCartAsync(userId, cancellationToken);
        }

        return userCart!;
    }

    private async Task<Cart> CreateUserCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = new Cart { UserId = userId };
        await carts.AddAsync(cart, cancellationToken);
        return cart;
    }

    /// <summary>
    /// Merges the guest cart's lines into the target, summing quantities for
    /// paintings present in both and clamping every line to current stock.
    /// </summary>
    private static void MergeInto(Cart target, Cart source)
    {
        foreach (var sourceItem in source.Items)
        {
            var stock = sourceItem.Painting?.StockQuantity ?? 0;
            if (stock <= 0)
            {
                continue;
            }

            var existing = target.FindItem(sourceItem.PaintingId);
            if (existing is not null)
            {
                existing.Quantity = Math.Min(existing.Quantity + sourceItem.Quantity, stock);
            }
            else
            {
                target.Items.Add(new CartItem
                {
                    PaintingId = sourceItem.PaintingId,
                    Quantity = Math.Min(sourceItem.Quantity, stock),
                });
            }
        }
    }

    // ----- Validation helpers -----

    private async Task<Painting> GetPurchasablePaintingAsync(Guid paintingId, CancellationToken cancellationToken)
    {
        var painting = await paintings.GetByIdAsync(paintingId, cancellationToken);

        // Unpublished paintings are invisible to the storefront, so adding one is
        // treated as "not found" rather than revealing that it exists.
        if (painting is null || !painting.IsPublished)
        {
            throw new NotFoundException("Painting not found.");
        }

        if (painting.StockQuantity <= 0)
        {
            throw new BadRequestException("This painting is sold out.");
        }

        return painting;
    }

    private static void EnsureWithinStock(int desiredQuantity, Painting painting)
    {
        if (desiredQuantity > painting.StockQuantity)
        {
            throw new BadRequestException(
                $"Only {painting.StockQuantity} of \"{painting.Title}\" {(painting.StockQuantity == 1 ? "is" : "are")} available.");
        }
    }

    /// <summary>
    /// Drops lines whose painting vanished or sold out and clamps any line that
    /// now exceeds available stock. Returns true when the cart was modified.
    /// </summary>
    private static bool PruneUnavailableItems(Cart cart)
    {
        var changed = false;

        foreach (var item in cart.Items.ToList())
        {
            var painting = item.Painting;
            if (painting is null || painting.StockQuantity <= 0)
            {
                cart.Items.Remove(item);
                changed = true;
            }
            else if (item.Quantity > painting.StockQuantity)
            {
                item.Quantity = painting.StockQuantity;
                changed = true;
            }
        }

        return changed;
    }
}
