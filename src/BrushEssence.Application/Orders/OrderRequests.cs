using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Orders;

/// <summary>Places an order from the current user's cart.</summary>
public class CreateOrderRequest
{
    public ShippingAddressInput ShippingAddress { get; set; } = new();
}

/// <summary>Shipping address supplied at checkout.</summary>
public class ShippingAddressInput
{
    public string FullName { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

/// <summary>Admin action: advance an order to the next status.</summary>
public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }

    /// <summary>Optional note recorded on the timeline (e.g. tracking number).</summary>
    public string? Note { get; set; }
}
