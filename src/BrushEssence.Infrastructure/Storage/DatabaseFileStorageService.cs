using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;
using BrushEssence.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Storage;

/// <summary>
/// Stores uploaded images as binary rows in PostgreSQL and serves them back
/// through <c>GET /api/images/{id}</c>. The returned URL is relative so it works
/// across environments.
/// </summary>
public sealed class DatabaseFileStorageService(ApplicationDbContext context) : IFileStorageService
{
    /// <summary>Public route prefix that maps to a stored image row.</summary>
    public const string UrlPrefix = "/api/images/";

    public async Task<string> SaveAsync(
        Stream content,
        string originalFileName,
        string subfolder,
        CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        var image = new StoredImage
        {
            FileName = Path.GetFileName(originalFileName),
            ContentType = ResolveContentType(originalFileName),
            SizeBytes = bytes.LongLength,
            Data = bytes,
        };

        context.Images.Add(image);
        await context.SaveChangesAsync(cancellationToken);

        return $"{UrlPrefix}{image.Id}";
    }

    public async Task<StoredFile?> GetAsync(string publicUrl, CancellationToken cancellationToken = default)
    {
        if (!TryParseId(publicUrl, out var id))
        {
            return null;
        }

        return await context.Images
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new StoredFile(i.Data, i.ContentType, i.FileName))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task DeleteAsync(string? publicUrl, CancellationToken cancellationToken = default)
    {
        if (!TryParseId(publicUrl, out var id))
        {
            // External URL or legacy disk path — nothing for us to delete.
            return;
        }

        // ExecuteDelete avoids loading the (potentially large) blob just to remove it.
        await context.Images
            .Where(i => i.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>Extracts the image id from a "/api/images/{id}" URL.</summary>
    public static bool TryParseId(string? publicUrl, out Guid id)
    {
        id = Guid.Empty;
        if (string.IsNullOrWhiteSpace(publicUrl))
        {
            return false;
        }

        var index = publicUrl.IndexOf(UrlPrefix, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return false;
        }

        var segment = publicUrl[(index + UrlPrefix.Length)..].Trim('/');
        return Guid.TryParse(segment, out id);
    }

    private static string ResolveContentType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };
}
