using BrushEssence.Domain.Common;
using FluentValidation;

namespace BrushEssence.Application.Admin;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Roles)
            .NotNull()
            .Must(roles => roles.Count > 0)
            .WithMessage("A user must have at least one role.")
            .Must(roles => roles.All(Roles.All.Contains))
            .WithMessage($"Roles must be one of: {string.Join(", ", Roles.All)}.")
            .Must(roles => roles.Distinct().Count() == roles.Count)
            .WithMessage("Roles must not contain duplicates.");
    }
}
