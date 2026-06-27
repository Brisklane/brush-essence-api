using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class PromotionRepository(ApplicationDbContext context) : IPromotionRepository
{
    public async Task<IReadOnlyList<Promotion>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Promotions
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.PromotionPaintings)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Promotions
            .Include(p => p.Category)
            .Include(p => p.PromotionPaintings)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Promotion>> GetLiveAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
        => await context.Promotions
            .AsNoTracking()
            .Include(p => p.PromotionPaintings)
            .Where(p => p.IsActive
                && (p.StartsAt == null || p.StartsAt <= now)
                && (p.EndsAt == null || p.EndsAt >= now))
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Promotion promotion, CancellationToken cancellationToken = default)
        => await context.Promotions.AddAsync(promotion, cancellationToken);

    public void Remove(Promotion promotion) => context.Promotions.Remove(promotion);
}
