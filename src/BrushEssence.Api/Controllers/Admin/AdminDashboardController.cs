using BrushEssence.Api.Authorization;
using BrushEssence.Application.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Controllers.Admin;

/// <summary>
/// Dashboard analytics and reporting for admins. <c>summary</c> powers the stat
/// cards; <c>report</c> supplies the daily time-series for the charts.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Produces("application/json")]
public sealed class AdminDashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("dashboard/summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken cancellationToken)
        => Ok(await dashboardService.GetSummaryAsync(cancellationToken));

    [HttpGet("reports")]
    [ProducesResponseType(typeof(ReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportDto>> GetReport(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
        => Ok(await dashboardService.GetReportAsync(days, cancellationToken));
}
