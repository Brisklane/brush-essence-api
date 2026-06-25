namespace BrushEssence.Application.Common.Interfaces;

/// <summary>
/// Abstraction over file storage for uploaded images. The default
/// implementation writes to local disk and serves files statically, but it can
/// be swapped for cloud storage (S3 / Azure Blob) without changing callers.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Persists a file and returns its public, relative URL
    /// (e.g. <c>/uploads/paintings/{guid}.jpg</c>).
    /// </summary>
    Task<string> SaveAsync(
        Stream content,
        string originalFileName,
        string subfolder,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a previously stored file by its public URL. No-op if missing.</summary>
    Task DeleteAsync(string? publicUrl, CancellationToken cancellationToken = default);
}
