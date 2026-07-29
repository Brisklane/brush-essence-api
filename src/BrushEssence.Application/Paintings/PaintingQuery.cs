namespace BrushEssence.Application.Paintings;

/// <summary>Search, filter, sort and pagination parameters for listing paintings.</summary>
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

    /// <summary>Filter by a single category (kept for backwards compatibility).</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Filter by one or more categories (checkbox-style multi-select). When any
    /// ids are supplied they take precedence over <see cref="CategoryId"/>.
    /// </summary>
    public List<Guid>? CategoryIds { get; set; }

    /// <summary>Inclusive minimum price.</summary>
    public decimal? MinPrice { get; set; }

    /// <summary>Inclusive maximum price.</summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>Filter by a specific medium.</summary>
    public Guid? MediumId { get; set; }

    /// <summary>
    /// Filter to paintings matching any of these mediums (OR). When ids are
    /// supplied they take precedence over <see cref="MediumId"/>.
    /// </summary>
    public List<Guid>? MediumIds { get; set; }

    /// <summary>
    /// Restrict to published / unpublished paintings. The public storefront
    /// passes <c>true</c>; admin listings leave this unset to see everything.
    /// </summary>
    public bool? IsPublished { get; set; }

    /// <summary>
    /// Raw sort token from the query string (e.g. "priceAsc"). Bound as a string
    /// so an unknown value degrades gracefully to the default instead of 400-ing.
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>Parsed sort order; defaults to newest-first.</summary>
    public PaintingSort SortOrder =>
        Enum.TryParse<PaintingSort>(Sort, ignoreCase: true, out var parsed)
            ? parsed
            : PaintingSort.Newest;
}
