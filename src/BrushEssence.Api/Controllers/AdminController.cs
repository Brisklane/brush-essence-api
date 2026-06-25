using BrushEssence.Api.Authorization;
using BrushEssence.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Controllers;

/// <summary>
/// Admin-only area. Demonstrates role/policy-based authorization — every action
/// requires the <see cref="AuthorizationPolicies.AdminOnly"/> policy (Admin role).
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Produces("application/json")]
public sealed class AdminController(ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("overview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Overview() => Ok(new
    {
        message = "Welcome to the admin area.",
        email = currentUser.Email,
        roles = currentUser.Roles,
    });
}
