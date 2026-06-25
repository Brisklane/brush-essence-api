namespace BrushEssence.Application.Carts;

/// <summary>Adds a painting to the cart (or increases its quantity).</summary>
public class AddCartItemRequest
{
    public Guid PaintingId { get; set; }

    /// <summary>Units to add. Defaults to 1 for a simple "add to cart" click.</summary>
    public int Quantity { get; set; } = 1;
}

/// <summary>Sets the absolute quantity for a painting already in the cart.</summary>
public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}
