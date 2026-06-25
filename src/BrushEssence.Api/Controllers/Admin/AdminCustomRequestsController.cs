using BrushEssence.Api.Authorization;
using BrushEssence.Application.Admin;
using BrushEssence.Application.Common.Models;
using BrushEssence.Application.CustomRequests;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Controllers.Admin;

/// <summary>Admin custom-request management: review all requests and advance status.</summary>
[ApiController]
[Route("api/admin/custom-requests")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Produces("application/json")]
public sealed class AdminCustomRequestsController(
    ICustomRequestService customRequestService,
    IValidator<UpdateCustomRequestStatusRequest> statusValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminCustomRequestListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminCustomRequestListItemDto>>> GetAll(
        [FromQuery] AdminCustomRequestQuery query,
        CancellationToken cancellationToken)
        => Ok(await customRequestService.GetAllAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomRequestDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await customRequestService.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(CustomRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomRequestDto>> UpdateStatus(
        Guid id,
        UpdateCustomRequestStatusRequest request,
        CancellationToken cancellationToken)
    {
        await statusValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await customRequestService.UpdateStatusAsync(id, request, cancellationToken));
    }
}
