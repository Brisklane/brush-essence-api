using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A shopping cart. Exactly one of <see cref="UserId"/> (a signed-in customer)
/// or <see cref="AnonymousId"/> (a guest's opaque cart token) identifies the
/// owner, so the same cart survives reloads and can be merged into the user's
/// cart when a guest signs in.
/// </summary>
public class Cart : AuditableEntity
{
    /// <summary>Owning user, when the cart belongs to a signed-in customer.</summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Opaque guest token, when the cart belongs to an anonymous visitor. The
    /// browser generates and stores this; it is sent on the <c>X-Cart-Token</c>
    /// header.
    /// </summary>
    public Guid? AnonymousId { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = [];

    /// <summary>Finds an existing line for a painting, or null when absent.</summary>
    public CartItem? FindItem(Guid paintingId)
        => Items.FirstOrDefault(item => item.PaintingId == paintingId);
}
