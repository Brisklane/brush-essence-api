namespace BrushEssence.Infrastructure.Storage;

/// <summary>Settings for local file storage of uploaded images.</summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Absolute physical root files are written to (set at startup).</summary>
    public string PhysicalRootPath { get; set; } = string.Empty;

    /// <summary>Public URL prefix that maps to <see cref="PhysicalRootPath"/>.</summary>
    public string PublicPathPrefix { get; set; } = "/uploads";

    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    public string[] AllowedContentTypes { get; set; } = ["image/jpeg", "image/png", "image/webp"];

    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
}
