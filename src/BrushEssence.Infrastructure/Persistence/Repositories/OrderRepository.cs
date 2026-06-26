using BrushEssence.Application.Admin;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Application.Orders;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(ApplicationDbContext context) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => await context.Orders.AddAsync(order, cancellationToken);

    public async Task<(IReadOnlyList<AdminOrderListItemDto> Items, int TotalCount)> GetPagedForAdminAsync(
        AdminOrderQuery query,
        CancellationToken cancellationToken = default)
    {
        var queryable = context.Orders.AsNoTracking();

        if (query.StatusFilter is { } status)
        {
            queryable = queryable.Where(o => o.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            queryable = queryable.Where(o =>
                EF.Functions.ILike(o.OrderNumber, term) ||
                EF.Functions.ILike(o.CustomerEmail, term));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .OrderByDescending(o => o.CreatedAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(o => new AdminOrderListItemDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerEmail = o.CustomerEmail,
                Status = o.Status,
                Currency = o.Currency,
                Total = o.Total,
                ItemCount = o.Items.Sum(i => i.Quantity),
                CreatedAt = o.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Painting)
            .Include(o => o.StatusHistory)
            // Two collection includes — split the query to avoid a cartesian blow-up.
            .AsSplitQuery()
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
