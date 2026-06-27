using BrushEssence.Api.Extensions;
using BrushEssence.Application.Promotions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace BrushEssence.Api.Controllers;

/// <summary>
/// Public, read-only promotions for the storefront. Output-cached and tagged
/// with the catalogue tag, so admin promotion writes refresh it.
/// </summary>
[ApiController]
[Route("api/promotions")]
[Produces("application/json")]
public sealed class PromotionsController(IPromotionService promotionService) : ControllerBase
{
    /// <summary>Currently-live promotions (for the home-page sale banner).</summary>
    [HttpGet("active")]
    [OutputCache(PolicyName = CachePolicies.Catalog)]
    [ProducesResponseType(typeof(IReadOnlyList<ActivePromotionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ActivePromotionDto>>> GetActive(
        CancellationToken cancellationToken)
        => Ok(await promotionService.GetActiveAsync(cancellationToken));
}
