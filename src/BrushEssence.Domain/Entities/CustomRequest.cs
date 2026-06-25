using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A customer's request for a bespoke, commissioned painting. The customer
/// describes what they want (with optional reference images and a budget), and
/// the artist moves it through the <see cref="CustomRequestStatus"/> lifecycle.
/// </summary>
public class CustomRequest : AuditableEntity
{
    /// <summary>Requesting customer.</summary>
    public Guid UserId { get; set; }

    /// <summary>Email captured at submission (snapshot of the user's email).</summary>
    public required string CustomerEmail { get; set; }

    /// <summary>Short summary / working title for the commission.</summary>
    public required string Title { get; set; }

    /// <summary>Full description of the desired piece.</summary>
    public required string Description { get; set; }

    /// <summary>Free-text size/format preference, e.g. "60 × 90 cm, landscape".</summary>
    public string? PreferredSize { get; set; }

    /// <summary>Optional budget the customer has in mind.</summary>
    public decimal? BudgetAmount { get; set; }

    /// <summary>ISO 4217 currency for <see cref="BudgetAmount"/>.</summary>
    public string Currency { get; set; } = "USD";

    public CustomRequestStatus Status { get; set; } = CustomRequestStatus.Submitted;

    // Navigation properties
    public User? User { get; set; }
    public ICollection<CustomRequestImage> Images { get; set; } = [];
    public ICollection<CustomRequestStatusEvent> StatusHistory { get; set; } = [];

    /// <summary>True while the customer may still edit the request (pre-review).</summary>
    public bool IsEditable => Status == CustomRequestStatus.Submitted;

    /// <summary>
    /// Moves the request to <paramref name="next"/>, recording a history entry.
    /// Caller validates the transition first
    /// (<see cref="CustomRequestStatusWorkflow.CanTransition"/>).
    /// </summary>
    public void TransitionTo(CustomRequestStatus next, string? note = null)
    {
        Status = next;
        StatusHistory.Add(new CustomRequestStatusEvent { Status = next, Note = note });
    }
}
