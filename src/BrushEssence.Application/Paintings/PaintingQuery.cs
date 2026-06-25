namespace BrushEssence.Application.Paintings;

/// <summary>Filter + pagination parameters for listing paintings.</summary>
public sealed class PaintingQuery
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

    /// <summary>Free-text search over title and description.</summary>
    public string? Search { get; set; }

    public Guid? CategoryId { get; set; }

    public bool? IsPublished { get; set; }
}
