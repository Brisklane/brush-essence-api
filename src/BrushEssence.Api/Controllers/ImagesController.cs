using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Controllers;

/// <summary>
/// Serves uploaded images stored in the database. Public, since the storefront
/// needs them. Stored images are immutable (a new upload gets a new id), so
/// responses are cached aggressively.
/// </summary>
[ApiController]
[Route("api/images")]
[AllowAnonymous]
public sealed class ImagesController(IFileStorageService fileStorage) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var file = await fileStorage.GetAsync(
            $"{DatabaseFileStorageService.UrlPrefix}{id}",
            cancellationToken);

        if (file is null)
        {
            return NotFound();
        }

        // Inline (no download filename) so the bytes render directly in an <img>.
        Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        return File(file.Content, file.ContentType, enableRangeProcessing: true);
    }
}
