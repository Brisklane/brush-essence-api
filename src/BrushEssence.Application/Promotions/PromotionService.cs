using BrushEssence.Application.Common.Exceptions;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Promotions;

public sealed class PromotionService(
    IPromotionRepository promotions,
    ICategoryRepository categories,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) : IPromotionService
{
    public async Task<IReadOnlyList<PromotionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        return (await promotions.GetAllAsync(cancellationToken))
            .Select(p => p.ToDto(now))
            .ToList();
    }

    public async Task<IReadOnlyList<ActivePromotionDto>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var live = await promotions.GetLiveAsync(now, cancellationToken);

        return live
            // Soonest-ending first so the banner can feature the most urgent one;
            // promotions with no end date sort last.
            .OrderBy(p => p.EndsAt ?? DateTimeOffset.MaxValue)
            .ThenByDescending(p => p.CreatedAt)
            .Select(p => new ActivePromotionDto
            {
                Name = p.Name,
                DiscountType = p.DiscountType,
                Value = p.Value,
                Scope = p.Scope,
                EndsAt = p.EndsAt,
            })
            .ToList();
    }

    public async Task<PromotionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var promotion = await promotions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Promotion not found.");

        return promotion.ToDto(timeProvider.GetUtcNow());
    }

    public async Task<PromotionDto> CreateAsync(
        SavePromotionRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCategoryAsync(request, cancellationToken);

        var promotion = new Promotion { Name = request.Name.Trim() };
        Apply(promotion, request);

        await promotions.AddAsync(promotion, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return promotion.ToDto(timeProvider.GetUtcNow());
    }

    public async Task<PromotionDto> UpdateAsync(
        Guid id,
        SavePromotionRequest request,
        CancellationToken cancellationToken = default)
    {
        var promotion = await promotions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Promotion not found.");

        await EnsureCategoryAsync(request, cancellationToken);

        promotion.Name = request.Name.Trim();
        Apply(promotion, request);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return promotion.ToDto(timeProvider.GetUtcNow());
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var promotion = await promotions.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Promotion not found.");

        promotions.Remove(promotion);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Copies the request onto the entity, normalising scope-specific fields.</summary>
    private static void Apply(Promotion promotion, SavePromotionRequest request)
    {
        promotion.DiscountType = request.DiscountType;
        promotion.Value = request.Value;
        promotion.Scope = request.Scope;
        promotion.IsActive = request.IsActive;
        promotion.StartsAt = request.StartsAt;
        promotion.EndsAt = request.EndsAt;

        // Only keep the targeting relevant to the chosen scope.
        promotion.CategoryId = request.Scope == PromotionScope.Category ? request.CategoryId : null;

        promotion.PromotionPaintings.Clear();
        if (request.Scope == PromotionScope.Paintings)
        {
            foreach (var paintingId in request.PaintingIds.Distinct())
            {
                promotion.PromotionPaintings.Add(new PromotionPainting { PaintingId = paintingId });
            }
        }
    }

    private async Task EnsureCategoryAsync(SavePromotionRequest request, CancellationToken cancellationToken)
    {
        if (request.Scope == PromotionScope.Category
            && request.CategoryId is { } id
            && !await categories.ExistsAsync(id, cancellationToken))
        {
            throw new BadRequestException("The selected category does not exist.");
        }
    }
}
