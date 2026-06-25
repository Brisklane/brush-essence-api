using BrushEssence.Api.Authorization;
using BrushEssence.Application.Admin;
using BrushEssence.Application.Common.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Controllers.Admin;

/// <summary>Admin user management: list, view, and enable/disable or re-role accounts.</summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Produces("application/json")]
public sealed class AdminUsersController(
    IAdminUserService userService,
    IValidator<UpdateUserRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> GetAll(
        [FromQuery] AdminUserQuery query,
        CancellationToken cancellationToken)
        => Ok(await userService.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await userService.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDto>> Update(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await userService.UpdateAsync(id, request, cancellationToken));
    }
}
