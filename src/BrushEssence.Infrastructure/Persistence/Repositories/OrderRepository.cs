using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Application.Orders;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(ApplicationDbContext context) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => await context.Orders.AddAsync(order, cancellationToken);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Painting)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<OrderSummaryDto>> GetSummariesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status,
                Currency = o.Currency,
                Total = o.Total,
                ItemCount = o.Items.Sum(i => i.Quantity),
                CreatedAt = o.CreatedAt,
            })
            .ToListAsync(cancellationToken);

    public Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default)
        => context.Orders.AnyAsync(o => o.OrderNumber == orderNumber, cancellationToken);
}
