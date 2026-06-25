using BrushEssence.Api.Authorization;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BrushEssence.Api.Controllers;

/// <summary>Image uploads. Admin-only; returns the stored file's public URL.</summary>
[ApiController]
[Route("api/uploads")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Produces("application/json")]
public sealed class UploadsController(
    IFileStorageService fileStorage,
    IOptions<FileStorageOptions> options) : ControllerBase
{
    private readonly FileStorageOptions _options = options.Value;

    [HttpPost("paintings")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPaintingImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return Problem(detail: "No file was provided.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (file.Length > _options.MaxFileSizeBytes)
        {
            var maxMb = _options.MaxFileSizeBytes / (1024 * 1024);
            return Problem(detail: $"File exceeds the maximum size of {maxMb} MB.", statusCode: StatusCodes.Status400BadRequest);
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_options.AllowedContentTypes.Contains(file.ContentType) ||
            !_options.AllowedExtensions.Contains(extension))
        {
            return Problem(
                detail: "Unsupported file type. Allowed types: JPEG, PNG, WebP.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        await using var stream = file.OpenReadStream();
        var url = await fileStorage.SaveAsync(stream, file.FileName, "paintings", cancellationToken);
        return Ok(new { url });
    }
}
