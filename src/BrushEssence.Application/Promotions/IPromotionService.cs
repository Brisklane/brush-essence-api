namespace BrushEssence.Application.Promotions;

/// <summary>Admin CRUD for discount promotions.</summary>
public interface IPromotionService
{
    Task<IReadOnlyList<PromotionDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PromotionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PromotionDto> CreateAsync(SavePromotionRequest request, CancellationToken cancellationToken = default);

    Task<PromotionDto> UpdateAsync(Guid id, SavePromotionRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
