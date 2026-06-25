namespace BrushEssence.Domain.Entities;

/// <summary>
/// Single source of truth for allowed custom-request status transitions.
///
/// Happy path: Submitted → Reviewed → InProgress → Completed.
/// A request may be Declined from any non-terminal state.
/// </summary>
public static class CustomRequestStatusWorkflow
{
    private static readonly IReadOnlyDictionary<CustomRequestStatus, CustomRequestStatus[]> Transitions =
        new Dictionary<CustomRequestStatus, CustomRequestStatus[]>
        {
            [CustomRequestStatus.Submitted] = [CustomRequestStatus.Reviewed, CustomRequestStatus.Declined],
            [CustomRequestStatus.Reviewed] = [CustomRequestStatus.InProgress, CustomRequestStatus.Declined],
            [CustomRequestStatus.InProgress] = [CustomRequestStatus.Completed, CustomRequestStatus.Declined],
            [CustomRequestStatus.Completed] = [],
            [CustomRequestStatus.Declined] = [],
        };

    public static bool CanTransition(CustomRequestStatus from, CustomRequestStatus to)
        => from != to && Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public static IReadOnlyList<CustomRequestStatus> NextStates(CustomRequestStatus from)
        => Transitions.TryGetValue(from, out var allowed) ? allowed : [];

    public static bool IsTerminal(CustomRequestStatus status) => NextStates(status).Count == 0;
}
