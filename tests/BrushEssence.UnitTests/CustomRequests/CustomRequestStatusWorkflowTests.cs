using BrushEssence.Domain.Entities;
using Xunit;

namespace BrushEssence.UnitTests.CustomRequests;

public class CustomRequestStatusWorkflowTests
{
    [Theory]
    [InlineData(CustomRequestStatus.Submitted, CustomRequestStatus.Reviewed)]
    [InlineData(CustomRequestStatus.Submitted, CustomRequestStatus.Quoted)]
    [InlineData(CustomRequestStatus.Submitted, CustomRequestStatus.Declined)]
    [InlineData(CustomRequestStatus.Reviewed, CustomRequestStatus.Quoted)]
    [InlineData(CustomRequestStatus.Reviewed, CustomRequestStatus.Declined)]
    [InlineData(CustomRequestStatus.Quoted, CustomRequestStatus.InProgress)]
    [InlineData(CustomRequestStatus.Quoted, CustomRequestStatus.Declined)]
    [InlineData(CustomRequestStatus.InProgress, CustomRequestStatus.Completed)]
    [InlineData(CustomRequestStatus.InProgress, CustomRequestStatus.Declined)]
    public void Allows_valid_transitions(CustomRequestStatus from, CustomRequestStatus to)
        => Assert.True(CustomRequestStatusWorkflow.CanTransition(from, to));

    [Theory]
    [InlineData(CustomRequestStatus.Submitted, CustomRequestStatus.InProgress)]
    [InlineData(CustomRequestStatus.Submitted, CustomRequestStatus.Completed)]
    [InlineData(CustomRequestStatus.Completed, CustomRequestStatus.InProgress)]
    [InlineData(CustomRequestStatus.Declined, CustomRequestStatus.Reviewed)]
    [InlineData(CustomRequestStatus.Reviewed, CustomRequestStatus.Completed)]
    public void Rejects_invalid_transitions(CustomRequestStatus from, CustomRequestStatus to)
        => Assert.False(CustomRequestStatusWorkflow.CanTransition(from, to));

    [Fact]
    public void Rejects_transition_to_same_status()
        => Assert.False(CustomRequestStatusWorkflow.CanTransition(
            CustomRequestStatus.Submitted, CustomRequestStatus.Submitted));

    [Theory]
    [InlineData(CustomRequestStatus.Completed)]
    [InlineData(CustomRequestStatus.Declined)]
    public void Terminal_states_have_no_next_states(CustomRequestStatus status)
    {
        Assert.True(CustomRequestStatusWorkflow.IsTerminal(status));
        Assert.Empty(CustomRequestStatusWorkflow.NextStates(status));
    }
}
