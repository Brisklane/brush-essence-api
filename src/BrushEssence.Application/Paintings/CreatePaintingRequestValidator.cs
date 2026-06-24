using FluentValidation;

namespace BrushEssence.Application.Paintings;

/// <summary>
/// Validation rules for <see cref="CreatePaintingRequest"/>. Discovered and
/// registered automatically by <c>AddApplication()</c>.
/// </summary>
public class CreatePaintingRequestValidator : AbstractValidator<CreatePaintingRequest>
{
    public CreatePaintingRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(4000);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO 4217 code.");

        RuleFor(x => x.WidthCm)
            .GreaterThan(0);

        RuleFor(x => x.HeightCm)
            .GreaterThan(0);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048);
    }
}
