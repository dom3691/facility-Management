namespace FacilityInspection.Infrastructure.FileStorage;

/// <summary>
/// Configuration for file storage, bound from the "FileStorage" section.
/// </summary>
public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Root folder for the local provider (default <c>wwwroot/uploads</c>). Relative paths
    /// resolve against the app's working directory. Ignored by cloud providers.
    /// </summary>
    public string BasePath { get; init; } = string.Empty;

    /// <summary>Maximum accepted file size in bytes (default 10 MB).</summary>
    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024;

    /// <summary>Allowed file extensions (lower-case, with the leading dot).</summary>
    public string[] AllowedExtensions { get; init; } = { ".jpg", ".jpeg", ".png", ".pdf" };
}
