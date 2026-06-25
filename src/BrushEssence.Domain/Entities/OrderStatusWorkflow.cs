namespace BrushEssence.Domain.Entities;

/// <summary>
/// The single source of truth for which order status transitions are allowed.
/// Keeping the rules here (rather than scattered through services) makes the
/// lifecycle easy to reason about and enforce consistently.
///
/// Happy path: Placed → Processing → Shipped → Delivered.
/// An order may be Cancelled from any non-terminal state.
/// </summary>
public static class OrderStatusWorkflow
{
    private static readonly IReadOnlyDictionary<OrderStatus, OrderStatus[]> Transitions =
        new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.Placed] = [OrderStatus.Processing, OrderStatus.Cancelled],
            [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Cancelled],
            [OrderStatus.Shipped] = [OrderStatus.Delivered, OrderStatus.Cancelled],
            [OrderStatus.Delivered] = [],
            [OrderStatus.Cancelled] = [],
        };

    /// <summary>True when <paramref name="to"/> is a legal next state of <paramref name="from"/>.</summary>
    public static bool CanTransition(OrderStatus from, OrderStatus to)
        => from != to && Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    /// <summary>The states an order in <paramref name="from"/> may move to next.</summary>
    public static IReadOnlyList<OrderStatus> NextStates(OrderStatus from)
        => Transitions.TryGetValue(from, out var allowed) ? allowed : [];

    /// <summary>True when no further transitions are possible (Delivered/Cancelled).</summary>
    public static bool IsTerminal(OrderStatus status) => NextStates(status).Count == 0;
}
