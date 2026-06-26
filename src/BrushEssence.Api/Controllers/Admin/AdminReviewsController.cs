using BrushEssence.Api.Authorization;
using BrushEssence.Application.Common.Models;
using BrushEssence.Application.Reviews;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Controllers.Admin;

/// <summary>Admin review moderation: list, approve/reject, edit, and remove reviews.</summary>
[ApiController]
[Route("api/admin/reviews")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Produces("application/json")]
public sealed class AdminReviewsController(
    IReviewService reviewService,
    IValidator<UpdateReviewStatusRequest> statusValidator,
    IValidator<AdminUpdateReviewRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminReviewListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminReviewListItemDto>>> GetAll(
        [FromQuery] AdminReviewQuery query,
        CancellationToken cancellationToken)
        => Ok(await reviewService.GetAllAsync(query, cancellationToken));

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(AdminReviewListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminReviewListItemDto>> UpdateStatus(
        Guid id,
        UpdateReviewStatusRequest request,
        CancellationToken cancellationToken)
    {
        await statusValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await reviewService.UpdateStatusAsync(id, request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AdminReviewListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminReviewListItemDto>> Update(
        Guid id,
        AdminUpdateReviewRequest request,
        CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await reviewService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await reviewService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
