namespace BrushEssence.Application.Common.Interfaces;

/// <summary>A stored file's bytes plus the metadata needed to serve it.</summary>
public sealed record StoredFile(byte[] Content, string ContentType, string FileName);

/// <summary>
/// Abstraction over storage for uploaded images. The default implementation
/// keeps bytes in the database (PostgreSQL), but it can be swapped for disk or
/// cloud storage (S3 / Azure Blob) without changing callers.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Persists a file and returns its public, relative URL
    /// (e.g. <c>/api/images/{guid}</c>).
    /// </summary>
    Task<string> SaveAsync(
        Stream content,
        string originalFileName,
        string subfolder,
        CancellationToken cancellationToken = default);

    /// <summary>Loads a previously stored file by its public URL, or null if missing.</summary>
    Task<StoredFile?> GetAsync(string publicUrl, CancellationToken cancellationToken = default);

    /// <summary>Deletes a previously stored file by its public URL. No-op if missing.</summary>
    Task DeleteAsync(string? publicUrl, CancellationToken cancellationToken = default);
}
