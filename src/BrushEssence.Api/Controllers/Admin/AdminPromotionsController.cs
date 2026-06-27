using BrushEssence.Api.Authorization;
using BrushEssence.Api.Extensions;
using BrushEssence.Application.Promotions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace BrushEssence.Api.Controllers.Admin;

/// <summary>
/// Admin discount promotions. Writes evict the catalogue cache so storefront
/// prices update promptly.
/// </summary>
[ApiController]
[Route("api/admin/promotions")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Produces("application/json")]
public sealed class AdminPromotionsController(
    IPromotionService promotionService,
    IValidator<SavePromotionRequest> validator,
    IOutputCacheStore outputCache) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PromotionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PromotionDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await promotionService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromotionDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await promotionService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PromotionDto>> Create(
        SavePromotionRequest request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var dto = await promotionService.CreateAsync(request, cancellationToken);
        await outputCache.EvictByTagAsync(CachePolicies.CatalogTag, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromotionDto>> Update(
        Guid id,
        SavePromotionRequest request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var dto = await promotionService.UpdateAsync(id, request, cancellationToken);
        await outputCache.EvictByTagAsync(CachePolicies.CatalogTag, cancellationToken);
        return Ok(dto);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await promotionService.DeleteAsync(id, cancellationToken);
        await outputCache.EvictByTagAsync(CachePolicies.CatalogTag, cancellationToken);
        return NoContent();
    }
}
