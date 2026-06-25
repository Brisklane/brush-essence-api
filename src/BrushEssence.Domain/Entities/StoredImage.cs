using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// An uploaded image stored as binary data directly in the database
/// (PostgreSQL <c>bytea</c>). Keeping images in the DB means they are backed up
/// and deployed atomically with the catalogue, at the cost of database size —
/// suitable for a boutique store. Swap to object storage if the catalogue grows.
/// </summary>
public class StoredImage : AuditableEntity
{
    /// <summary>Original file name as uploaded (for download/debugging).</summary>
    public required string FileName { get; set; }

    /// <summary>MIME type, e.g. "image/jpeg", used when serving the bytes.</summary>
    public required string ContentType { get; set; }

    /// <summary>Size in bytes (denormalised so we don't load the blob to read it).</summary>
    public long SizeBytes { get; set; }

    /// <summary>The raw image bytes.</summary>
    public required byte[] Data { get; set; }
}
