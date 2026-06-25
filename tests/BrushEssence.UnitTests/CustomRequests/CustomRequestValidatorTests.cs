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
        Currency = "USD",
    };

    [Fact]
    public void Valid_request_passes_validation()
        => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Missing_title_and_description_fail_validation()
    {
        var request = new CreateCustomRequestRequest { Title = "", Description = "", Currency = "USD" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Negative_budget_fails_validation()
    {
        var request = Valid();
        request.BudgetAmount = -10m;

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.BudgetAmount);
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
