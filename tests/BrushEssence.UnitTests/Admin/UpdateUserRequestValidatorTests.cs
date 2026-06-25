using BrushEssence.Application.Admin;
using BrushEssence.Domain.Common;
using FluentValidation.TestHelper;
using Xunit;

namespace BrushEssence.UnitTests.Admin;

public class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _validator = new();

    [Fact]
    public void Valid_request_passes_validation()
    {
        var request = new UpdateUserRequest { IsActive = true, Roles = [Roles.Customer] };

        _validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_roles_fail_validation()
    {
        var request = new UpdateUserRequest { IsActive = true, Roles = [] };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Roles);
    }

    [Fact]
    public void Unknown_role_fails_validation()
    {
        var request = new UpdateUserRequest { IsActive = true, Roles = ["Wizard"] };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Roles);
    }

    [Fact]
    public void Duplicate_roles_fail_validation()
    {
        var request = new UpdateUserRequest
        {
            IsActive = true,
            Roles = [Roles.Admin, Roles.Admin],
        };

        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Roles);
    }
}
