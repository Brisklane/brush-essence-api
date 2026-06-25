using FluentValidation;

namespace BrushEssence.Application.Carts;

/// <summary>Shared cart business limits.</summary>
public static class CartLimits
{
    /// <summary>
    /// Absolute ceiling on units of a single painting in one cart, independent of
    /// stock. A sane guard against abuse / runaway input; real availability is
    /// still bounded by the painting's stock at add/update time.
    /// </summary>
    public const int MaxQuantityPerItem = 99;
}

public sealed class AddCartItemRequestValidator : AbstractValidator<AddCartItemRequest>
{
    public AddCartItemRequestValidator()
    {
        RuleFor(x => x.PaintingId).NotEmpty();
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(CartLimits.MaxQuantityPerItem);
    }
}

public sealed class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        // Quantity 0 is rejected here; removing a line is an explicit DELETE.
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(CartLimits.MaxQuantityPerItem);
    }
}
