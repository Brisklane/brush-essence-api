using BrushEssence.Api.Authorization;
using BrushEssence.Application.Common.Models;
using BrushEssence.Application.Paintings;
using BrushEssence.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Controllers;

/// <summary>
/// Painting catalogue. Reads are public; writes require the Admin policy.
/// </summary>
[ApiController]
[Route("api/paintings")]
[Produces("application/json")]
public sealed class PaintingsController(
    IPaintingService paintingService,
    IValidator<CreatePaintingRequest> createValidator,
    IValidator<UpdatePaintingRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PaintingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PaintingDto>>> GetAll(
        [FromQuery] PaintingQuery query,
        CancellationToken cancellationToken)
    {
        // Anonymous/customer callers only ever see the published catalogue;
        // only admins may browse drafts (and filter by IsPublished freely).
        if (!User.IsInRole(Roles.Admin))
        {
            query.IsPublished = true;
        }

        return Ok(await paintingService.GetPagedAsync(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaintingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaintingDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var painting = await paintingService.GetByIdAsync(id, cancellationToken);

        // Hide unpublished paintings from non-admins (don't reveal their existence).
        if (!painting.IsPublished && !User.IsInRole(Roles.Admin))
        {
            return NotFound();
        }

        return Ok(painting);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(PaintingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaintingDto>> Create(
        CreatePaintingRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var dto = await paintingService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PaintingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaintingDto>> Update(
        Guid id,
        UpdatePaintingRequest request,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await paintingService.UpdateAsync(id, request, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await paintingService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
