using BrushEssence.Application.Admin;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class ReportingRepository(ApplicationDbContext context) : IReportingRepository
{
    public async Task<OrderAggregates> GetOrderAggregatesAsync(CancellationToken cancellationToken = default)
    {
        var total = await context.Orders.CountAsync(cancellationToken);

        var revenue = await context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SumAsync(o => (decimal?)o.Total, cancellationToken) ?? 0m;

        var byStatus = await context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        var currency = await context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => o.Currency)
            .FirstOrDefaultAsync(cancellationToken) ?? "USD";

        return new OrderAggregates(total, revenue, currency, byStatus);
    }

    public async Task<UserAggregates> GetUserAggregatesAsync(
        DateTimeOffset newSince,
        CancellationToken cancellationToken = default)
    {
        var total = await context.Users.CountAsync(cancellationToken);
        var active = await context.Users.CountAsync(u => u.IsActive, cancellationToken);
        var newSinceCount = await context.Users.CountAsync(u => u.CreatedAt >= newSince, cancellationToken);

        return new UserAggregates(total, active, newSinceCount);
    }

    public async Task<RequestAggregates> GetRequestAggregatesAsync(CancellationToken cancellationToken = default)
    {
        var total = await context.CustomRequests.CountAsync(cancellationToken);

        var byStatus = await context.CustomRequests
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        return new RequestAggregates(total, byStatus);
    }

    public async Task<CatalogAggregates> GetCatalogAggregatesAsync(CancellationToken cancellationToken = default)
    {
        var total = await context.Paintings.CountAsync(cancellationToken);
        var published = await context.Paintings.CountAsync(p => p.IsPublished, cancellationToken);
        var outOfStock = await context.Paintings.CountAsync(p => p.StockQuantity <= 0, cancellationToken);

        return new CatalogAggregates(total, published, outOfStock);
    }

    public async Task<IReadOnlyList<AdminOrderListItemDto>> GetRecentOrdersAsync(
        int take,
        CancellationToken cancellationToken = default)
        => await context.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Take(take)
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

    public async Task<IReadOnlyList<AdminCustomRequestListItemDto>> GetRecentRequestsAsync(
        int take,
        CancellationToken cancellationToken = default)
        => await context.CustomRequests
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .Select(r => new AdminCustomRequestListItemDto
            {
                Id = r.Id,
                Title = r.Title,
                CustomerEmail = r.CustomerEmail,
                Status = r.Status,
                ImageCount = r.Images.Count,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<OrderActivityRow>> GetOrdersSinceAsync(
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
        => await context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= since)
            .Select(o => new OrderActivityRow(o.CreatedAt, o.Total, o.Currency, o.Status))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DateTimeOffset>> GetUserSignupsSinceAsync(
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
        => await context.Users
            .AsNoTracking()
            .Where(u => u.CreatedAt >= since)
            .Select(u => u.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DateTimeOffset>> GetRequestsSinceAsync(
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
        => await context.CustomRequests
            .AsNoTracking()
            .Where(r => r.CreatedAt >= since)
            .Select(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
}
