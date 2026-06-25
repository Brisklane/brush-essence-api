using BrushEssence.Application.Common.Models;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Admin;

/// <summary>Admin custom-request list row (includes the requesting customer).</summary>
public class AdminCustomRequestListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public CustomRequestStatus Status { get; set; }
    public int ImageCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>Search/filter/paging parameters for the admin custom-request list.</summary>
public sealed class AdminCustomRequestQuery : PagedQuery
{
    public string? Status { get; set; }

    public CustomRequestStatus? StatusFilter =>
        Enum.TryParse<CustomRequestStatus>(Status, ignoreCase: true, out var parsed) ? parsed : null;
}
