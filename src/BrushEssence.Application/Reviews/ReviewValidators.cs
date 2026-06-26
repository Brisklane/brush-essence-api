using FluentValidation;

namespace BrushEssence.Application.Reviews;

/// <summary>Shared review content limits.</summary>
public static class ReviewLimits
{
    public const int MinRating = 1;
    public const int MaxRating = 5;
    public const int MaxTitleLength = 150;
    public const int MaxCommentLength = 2000;
}

public sealed class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(ReviewLimits.MinRating, ReviewLimits.MaxRating)
            .WithMessage($"Rating must be between {ReviewLimits.MinRating} and {ReviewLimits.MaxRating}.");
        RuleFor(x => x.Title).MaximumLength(ReviewLimits.MaxTitleLength);
        RuleFor(x => x.Comment).MaximumLength(ReviewLimits.MaxCommentLength);
    }
}

public sealed class AdminUpdateReviewRequestValidator : AbstractValidator<AdminUpdateReviewRequest>
{
    public AdminUpdateReviewRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(ReviewLimits.MinRating, ReviewLimits.MaxRating)
            .WithMessage($"Rating must be between {ReviewLimits.MinRating} and {ReviewLimits.MaxRating}.");
        RuleFor(x => x.Title).MaximumLength(ReviewLimits.MaxTitleLength);
        RuleFor(x => x.Comment).MaximumLength(ReviewLimits.MaxCommentLength);
    }
}

public sealed class UpdateReviewStatusRequestValidator : AbstractValidator<UpdateReviewStatusRequest>
{
    public UpdateReviewStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
