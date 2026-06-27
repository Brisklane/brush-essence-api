using BrushEssence.Api.Extensions;
using BrushEssence.Application.Auth;
using BrushEssence.Application.Common.Exceptions;
using BrushEssence.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BrushEssence.Api.Controllers;

/// <summary>
/// Authentication endpoints. The raw refresh token is returned in the response
/// body so the Next.js BFF can store it in an httpOnly cookie; it is never
/// persisted server-side in plaintext. Rate-limited more tightly than the rest
/// of the API to blunt brute-force and credential-stuffing attempts.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[EnableRateLimiting(RateLimitPolicies.Auth)]
public sealed class AuthController(
    IAuthService authService,
    ICurrentUser currentUser,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<ForgotPasswordRequest> forgotPasswordValidator,
    IValidator<ResetPasswordRequest> resetPasswordValidator,
    IValidator<VerifyEmailRequest> verifyEmailValidator) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResult>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        await registerValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await authService.RegisterAsync(request, GetIpAddress(), cancellationToken));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResult>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await authService.LoginAsync(request, GetIpAddress(), cancellationToken));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResult>> Refresh(
        RefreshRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await authService.RefreshAsync(request.RefreshToken, GetIpAddress(), cancellationToken));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await forgotPasswordValidator.ValidateAndThrowAsync(request, cancellationToken);
        await authService.ForgotPasswordAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await resetPasswordValidator.ValidateAndThrowAsync(request, cancellationToken);
        await authService.ResetPasswordAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(
        VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        await verifyEmailValidator.ValidateAndThrowAsync(request, cancellationToken);
        await authService.VerifyEmailAsync(request, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("resend-verification")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResendVerification(CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new AuthenticationException("The access token does not contain a user id.");

        await authService.ResendVerificationAsync(userId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new AuthenticationException("The access token does not contain a user id.");

        return Ok(await authService.GetProfileAsync(userId, cancellationToken));
    }

    private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();
}
