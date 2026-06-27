using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A reusable art medium / technique (e.g. "Oil on canvas", "Acrylic on canvas")
/// that paintings are tagged with. Managed by admins like categories, so the
/// list is picked from rather than re-typed.
/// </summary>
public class Medium : AuditableEntity
{
    /// <summary>Display name; unique.</summary>
    public required string Name { get; set; }

    // Navigation property
    public ICollection<Painting> Paintings { get; set; } = [];
}
