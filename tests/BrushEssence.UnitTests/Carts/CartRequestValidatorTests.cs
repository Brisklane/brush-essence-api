using BrushEssence.Application.Carts;
using FluentValidation.TestHelper;
using Xunit;

namespace BrushEssence.UnitTests.Carts;

public class AddCartItemRequestValidatorTests
{
    private readonly AddCartItemRequestValidator _validator = new();

    [Fact]
    public void Valid_request_passes_validation()
    {
        var request = new AddCartItemRequest
        {
            PaintingId = Guid.NewGuid(),
            Quantity = 2,
        };

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_painting_id_fails_validation()
    {
        var request = new AddCartItemRequest
        {
            PaintingId = Guid.Empty,
            Quantity = 1,
        };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.PaintingId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(CartLimits.MaxQuantityPerItem + 1)]
    public void Out_of_range_quantity_fails_validation(int quantity)
    {
        var request = new AddCartItemRequest
        {
            PaintingId = Guid.NewGuid(),
            Quantity = quantity,
        };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Quantity);
    }
}

public class UpdateCartItemRequestValidatorTests
{
    private readonly UpdateCartItemRequestValidator _validator = new();

    [Fact]
    public void Positive_quantity_within_limit_passes_validation()
    {
        _validator.TestValidate(new UpdateCartItemRequest { Quantity = 3 })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(CartLimits.MaxQuantityPerItem + 1)]
    public void Out_of_range_quantity_fails_validation(int quantity)
    {
        _validator.TestValidate(new UpdateCartItemRequest { Quantity = quantity })
            .ShouldHaveValidationErrorFor(x => x.Quantity);
    }
}
