using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A reference image attached to a <see cref="CustomRequest"/>. Only the stored
/// file's public URL is kept here; the bytes live in the file storage service.
/// </summary>
public class CustomRequestImage : AuditableEntity
{
    public Guid CustomRequestId { get; set; }

    /// <summary>Public URL of the stored image (from <c>IFileStorageService</c>).</summary>
    public required string Url { get; set; }

    /// <summary>Original file name, kept for display/accessibility.</summary>
    public string? FileName { get; set; }

    // Navigation property
    public CustomRequest? CustomRequest { get; set; }
}
