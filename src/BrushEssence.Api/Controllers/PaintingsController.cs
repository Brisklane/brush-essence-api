using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Application.Paintings;
using BrushEssence.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Controllers;

/// <summary>
/// Sample CRUD-style endpoint that exercises the full stack (repository,
/// unit of work, explicit mapping, FluentValidation) end-to-end.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaintingsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreatePaintingRequest> _createValidator;

    public PaintingsController(
        IUnitOfWork unitOfWork,
        IValidator<CreatePaintingRequest> createValidator)
    {
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PaintingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaintingDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var paintings = await _unitOfWork.Repository<Painting>().ListAsync(cancellationToken);
        return Ok(paintings.ToDtoList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaintingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaintingDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var painting = await _unitOfWork.Repository<Painting>().GetByIdAsync(id, cancellationToken);
        return painting is null ? NotFound() : Ok(painting.ToDto());
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaintingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaintingDto>> Create(
        CreatePaintingRequest request,
        CancellationToken cancellationToken)
    {
        // The global exception handler turns this into an RFC 7807 response.
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var painting = request.ToEntity();

        await _unitOfWork.Repository<Painting>().AddAsync(painting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = painting.Id }, painting.ToDto());
    }
}
