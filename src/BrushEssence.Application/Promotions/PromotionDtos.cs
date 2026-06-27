using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Promotions;

public sealed class PromotionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal Value { get; set; }
    public PromotionScope Scope { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public IReadOnlyList<Guid> PaintingIds { get; set; } = [];
    public bool IsActive { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    /// <summary>True when currently active and within its date window.</summary>
    public bool IsLive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class SavePromotionRequest
{
    public string Name { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal Value { get; set; }
    public PromotionScope Scope { get; set; }
    public Guid? CategoryId { get; set; }
    public List<Guid> PaintingIds { get; set; } = [];
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
}
