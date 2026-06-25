using BrushEssence.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace BrushEssence.Infrastructure.Storage;

/// <summary>
/// Stores uploaded files on the local filesystem under a configured root and
/// returns relative URLs served by the static-files middleware.
/// </summary>
public sealed class LocalFileStorageService(IOptions<FileStorageOptions> options) : IFileStorageService
{
    private readonly FileStorageOptions _options = options.Value;

    public async Task<string> SaveAsync(
        Stream content,
        string originalFileName,
        string subfolder,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";

        var targetDirectory = Path.Combine(_options.PhysicalRootPath, subfolder);
        Directory.CreateDirectory(targetDirectory);

        var physicalPath = Path.Combine(targetDirectory, fileName);
        await using (var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        var prefix = _options.PublicPathPrefix.TrimEnd('/');
        return $"{prefix}/{subfolder}/{fileName}".Replace('\\', '/');
    }

    public Task DeleteAsync(string? publicUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
        {
            return Task.CompletedTask;
        }

        var prefix = _options.PublicPathPrefix.TrimEnd('/');
        if (!publicUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            // Not a file we own (e.g. an external URL) — ignore.
            return Task.CompletedTask;
        }

        var relative = publicUrl[prefix.Length..].TrimStart('/');
        var physicalPath = Path.Combine(
            _options.PhysicalRootPath,
            relative.Replace('/', Path.DirectorySeparatorChar));

        try
        {
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }
        catch
        {
            // Best effort: a failed cleanup should not fail the operation.
        }

        return Task.CompletedTask;
    }
}
