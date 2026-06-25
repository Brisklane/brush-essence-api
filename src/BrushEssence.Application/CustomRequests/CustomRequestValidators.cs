using FluentValidation;

namespace BrushEssence.Application.CustomRequests;

/// <summary>Shared limits for custom requests.</summary>
public static class CustomRequestLimits
{
    public const int MaxImages = 8;
    public const int MaxDescriptionLength = 4000;
}

public sealed class CustomRequestImageInputValidator : AbstractValidator<CustomRequestImageInput>
{
    public CustomRequestImageInputValidator()
    {
        RuleFor(x => x.Url).NotEmpty().MaximumLength(2048);
        RuleFor(x => x.FileName).MaximumLength(260);
    }
}

public sealed class CreateCustomRequestRequestValidator : AbstractValidator<CreateCustomRequestRequest>
{
    public CreateCustomRequestRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(CustomRequestLimits.MaxDescriptionLength);
        RuleFor(x => x.PreferredSize).MaximumLength(200);
        RuleFor(x => x.BudgetAmount).GreaterThan(0).When(x => x.BudgetAmount.HasValue);
        RuleFor(x => x.Currency).NotEmpty().Length(3)
            .WithMessage("Currency must be a 3-letter ISO 4217 code.");
        RuleFor(x => x.Images).Must(images => images.Count <= CustomRequestLimits.MaxImages)
            .WithMessage($"At most {CustomRequestLimits.MaxImages} reference images are allowed.");
        RuleForEach(x => x.Images).SetValidator(new CustomRequestImageInputValidator());
    }
}

public sealed class UpdateCustomRequestRequestValidator : AbstractValidator<UpdateCustomRequestRequest>
{
    public UpdateCustomRequestRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(CustomRequestLimits.MaxDescriptionLength);
        RuleFor(x => x.PreferredSize).MaximumLength(200);
        RuleFor(x => x.BudgetAmount).GreaterThan(0).When(x => x.BudgetAmount.HasValue);
        RuleFor(x => x.Currency).NotEmpty().Length(3)
            .WithMessage("Currency must be a 3-letter ISO 4217 code.");
        RuleFor(x => x.Images).Must(images => images.Count <= CustomRequestLimits.MaxImages)
            .WithMessage($"At most {CustomRequestLimits.MaxImages} reference images are allowed.");
        RuleForEach(x => x.Images).SetValidator(new CustomRequestImageInputValidator());
    }
}

public sealed class UpdateCustomRequestStatusRequestValidator : AbstractValidator<UpdateCustomRequestStatusRequest>
{
    public UpdateCustomRequestStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}
