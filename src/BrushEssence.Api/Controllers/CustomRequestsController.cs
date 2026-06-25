using BrushEssence.Application.CustomRequests;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Controllers;

/// <summary>
/// Customer custom painting requests. Every endpoint requires authentication and
/// only ever exposes the caller's own requests. Admin review/status management
/// lives under <c>api/admin/custom-requests</c>.
/// </summary>
[ApiController]
[Route("api/custom-requests")]
[Produces("application/json")]
[Authorize]
public sealed class CustomRequestsController(
    ICustomRequestService customRequestService,
    IValidator<CreateCustomRequestRequest> createValidator,
    IValidator<UpdateCustomRequestRequest> updateValidator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CustomRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomRequestDto>> Create(
        CreateCustomRequestRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var dto = await customRequestService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomRequestSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomRequestSummaryDto>>> GetMyRequests(
        CancellationToken cancellationToken)
        => Ok(await customRequestService.GetMyRequestsAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomRequestDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await customRequestService.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomRequestDto>> Update(
        Guid id,
        UpdateCustomRequestRequest request,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await customRequestService.UpdateAsync(id, request, cancellationToken));
    }
}
