using BrushEssence.Application.CustomRequests;
using FluentValidation.TestHelper;
using Xunit;

namespace BrushEssence.UnitTests.CustomRequests;

public class CreateCustomRequestRequestValidatorTests
{
    private readonly CreateCustomRequestRequestValidator _validator = new();

    private static CreateCustomRequestRequest Valid() => new()
    {
        Title = "Sunset commission",
        Description = "A warm sunset over the sea, ~60x90cm.",
    };

    [Fact]
    public void Valid_request_passes_validation()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Missing_title_and_description_fail_validation()
    {
        var request = new CreateCustomRequestRequest { Title = "", Description = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Quote_amount_must_be_positive(decimal amount)
    {
        var validator = new SetCustomRequestQuoteRequestValidator();
        var request = new SetCustomRequestQuoteRequest { Amount = amount };

        validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Too_many_images_fail_validation()
    {
        var request = Valid();
        request.Images = Enumerable
            .Range(0, CustomRequestLimits.MaxImages + 1)
            .Select(i => new CustomRequestImageInput { Url = $"/uploads/custom-requests/{i}.jpg" })
            .ToList();

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Images);
    }
}

public class UpdateCustomRequestStatusRequestValidatorTests
{
    private readonly UpdateCustomRequestStatusRequestValidator _validator = new();

    [Fact]
    public void Out_of_range_status_fails_validation()
    {
        var request = new UpdateCustomRequestStatusRequest
        {
            Status = (Domain.Entities.CustomRequestStatus)99,
        };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Status);
    }
}
