namespace BrushEssence.Application.Admin;

/// <summary>Top-line metrics for the admin dashboard's stat cards.</summary>
public class DashboardSummaryDto
{
    // Revenue
    public decimal TotalRevenue { get; set; }
    public string Currency { get; set; } = "PKR";

    // Orders
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public Dictionary<string, int> OrdersByStatus { get; set; } = [];

    // Users
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsersLast30Days { get; set; }

    // Custom requests
    public int TotalRequests { get; set; }
    public int OpenRequests { get; set; }
    public Dictionary<string, int> RequestsByStatus { get; set; } = [];

    // Catalogue
    public int TotalPaintings { get; set; }
    public int PublishedPaintings { get; set; }
    public int OutOfStockPaintings { get; set; }

    // Recent activity
    public IReadOnlyList<AdminOrderListItemDto> RecentOrders { get; set; } = [];
    public IReadOnlyList<AdminCustomRequestListItemDto> RecentRequests { get; set; } = [];
}

/// <summary>A single day's aggregated activity for the reporting charts.</summary>
public class ReportPointDto
{
    /// <summary>Calendar day (UTC), ISO-8601 date.</summary>
    public DateOnly Date { get; set; }
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
    public int NewUsers { get; set; }
    public int NewRequests { get; set; }
}

/// <summary>Aggregated time-series report (sales, users, requests) over a window.</summary>
public class ReportDto
{
    public int Days { get; set; }
    public string Currency { get; set; } = "PKR";
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalNewUsers { get; set; }
    public int TotalNewRequests { get; set; }
    public IReadOnlyList<ReportPointDto> Points { get; set; } = [];
}
