using BrushEssence.Application.Admin;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;
using Xunit;

namespace BrushEssence.UnitTests.Admin;

public class DashboardServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 26, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Report_buckets_activity_by_day_and_fills_gaps()
    {
        var repo = new FakeReportingRepository
        {
            OrdersSince =
            [
                new OrderActivityRow(new DateTimeOffset(2026, 6, 26, 9, 0, 0, TimeSpan.Zero), 100m, "USD", OrderStatus.Placed),
                // Cancelled orders count as volume but not revenue.
                new OrderActivityRow(new DateTimeOffset(2026, 6, 26, 9, 30, 0, TimeSpan.Zero), 50m, "USD", OrderStatus.Cancelled),
                new OrderActivityRow(new DateTimeOffset(2026, 6, 24, 12, 0, 0, TimeSpan.Zero), 30m, "USD", OrderStatus.Delivered),
            ],
            UserSignups = [new DateTimeOffset(2026, 6, 25, 8, 0, 0, TimeSpan.Zero)],
            Requests = [new DateTimeOffset(2026, 6, 24, 8, 0, 0, TimeSpan.Zero)],
        };
        var service = new DashboardService(repo, new FixedTimeProvider(Now));

        var report = await service.GetReportAsync(7);

        Assert.Equal(7, report.Days);
        Assert.Equal(7, report.Points.Count); // 2026-06-20 .. 2026-06-26 inclusive
        Assert.Equal("USD", report.Currency);
        Assert.Equal(130m, report.TotalRevenue); // 100 + 30, cancelled 50 excluded
        Assert.Equal(3, report.TotalOrders);
        Assert.Equal(1, report.TotalNewUsers);
        Assert.Equal(1, report.TotalNewRequests);

        var today = report.Points[^1];
        Assert.Equal(new DateOnly(2026, 6, 26), today.Date);
        Assert.Equal(100m, today.Revenue);
        Assert.Equal(2, today.Orders);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(100000, 365)]
    public async Task Report_clamps_days_to_range(int requested, int expected)
    {
        var service = new DashboardService(new FakeReportingRepository(), new FixedTimeProvider(Now));

        var report = await service.GetReportAsync(requested);

        Assert.Equal(expected, report.Days);
        Assert.Equal(expected, report.Points.Count);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeReportingRepository : IReportingRepository
    {
        public IReadOnlyList<OrderActivityRow> OrdersSince { get; init; } = [];
        public IReadOnlyList<DateTimeOffset> UserSignups { get; init; } = [];
        public IReadOnlyList<DateTimeOffset> Requests { get; init; } = [];

        public Task<OrderAggregates> GetOrderAggregatesAsync(CancellationToken ct = default)
            => Task.FromResult(new OrderAggregates(0, 0m, "USD", new Dictionary<OrderStatus, int>()));

        public Task<UserAggregates> GetUserAggregatesAsync(DateTimeOffset newSince, CancellationToken ct = default)
            => Task.FromResult(new UserAggregates(0, 0, 0));

        public Task<RequestAggregates> GetRequestAggregatesAsync(CancellationToken ct = default)
            => Task.FromResult(new RequestAggregates(0, new Dictionary<CustomRequestStatus, int>()));

        public Task<CatalogAggregates> GetCatalogAggregatesAsync(CancellationToken ct = default)
            => Task.FromResult(new CatalogAggregates(0, 0, 0));

        public Task<IReadOnlyList<AdminOrderListItemDto>> GetRecentOrdersAsync(int take, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<AdminOrderListItemDto>>([]);

        public Task<IReadOnlyList<AdminCustomRequestListItemDto>> GetRecentRequestsAsync(int take, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<AdminCustomRequestListItemDto>>([]);

        public Task<IReadOnlyList<OrderActivityRow>> GetOrdersSinceAsync(DateTimeOffset since, CancellationToken ct = default)
            => Task.FromResult(OrdersSince);

        public Task<IReadOnlyList<DateTimeOffset>> GetUserSignupsSinceAsync(DateTimeOffset since, CancellationToken ct = default)
            => Task.FromResult(UserSignups);

        public Task<IReadOnlyList<DateTimeOffset>> GetRequestsSinceAsync(DateTimeOffset since, CancellationToken ct = default)
            => Task.FromResult(Requests);
    }
}
