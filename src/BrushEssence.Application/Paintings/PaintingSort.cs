namespace BrushEssence.Application.Paintings;

/// <summary>Supported sort orders for catalogue listings.</summary>
public enum PaintingSort
{
    /// <summary>Most recently added first (default).</summary>
    Newest = 0,

    /// <summary>Oldest added first.</summary>
    Oldest = 1,

    /// <summary>Cheapest first.</summary>
    PriceAsc = 2,

    /// <summary>Most expensive first.</summary>
    PriceDesc = 3,

    /// <summary>Title A → Z.</summary>
    TitleAsc = 4,

    /// <summary>Title Z → A.</summary>
    TitleDesc = 5,
}
