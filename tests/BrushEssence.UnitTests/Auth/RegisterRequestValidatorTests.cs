using BrushEssence.Application.Auth;
using FluentValidation.TestHelper;
using Xunit;

namespace BrushEssence.UnitTests.Auth;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Valid_request_passes()
    {
        var result = _validator.TestValidate(new RegisterRequest
        {
            Email = "artist@example.com",
            Password = "Sup3rSecret",
            FullName = "Vincent",
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void Invalid_email_fails(string email)
    {
        var result = _validator.TestValidate(new RegisterRequest
        {
            Email = email,
            Password = "Sup3rSecret",
        });

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("short1A")]      // too short
    [InlineData("alllowercase1")] // no uppercase
    [InlineData("ALLUPPERCASE1")] // no lowercase
    [InlineData("NoDigitsHere")]  // no digit
    public void Weak_password_fails(string password)
    {
        var result = _validator.TestValidate(new RegisterRequest
        {
            Email = "artist@example.com",
            Password = password,
        });

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
