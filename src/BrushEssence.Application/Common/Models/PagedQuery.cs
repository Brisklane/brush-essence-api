namespace BrushEssence.Application.Common.Models;

/// <summary>
/// Base for paged, searchable list queries. Page/size are clamped to safe
/// bounds so malformed query strings degrade gracefully instead of 400-ing.
/// </summary>
public abstract class PagedQuery
{
    public const int MaxPageSize = 100;

    private int _page = 1;
    private int _pageSize = 20;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > MaxPageSize ? 20 : value;
    }

    /// <summary>Free-text search term (interpreted per query).</summary>
    public string? Search { get; set; }

    /// <summary>Number of rows to skip for the current page.</summary>
    public int Skip => (Page - 1) * PageSize;
}
