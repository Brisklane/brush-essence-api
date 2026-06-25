using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Orders;

/// <summary>Full order read model (details page).</summary>
public class OrderDto
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public ShippingAddressDto ShippingAddress { get; set; } = new();

    public string Currency { get; set; } = "USD";

    public decimal Subtotal { get; set; }

    public decimal ShippingCost { get; set; }

    public decimal Total { get; set; }

    public IReadOnlyList<OrderItemDto> Items { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>Compact order read model for the history list.</summary>
public class OrderSummaryDto
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public string Currency { get; set; } = "USD";

    public decimal Total { get; set; }

    /// <summary>Total units across the order's lines.</summary>
    public int ItemCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public class OrderItemDto
{
    public Guid Id { get; set; }

    public Guid? PaintingId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}

public class ShippingAddressDto
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

/// <summary>A single milestone on the order's tracking timeline.</summary>
public class OrderStatusEventDto
{
    public OrderStatus Status { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}

/// <summary>Order tracking read model: where the order is and how it got there.</summary>
public class OrderTrackingDto
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    /// <summary>True when the order can progress no further (Delivered/Cancelled).</summary>
    public bool IsComplete { get; set; }

    /// <summary>Chronological list of status changes, oldest first.</summary>
    public IReadOnlyList<OrderStatusEventDto> History { get; set; } = [];
}
