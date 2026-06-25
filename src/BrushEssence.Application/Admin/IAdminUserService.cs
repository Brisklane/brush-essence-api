using BrushEssence.Application.Common.Models;

namespace BrushEssence.Application.Admin;

/// <summary>Admin operations for managing user accounts.</summary>
public interface IAdminUserService
{
    Task<PagedResult<AdminUserDto>> GetPagedAsync(AdminUserQuery query, CancellationToken cancellationToken = default);

    Task<AdminUserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user's active state and roles. An admin cannot lock themselves
    /// out by disabling their own account or removing their own Admin role.
    /// </summary>
    Task<AdminUserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
}
