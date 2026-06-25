using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.CustomRequests;

/// <summary>A reference image (already uploaded) to attach to a request.</summary>
public class CustomRequestImageInput
{
    public string Url { get; set; } = string.Empty;
    public string? FileName { get; set; }
}

/// <summary>Submits a new custom painting request.</summary>
public class CreateCustomRequestRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PreferredSize { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public List<CustomRequestImageInput> Images { get; set; } = [];
}

/// <summary>
/// Edits an existing request. Only permitted while the request is still
/// editable (status Submitted). The image list replaces the current set.
/// </summary>
public class UpdateCustomRequestRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PreferredSize { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public List<CustomRequestImageInput> Images { get; set; } = [];
}

/// <summary>Admin action: move a request along its lifecycle.</summary>
public class UpdateCustomRequestStatusRequest
{
    public CustomRequestStatus Status { get; set; }

    /// <summary>Optional message recorded on the timeline (e.g. a quote).</summary>
    public string? Note { get; set; }
}
