using BrushEssence.Application.Reviews;
using FluentValidation.TestHelper;
using Xunit;

namespace BrushEssence.UnitTests.Reviews;

public class CreateReviewRequestValidatorTests
{
    private readonly CreateReviewRequestValidator _validator = new();

    [Fact]
    public void Valid_review_passes_validation()
    {
        var request = new CreateReviewRequest
        {
            Rating = 5,
            Title = "Stunning",
            Comment = "Even better in person.",
        };

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Out_of_range_rating_fails_validation(int rating)
    {
        var request = new CreateReviewRequest { Rating = rating };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Overlong_comment_fails_validation()
    {
        var request = new CreateReviewRequest
        {
            Rating = 4,
            Comment = new string('a', ReviewLimits.MaxCommentLength + 1),
        };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Comment);
    }

    [Fact]
    public void Rating_only_review_is_valid()
    {
        var request = new CreateReviewRequest { Rating = 3 };

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }
}
