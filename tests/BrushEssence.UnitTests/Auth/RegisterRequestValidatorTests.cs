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
            Password = "Secret!1",
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
            Password = "Secret!1",
        });

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    // Policy: min 8 chars, at least one uppercase, one lowercase, one digit, one special char.
    [Theory]
    [InlineData("Aa!1aa")]    // too short (< 8)
    [InlineData("abcdef1!")]  // no uppercase
    [InlineData("ABCDEF1!")]  // no lowercase
    [InlineData("Abcdefg!")]  // no digit
    [InlineData("Abcdefg1")]  // no special character
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
