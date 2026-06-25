using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A grouping for paintings (e.g. "Landscapes", "Portraits"). A category has
/// many paintings; a painting belongs to at most one category.
/// </summary>
public class Category : AuditableEntity
{
    /// <summary>Display name; unique.</summary>
    public required string Name { get; set; }

    /// <summary>URL-friendly identifier derived from the name; unique.</summary>
    public required string Slug { get; set; }

    public string? Description { get; set; }

    // Navigation property
    public ICollection<Painting> Paintings { get; set; } = [];
}
