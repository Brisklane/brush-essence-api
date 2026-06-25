using BrushEssence.Application.Common.Models;

namespace BrushEssence.Application.Admin;

/// <summary>Admin-facing projection of a user account.</summary>
public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>Search/filter/paging parameters for the admin user list.</summary>
public sealed class AdminUserQuery : PagedQuery
{
    /// <summary>Restrict to users holding this role (e.g. "Admin").</summary>
    public string? Role { get; set; }

    /// <summary>Restrict to active / disabled accounts.</summary>
    public bool? IsActive { get; set; }
}

/// <summary>Admin update of a user's account state and roles.</summary>
public class UpdateUserRequest
{
    public bool IsActive { get; set; } = true;

    /// <summary>The complete set of roles the user should have.</summary>
    public List<string> Roles { get; set; } = [];
}
