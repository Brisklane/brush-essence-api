namespace BrushEssence.Domain.ValueObjects;

/// <summary>
/// A postal shipping address captured at checkout. Modelled as an EF Core owned
/// type (no identity of its own) — it lives and dies with its <see cref="Entities.Order"/>.
/// </summary>
public class ShippingAddress
{
    public required string FullName { get; set; }

    public required string Line1 { get; set; }

    public string? Line2 { get; set; }

    public required string City { get; set; }

    /// <summary>State / province / region.</summary>
    public string? Region { get; set; }

    public required string PostalCode { get; set; }

    public required string Country { get; set; }

    /// <summary>Contact phone for the courier.</summary>
    public string? Phone { get; set; }
}
