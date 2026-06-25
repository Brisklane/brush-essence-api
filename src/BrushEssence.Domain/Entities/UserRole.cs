namespace BrushEssence.Domain.Entities;

/// <summary>
/// Join entity for the many-to-many relationship between <see cref="User"/> and
/// <see cref="Role"/>. Uses a composite primary key (UserId, RoleId).
/// </summary>
public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Role? Role { get; set; }
}
