using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Admin;

public sealed class DashboardService(
    IReportingRepository reporting,
    TimeProvider timeProvider) : IDashboardService
{
    private const int RecentCount = 5;
    private const int MinReportDays = 1;
    private const int MaxReportDays = 365;

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();

        var orders = await reporting.GetOrderAggregatesAsync(cancellationToken);
        var users = await reporting.GetUserAggregatesAsync(now.AddDays(-30), cancellationToken);
        var requests = await reporting.GetRequestAggregatesAsync(cancellationToken);
        var catalog = await reporting.GetCatalogAggregatesAsync(cancellationToken);
        var recentOrders = await reporting.GetRecentOrdersAsync(RecentCount, cancellationToken);
        var recentRequests = await reporting.GetRecentRequestsAsync(RecentCount, cancellationToken);

        return new DashboardSummaryDto
        {
            TotalRevenue = orders.Revenue,
            Currency = orders.Currency,
            TotalOrders = orders.TotalOrders,
            PendingOrders =
                CountFor(orders.ByStatus, OrderStatus.Placed) +
                CountFor(orders.ByStatus, OrderStatus.Processing),
            OrdersByStatus = ToStringKeyed(orders.ByStatus),

            TotalUsers = users.Total,
            ActiveUsers = users.Active,
            NewUsersLast30Days = users.NewSince,

            TotalRequests = requests.Total,
            OpenRequests =
                CountFor(requests.ByStatus, CustomRequestStatus.Submitted) +
                CountFor(requests.ByStatus, CustomRequestStatus.Reviewed) +
                CountFor(requests.ByStatus, CustomRequestStatus.InProgress),
            RequestsByStatus = ToStringKeyed(requests.ByStatus),

            TotalPaintings = catalog.Total,
            PublishedPaintings = catalog.Published,
            OutOfStockPaintings = catalog.OutOfStock,

            RecentOrders = recentOrders,
            RecentRequests = recentRequests,
        };
    }

    public async Task<ReportDto> GetReportAsync(int days, CancellationToken cancellationToken = default)
    {
        days = Math.Clamp(days, MinReportDays, MaxReportDays);

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var startDate = today.AddDays(-(days - 1));
        var since = new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var orderRows = await reporting.GetOrdersSinceAsync(since, cancellationToken);
        var userDates = await reporting.GetUserSignupsSinceAsync(since, cancellationToken);
        var requestDates = await reporting.GetRequestsSinceAsync(since, cancellationToken);

        // Seed one bucket per calendar day so the series has no gaps.
        var buckets = new Dictionary<DateOnly, ReportPointDto>(days);
        for (var date = startDate; date <= today; date = date.AddDays(1))
        {
            buckets[date] = new ReportPointDto { Date = date };
        }

        foreach (var row in orderRows)
        {
            if (!buckets.TryGetValue(DayOf(row.CreatedAt), out var point))
            {
                continue;
            }

            point.Orders++;
            if (row.Status != OrderStatus.Cancelled)
            {
                point.Revenue += row.Total;
            }
        }

        foreach (var created in userDates)
        {
            if (buckets.TryGetValue(DayOf(created), out var point))
            {
                point.NewUsers++;
            }
        }

        foreach (var created in requestDates)
        {
            if (buckets.TryGetValue(DayOf(created), out var point))
            {
                point.NewRequests++;
            }
        }

        var points = buckets.Values.OrderBy(p => p.Date).ToList();

        return new ReportDto
        {
            Days = days,
            Currency = orderRows.Count > 0 ? orderRows[0].Currency : "PKR",
            TotalRevenue = points.Sum(p => p.Revenue),
            TotalOrders = points.Sum(p => p.Orders),
            TotalNewUsers = points.Sum(p => p.NewUsers),
            TotalNewRequests = points.Sum(p => p.NewRequests),
            Points = points,
        };
    }

    private static DateOnly DayOf(DateTimeOffset value) => DateOnly.FromDateTime(value.UtcDateTime);

    private static int CountFor<TKey>(IReadOnlyDictionary<TKey, int> source, TKey key)
        where TKey : notnull
        => source.TryGetValue(key, out var count) ? count : 0;

    private static Dictionary<string, int> ToStringKeyed<TKey>(IReadOnlyDictionary<TKey, int> source)
        where TKey : notnull
        => source.ToDictionary(pair => pair.Key.ToString()!, pair => pair.Value);
}
