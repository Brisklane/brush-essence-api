using FluentValidation;

namespace BrushEssence.Application.Mediums;

public sealed class CreateMediumRequestValidator : AbstractValidator<CreateMediumRequest>
{
    public CreateMediumRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateMediumRequestValidator : AbstractValidator<UpdateMediumRequest>
{
    public UpdateMediumRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
