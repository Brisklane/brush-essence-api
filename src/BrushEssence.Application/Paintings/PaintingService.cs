using BrushEssence.Application.Common.Exceptions;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Application.Common.Models;
using BrushEssence.Application.Promotions;

namespace BrushEssence.Application.Paintings;

public sealed class PaintingService(
    IPaintingRepository paintings,
    ICategoryRepository categories,
    IMediumRepository mediums,
    IPromotionRepository promotions,
    IFileStorageService fileStorage,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) : IPaintingService
{
    public async Task<PagedResult<PaintingDto>> GetPagedAsync(
        PaintingQuery query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await paintings.GetPagedAsync(query, cancellationToken);
        await ApplyDiscountsAsync(items, cancellationToken);

        return new PagedResult<PaintingDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<PaintingDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await paintings.GetDtoByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Painting not found.");

        await ApplyDiscountsAsync([dto], cancellationToken);
        return dto;
    }

    /// <summary>Sets each painting's discounted price from the live promotions.</summary>
    private async Task ApplyDiscountsAsync(IReadOnlyList<PaintingDto> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        var live = await promotions.GetLiveAsync(timeProvider.GetUtcNow(), cancellationToken);
        if (live.Count == 0)
        {
            return;
        }

        foreach (var item in items)
        {
            item.DiscountedPrice = PromotionCalculator.DiscountedPrice(
                item.Price, item.Id, item.CategoryId, live);
        }
    }

    public async Task<PaintingDto> CreateAsync(
        CreatePaintingRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);
        await EnsureMediumExistsAsync(request.MediumId, cancellationToken);

        var painting = request.ToEntity();
        await paintings.AddAsync(painting, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(painting.Id, cancellationToken);
    }

    public async Task<PaintingDto> UpdateAsync(
        Guid id,
        UpdatePaintingRequest request,
        CancellationToken cancellationToken = default)
    {
        var painting = await paintings.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Painting not found.");

        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);
        await EnsureMediumExistsAsync(request.MediumId, cancellationToken);

        var previousImageUrl = painting.ImageUrl;
        painting.ApplyUpdate(request);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // If the image was replaced, remove the now-orphaned file (best effort).
        if (!string.IsNullOrEmpty(previousImageUrl) && previousImageUrl != painting.ImageUrl)
        {
            await fileStorage.DeleteAsync(previousImageUrl, cancellationToken);
        }

        return await GetByIdAsync(painting.Id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var painting = await paintings.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Painting not found.");

        var imageUrl = painting.ImageUrl;
        paintings.Remove(painting);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await fileStorage.DeleteAsync(imageUrl, cancellationToken);
    }

    private async Task EnsureCategoryExistsAsync(Guid? categoryId, CancellationToken cancellationToken)
    {
        if (categoryId is { } id && !await categories.ExistsAsync(id, cancellationToken))
        {
            throw new BadRequestException("The selected category does not exist.");
        }
    }

    private async Task EnsureMediumExistsAsync(Guid? mediumId, CancellationToken cancellationToken)
    {
        if (mediumId is { } id && !await mediums.ExistsAsync(id, cancellationToken))
        {
            throw new BadRequestException("The selected medium does not exist.");
        }
    }
}
