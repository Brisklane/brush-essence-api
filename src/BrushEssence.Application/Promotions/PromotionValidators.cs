using BrushEssence.Domain.Entities;
using FluentValidation;

namespace BrushEssence.Application.Promotions;

public sealed class SavePromotionRequestValidator : AbstractValidator<SavePromotionRequest>
{
    public SavePromotionRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DiscountType).IsInEnum();
        RuleFor(x => x.Scope).IsInEnum();

        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.Value)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == DiscountType.Percentage)
            .WithMessage("A percentage discount must be 100 or less.");

        RuleFor(x => x.CategoryId)
            .NotNull()
            .When(x => x.Scope == PromotionScope.Category)
            .WithMessage("Choose a category for a category-wide promotion.");

        RuleFor(x => x.PaintingIds)
            .Must(ids => ids.Count > 0)
            .When(x => x.Scope == PromotionScope.Paintings)
            .WithMessage("Select at least one painting.");

        RuleFor(x => x.EndsAt)
            .GreaterThan(x => x.StartsAt!.Value)
            .When(x => x.StartsAt.HasValue && x.EndsAt.HasValue)
            .WithMessage("The end date must be after the start date.");
    }
}
