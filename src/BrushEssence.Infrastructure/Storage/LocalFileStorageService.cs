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

    public async Task<StoredFile?> GetAsync(string publicUrl, CancellationToken cancellationToken = default)
    {
        if (!TryResolvePhysicalPath(publicUrl, out var physicalPath) || !File.Exists(physicalPath))
        {
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken);
        var contentType = Path.GetExtension(physicalPath).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream",
        };
        return new StoredFile(bytes, contentType, Path.GetFileName(physicalPath));
    }

    public Task DeleteAsync(string? publicUrl, CancellationToken cancellationToken = default)
    {
        if (!TryResolvePhysicalPath(publicUrl, out var physicalPath))
        {
            return Task.CompletedTask;
        }

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

    /// <summary>Maps a public URL we own back to its on-disk path.</summary>
    private bool TryResolvePhysicalPath(string? publicUrl, out string physicalPath)
    {
        physicalPath = string.Empty;

        var prefix = _options.PublicPathPrefix.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(publicUrl) ||
            !publicUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            // Not a file we own (e.g. an external URL) — ignore.
            return false;
        }

        var relative = publicUrl[prefix.Length..].TrimStart('/');
        physicalPath = Path.Combine(
            _options.PhysicalRootPath,
            relative.Replace('/', Path.DirectorySeparatorChar));
        return true;
    }
}
