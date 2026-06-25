using BrushEssence.Application.Common.Models;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Admin;

/// <summary>Admin order list row (includes the customer the order belongs to).</summary>
public class AdminOrderListItemDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>Search/filter/paging parameters for the admin order list.</summary>
public sealed class AdminOrderQuery : PagedQuery
{
    /// <summary>Raw status filter from the query string (degrades gracefully).</summary>
    public string? Status { get; set; }

    public OrderStatus? StatusFilter =>
        Enum.TryParse<OrderStatus>(Status, ignoreCase: true, out var parsed) ? parsed : null;
}
