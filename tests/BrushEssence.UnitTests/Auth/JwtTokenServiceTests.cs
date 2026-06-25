using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BrushEssence.Domain.Common;
using BrushEssence.Domain.Entities;
using BrushEssence.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace BrushEssence.UnitTests.Auth;

public class JwtTokenServiceTests
{
    private readonly JwtOptions _options = new()
    {
        Secret = "unit-test-signing-key-that-is-long-enough-1234567890",
        Issuer = "BrushEssence",
        Audience = "BrushEssence",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7,
    };

    private JwtTokenService CreateService() =>
        new(Options.Create(_options), TimeProvider.System);

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "artist@example.com",
        PasswordHash = "x",
        FullName = "Vincent",
    };

    [Fact]
    public void Access_token_is_valid_and_carries_expected_claims()
    {
        var service = CreateService();
        var user = CreateUser();

        var (token, expiresAt) = service.GenerateAccessToken(user, [Roles.Customer]);

        Assert.True(expiresAt > DateTimeOffset.UtcNow);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
            ClockSkew = TimeSpan.Zero,
        };

        var principal = new JwtSecurityTokenHandler()
            .ValidateToken(token, validationParameters, out _);

        Assert.Equal(user.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(user.Email, principal.FindFirst(ClaimTypes.Email)?.Value);
        Assert.Contains(principal.FindAll(ClaimTypes.Role), c => c.Value == Roles.Customer);
    }

    [Fact]
    public void Hash_token_is_deterministic_and_secure_tokens_are_unique()
    {
        var service = CreateService();

        Assert.Equal(service.HashToken("abc"), service.HashToken("abc"));
        Assert.NotEqual(service.GenerateSecureToken(), service.GenerateSecureToken());
    }
}
