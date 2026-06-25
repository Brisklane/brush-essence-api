using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class CartRepository(ApplicationDbContext context) : ICartRepository
{
    public Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => LoadCart(cart => cart.UserId == userId, cancellationToken);

    public Task<Cart?> GetByAnonymousIdAsync(Guid anonymousId, CancellationToken cancellationToken = default)
        => LoadCart(cart => cart.AnonymousId == anonymousId, cancellationToken);

    public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
        => await context.Carts.AddAsync(cart, cancellationToken);

    public void Remove(Cart cart) => context.Carts.Remove(cart);

    // Tracked load of the whole aggregate (items + each item's painting) so the
    // service can mutate quantities and read live prices/stock in one round trip.
    private Task<Cart?> LoadCart(
        System.Linq.Expressions.Expression<Func<Cart, bool>> predicate,
        CancellationToken cancellationToken)
        => context.Carts
            .Include(cart => cart.Items)
                .ThenInclude(item => item.Painting)
            .FirstOrDefaultAsync(predicate, cancellationToken);
}
