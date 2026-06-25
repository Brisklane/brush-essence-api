using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.CustomRequests;

/// <summary>Full custom-request read model (details page).</summary>
public class CustomRequestDto
{
    public Guid Id { get; set; }

    public string CustomerEmail { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? PreferredSize { get; set; }

    public decimal? BudgetAmount { get; set; }

    public string Currency { get; set; } = "USD";

    public CustomRequestStatus Status { get; set; }

    /// <summary>True while the customer may still edit the request (pre-review).</summary>
    public bool IsEditable { get; set; }

    public IReadOnlyList<CustomRequestImageDto> Images { get; set; } = [];

    /// <summary>Status updates, oldest first.</summary>
    public IReadOnlyList<CustomRequestStatusEventDto> History { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>Compact read model for the request history list.</summary>
public class CustomRequestSummaryDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public CustomRequestStatus Status { get; set; }

    /// <summary>How many reference images are attached.</summary>
    public int ImageCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public class CustomRequestImageDto
{
    public Guid Id { get; set; }

    public string Url { get; set; } = string.Empty;

    public string? FileName { get; set; }
}

public class CustomRequestStatusEventDto
{
    public CustomRequestStatus Status { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}
