using BrushEssence.Application.Common.Exceptions;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Common;
using BrushEssence.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BrushEssence.Application.Auth;

/// <summary>
/// Orchestrates authentication. Depends only on abstractions (repositories,
/// password hasher, token service) so it stays free of EF Core / JWT details.
/// </summary>
public sealed class AuthService(
    IUserRepository users,
    IRoleRepository roles,
    IRefreshTokenRepository refreshTokens,
    IPasswordResetTokenRepository resetTokens,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokenService,
    TimeProvider timeProvider,
    ILogger<AuthService> logger) : IAuthService
{
    private const int PasswordResetTokenLifetimeHours = 1;

    public async Task<AuthResult> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var email = Normalize(request.Email);

        if (await users.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var customerRole = await roles.GetByNameAsync(Roles.Customer, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Seed role '{Roles.Customer}' is missing. Apply migrations first.");

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim(),
        };
        user.UserRoles.Add(new UserRole { RoleId = customerRole.Id });

        await users.AddAsync(user, cancellationToken);

        return await IssueTokensAsync(user, [Roles.Customer], ipAddress, cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var email = Normalize(request.Email);
        var user = await users.GetByEmailAsync(email, cancellationToken);

        // Verify even when the user is null to keep timing uniform is overkill here;
        // a simple generic message avoids leaking which part was wrong.
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new AuthenticationException("This account has been disabled.");
        }

        return await IssueTokensAsync(user, RolesOf(user), ipAddress, cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var hash = tokenService.HashToken(refreshToken);
        var existing = await refreshTokens.GetByHashWithUserRolesAsync(hash, cancellationToken);

        if (existing is null || !existing.IsActive || existing.User is null)
        {
            throw new AuthenticationException("Invalid or expired refresh token.");
        }

        var user = existing.User;
        var roleNames = RolesOf(user);

        var (accessToken, accessExpiresAt) = tokenService.GenerateAccessToken(user, roleNames);
        var (rawRefresh, refreshExpiresAt) = tokenService.GenerateRefreshToken();
        var newHash = tokenService.HashToken(rawRefresh);

        // Rotate: revoke the presented token and link it to its replacement.
        var now = timeProvider.GetUtcNow();
        existing.IsRevoked = true;
        existing.RevokedAt = now;
        existing.ReplacedByTokenHash = newHash;

        await refreshTokens.AddAsync(
            new RefreshToken
            {
                UserId = user.Id,
                TokenHash = newHash,
                ExpiresAt = refreshExpiresAt,
                CreatedByIp = ipAddress,
            },
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResult
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshToken = rawRefresh,
            User = MapToDto(user, roleNames),
        };
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hash = tokenService.HashToken(refreshToken);
        var existing = await refreshTokens.GetByHashWithUserRolesAsync(hash, cancellationToken);

        // Idempotent: silently succeed if the token is unknown or already revoked.
        if (existing is { IsRevoked: false })
        {
            existing.IsRevoked = true;
            existing.RevokedAt = timeProvider.GetUtcNow();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = Normalize(request.Email);
        var user = await users.GetByEmailAsync(email, cancellationToken);

        if (user is not null)
        {
            var rawToken = tokenService.GenerateSecureToken();
            await resetTokens.AddAsync(
                new PasswordResetToken
                {
                    UserId = user.Id,
                    TokenHash = tokenService.HashToken(rawToken),
                    ExpiresAt = timeProvider.GetUtcNow().AddHours(PasswordResetTokenLifetimeHours),
                },
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            // TODO: deliver this token to the user via email. No email provider is
            // configured yet, so the token is logged for local/dev testing only.
            logger.LogWarning(
                "Password reset requested for {Email}. Reset token (DEV ONLY): {ResetToken}",
                email,
                rawToken);
        }

        // Always succeed regardless of whether the account exists (prevents enumeration).
    }

    public async Task ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var hash = tokenService.HashToken(request.Token);
        var resetToken = await resetTokens.GetByHashWithUserAsync(hash, cancellationToken);

        if (resetToken is null || !resetToken.IsActive || resetToken.User is null)
        {
            throw new BadRequestException("This password reset link is invalid or has expired.");
        }

        var now = timeProvider.GetUtcNow();
        resetToken.User.PasswordHash = passwordHasher.Hash(request.NewPassword);
        resetToken.IsUsed = true;
        resetToken.UsedAt = now;

        // Revoke all sessions after a password change.
        await refreshTokens.RevokeAllActiveForUserAsync(resetToken.UserId, now, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdWithRolesAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return MapToDto(user, RolesOf(user));
    }

    private async Task<AuthResult> IssueTokensAsync(
        User user,
        IReadOnlyList<string> roleNames,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var (accessToken, accessExpiresAt) = tokenService.GenerateAccessToken(user, roleNames);
        var (rawRefresh, refreshExpiresAt) = tokenService.GenerateRefreshToken();

        await refreshTokens.AddAsync(
            new RefreshToken
            {
                UserId = user.Id,
                TokenHash = tokenService.HashToken(rawRefresh),
                ExpiresAt = refreshExpiresAt,
                CreatedByIp = ipAddress,
            },
            cancellationToken);

        user.LastLoginAt = timeProvider.GetUtcNow();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResult
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshToken = rawRefresh,
            User = MapToDto(user, roleNames),
        };
    }

    private static IReadOnlyList<string> RolesOf(User user) =>
        user.UserRoles
            .Where(ur => ur.Role is not null)
            .Select(ur => ur.Role!.Name)
            .ToArray();

    private static UserDto MapToDto(User user, IReadOnlyList<string> roleNames) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FullName = user.FullName,
        IsEmailVerified = user.IsEmailVerified,
        Roles = roleNames,
        CreatedAt = user.CreatedAt,
    };

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
