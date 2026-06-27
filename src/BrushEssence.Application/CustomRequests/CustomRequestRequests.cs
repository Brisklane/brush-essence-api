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
    public List<CustomRequestImageInput> Images { get; set; } = [];
}

/// <summary>Admin action: move a request along its lifecycle.</summary>
public class UpdateCustomRequestStatusRequest
{
    public CustomRequestStatus Status { get; set; }

    /// <summary>Optional message recorded on the timeline.</summary>
    public string? Note { get; set; }
}

/// <summary>Admin action: send the customer a price quote (moves to Quoted).</summary>
public class SetCustomRequestQuoteRequest
{
    public decimal Amount { get; set; }

    /// <summary>Optional message to the customer (e.g. timeline, what's included).</summary>
    public string? Note { get; set; }
}
