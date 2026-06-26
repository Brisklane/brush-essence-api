namespace BrushEssence.Application.Common.Models;

/// <summary>
/// Concrete, bindable paging query for endpoints that only need page/size/search
/// (e.g. public review lists). Feature-specific lists derive their own queries
/// from <see cref="PagedQuery"/> instead.
/// </summary>
public sealed class PaginationQuery : PagedQuery;
