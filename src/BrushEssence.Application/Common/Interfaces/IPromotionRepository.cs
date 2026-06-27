using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Common.Interfaces;

public interface IPromotionRepository
{
    /// <summary>All promotions (with category + targeted paintings), newest first.</summary>
    Task<IReadOnlyList<Promotion>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>A single promotion with its category and targeted paintings (tracked).</summary>
    Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Promotions that are currently live (active and within their date window),
    /// including their targeted paintings — used to price the catalogue/cart.
    /// </summary>
    Task<IReadOnlyList<Promotion>> GetLiveAsync(DateTimeOffset now, CancellationToken cancellationToken = default);

    Task AddAsync(Promotion promotion, CancellationToken cancellationToken = default);

    void Remove(Promotion promotion);
}
