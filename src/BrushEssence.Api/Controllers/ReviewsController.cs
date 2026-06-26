using BrushEssence.Application.Common.Models;
using BrushEssence.Application.Reviews;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Controllers;

/// <summary>
/// Reviews for a painting. Listing and the rating summary are public; submitting
/// a review (and reading your own) requires authentication. New reviews are held
/// for moderation before they appear publicly.
/// </summary>
[ApiController]
[Route("api/paintings/{paintingId:guid}/reviews")]
[Produces("application/json")]
public sealed class ReviewsController(
    IReviewService reviewService,
    IValidator<CreateReviewRequest> createValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetForPainting(
        Guid paintingId,
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
        => Ok(await reviewService.GetForPaintingAsync(paintingId, query, cancellationToken));

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ReviewSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReviewSummaryDto>> GetSummary(
        Guid paintingId,
        CancellationToken cancellationToken)
        => Ok(await reviewService.GetSummaryAsync(paintingId, cancellationToken));

    [Authorize]
    [HttpGet("mine")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<ReviewDto>> GetMine(
        Guid paintingId,
        CancellationToken cancellationToken)
    {
        var review = await reviewService.GetMyReviewAsync(paintingId, cancellationToken);
        return review is null ? NoContent() : Ok(review);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewDto>> Create(
        Guid paintingId,
        CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var review = await reviewService.CreateAsync(paintingId, request, cancellationToken);
        return CreatedAtAction(nameof(GetMine), new { paintingId }, review);
    }
}
