using BrushEssence.Application.Paintings;
using FluentValidation.TestHelper;
using Xunit;

namespace BrushEssence.UnitTests.Paintings;

public class CreatePaintingRequestValidatorTests
{
    private readonly CreatePaintingRequestValidator _validator = new();

    [Fact]
    public void Valid_request_passes_validation()
    {
        var request = new CreatePaintingRequest
        {
            Title = "Sunset over the harbour",
            Price = 250m,
            Currency = "USD",
            WidthCm = 40,
            HeightCm = 60,
            StockQuantity = 1,
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Missing_title_fails_validation()
    {
        var request = new CreatePaintingRequest
        {
            Title = string.Empty,
            Price = 250m,
            Currency = "USD",
            WidthCm = 40,
            HeightCm = 60,
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Non_positive_price_fails_validation()
    {
        var request = new CreatePaintingRequest
        {
            Title = "Untitled",
            Price = 0m,
            Currency = "USD",
            WidthCm = 40,
            HeightCm = 60,
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Price);
    }
}
