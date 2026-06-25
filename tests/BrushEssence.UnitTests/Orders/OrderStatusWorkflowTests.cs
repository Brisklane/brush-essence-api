using BrushEssence.Domain.Entities;
using Xunit;

namespace BrushEssence.UnitTests.Orders;

public class OrderStatusWorkflowTests
{
    [Theory]
    [InlineData(OrderStatus.Placed, OrderStatus.Processing)]
    [InlineData(OrderStatus.Placed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Processing, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Processing, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled)]
    public void Allows_valid_transitions(OrderStatus from, OrderStatus to)
        => Assert.True(OrderStatusWorkflow.CanTransition(from, to));

    [Theory]
    [InlineData(OrderStatus.Placed, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Placed, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Processing)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Processing)]
    public void Rejects_invalid_transitions(OrderStatus from, OrderStatus to)
        => Assert.False(OrderStatusWorkflow.CanTransition(from, to));

    [Fact]
    public void Rejects_transition_to_same_status()
        => Assert.False(OrderStatusWorkflow.CanTransition(OrderStatus.Placed, OrderStatus.Placed));

    [Theory]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void Terminal_states_have_no_next_states(OrderStatus status)
    {
        Assert.True(OrderStatusWorkflow.IsTerminal(status));
        Assert.Empty(OrderStatusWorkflow.NextStates(status));
    }

    [Fact]
    public void Placed_can_advance_or_cancel()
    {
        var next = OrderStatusWorkflow.NextStates(OrderStatus.Placed);
        Assert.Contains(OrderStatus.Processing, next);
        Assert.Contains(OrderStatus.Cancelled, next);
    }
}
