namespace BrushEssence.Application.Admin;

/// <summary>Admin dashboard analytics and reporting.</summary>
public interface IDashboardService
{
    /// <summary>Top-line summary metrics for the dashboard stat cards.</summary>
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Daily time-series of sales, new users and new requests over the last
    /// <paramref name="days"/> days (clamped to a sensible range).
    /// </summary>
    Task<ReportDto> GetReportAsync(int days, CancellationToken cancellationToken = default);
}
