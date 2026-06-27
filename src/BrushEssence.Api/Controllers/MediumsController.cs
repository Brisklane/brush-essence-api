using BrushEssence.Api.Authorization;
using BrushEssence.Api.Extensions;
using BrushEssence.Application.Mediums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace BrushEssence.Api.Controllers;

/// <summary>
/// Art mediums (e.g. "Oil on canvas"). Reads are public (output-cached); writes
/// require the Admin policy and evict the catalogue cache.
/// </summary>
[ApiController]
[Route("api/mediums")]
[Produces("application/json")]
public sealed class MediumsController(
    IMediumService mediumService,
    IValidator<CreateMediumRequest> createValidator,
    IValidator<UpdateMediumRequest> updateValidator,
    IOutputCacheStore outputCache) : ControllerBase
{
    [HttpGet]
    [OutputCache(PolicyName = CachePolicies.Catalog)]
    [ProducesResponseType(typeof(IReadOnlyList<MediumDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MediumDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await mediumService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [OutputCache(PolicyName = CachePolicies.Catalog)]
    [ProducesResponseType(typeof(MediumDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediumDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await mediumService.GetByIdAsync(id, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(MediumDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MediumDto>> Create(
        CreateMediumRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var dto = await mediumService.CreateAsync(request, cancellationToken);
        await outputCache.EvictByTagAsync(CachePolicies.CatalogTag, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MediumDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MediumDto>> Update(
        Guid id,
        UpdateMediumRequest request,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var dto = await mediumService.UpdateAsync(id, request, cancellationToken);
        await outputCache.EvictByTagAsync(CachePolicies.CatalogTag, cancellationToken);
        return Ok(dto);
    }

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediumService.DeleteAsync(id, cancellationToken);
        await outputCache.EvictByTagAsync(CachePolicies.CatalogTag, cancellationToken);
        return NoContent();
    }
}
