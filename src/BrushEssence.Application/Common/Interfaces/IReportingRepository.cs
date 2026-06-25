using BrushEssence.Application.Admin;

namespace BrushEssence.Application.Common.Interfaces;

/// <summary>
/// Read-only aggregate queries that power the admin dashboard and reports.
/// Kept separate from the per-aggregate repositories because it spans them.
/// </summary>
public interface IReportingRepository
{
    Task<OrderAggregates> GetOrderAggregatesAsync(CancellationToken cancellationToken = default);

    Task<UserAggregates> GetUserAggregatesAsync(DateTimeOffset newSince, CancellationToken cancellationToken = default);

    Task<RequestAggregates> GetRequestAggregatesAsync(CancellationToken cancellationToken = default);

    Task<CatalogAggregates> GetCatalogAggregatesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminOrderListItemDto>> GetRecentOrdersAsync(int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminCustomRequestListItemDto>> GetRecentRequestsAsync(int take, CancellationToken cancellationToken = default);

    // Raw rows since a point in time, bucketed into a daily series by the service.
    Task<IReadOnlyList<OrderActivityRow>> GetOrdersSinceAsync(DateTimeOffset since, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DateTimeOffset>> GetUserSignupsSinceAsync(DateTimeOffset since, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DateTimeOffset>> GetRequestsSinceAsync(DateTimeOffset since, CancellationToken cancellationToken = default);
}
