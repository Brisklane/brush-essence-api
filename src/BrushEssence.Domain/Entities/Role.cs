using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A named authorization role (e.g. Admin, Customer). Assigned to users via the
/// <see cref="UserRole"/> join entity (many-to-many).
/// </summary>
public class Role : AuditableEntity
{
    /// <summary>Unique role name; see <see cref="Roles"/> for the canonical set.</summary>
    public required string Name { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
