using BrushEssence.Application.Common.Exceptions;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Application.Common.Models;
using BrushEssence.Domain.Common;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Admin;

public sealed class AdminUserService(
    IUserRepository users,
    IRoleRepository roles,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IAdminUserService
{
    public async Task<PagedResult<AdminUserDto>> GetPagedAsync(
        AdminUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await users.GetPagedAsync(query, cancellationToken);

        return new PagedResult<AdminUserDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<AdminUserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdWithRolesAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        return ToDto(user);
    }

    public async Task<AdminUserDto> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdWithRolesAsync(id, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var editingSelf = currentUser.UserId == user.Id;
        var keepsAdmin = request.Roles.Contains(Roles.Admin);

        // Guard against an admin accidentally locking themselves out.
        if (editingSelf && !request.IsActive)
        {
            throw new BadRequestException("You cannot disable your own account.");
        }

        if (editingSelf && !keepsAdmin)
        {
            throw new BadRequestException("You cannot remove your own Admin role.");
        }

        user.IsActive = request.IsActive;
        await ApplyRolesAsync(user, request.Roles, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    /// <summary>Replaces the user's roles with the requested set.</summary>
    private async Task ApplyRolesAsync(User user, List<string> roleNames, CancellationToken cancellationToken)
    {
        var desired = new List<UserRole>();

        foreach (var name in roleNames.Distinct())
        {
            var role = await roles.GetByNameAsync(name, cancellationToken)
                ?? throw new BadRequestException($"Unknown role '{name}'.");

            desired.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role });
        }

        user.UserRoles.Clear();
        foreach (var userRole in desired)
        {
            user.UserRoles.Add(userRole);
        }
    }

    private static AdminUserDto ToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FullName = user.FullName,
        IsActive = user.IsActive,
        IsEmailVerified = user.IsEmailVerified,
        Roles = user.UserRoles
            .Where(ur => ur.Role != null)
            .Select(ur => ur.Role!.Name)
            .OrderBy(name => name)
            .ToArray(),
        LastLoginAt = user.LastLoginAt,
        CreatedAt = user.CreatedAt,
    };
}
