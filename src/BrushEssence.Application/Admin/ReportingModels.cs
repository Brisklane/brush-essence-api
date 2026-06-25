using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Admin;

/// <summary>Aggregate order figures for the dashboard.</summary>
public sealed record OrderAggregates(
    int TotalOrders,
    decimal Revenue,
    string Currency,
    IReadOnlyDictionary<OrderStatus, int> ByStatus);

/// <summary>Aggregate user figures for the dashboard.</summary>
public sealed record UserAggregates(int Total, int Active, int NewSince);

/// <summary>Aggregate custom-request figures for the dashboard.</summary>
public sealed record RequestAggregates(
    int Total,
    IReadOnlyDictionary<CustomRequestStatus, int> ByStatus);

/// <summary>Aggregate catalogue figures for the dashboard.</summary>
public sealed record CatalogAggregates(int Total, int Published, int OutOfStock);

/// <summary>A single order's contribution to the activity time-series.</summary>
public sealed record OrderActivityRow(DateTimeOffset CreatedAt, decimal Total, string Currency, OrderStatus Status);
