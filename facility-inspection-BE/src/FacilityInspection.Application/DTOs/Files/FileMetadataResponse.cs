namespace FacilityInspection.Application.DTOs.Files;

/// <summary>Public attachment metadata (no internal storage path).</summary>
public record FileMetadataResponse
{
    public Guid Id { get; init; }

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long FileSizeBytes { get; init; }

    public string Module { get; init; } = string.Empty;
}
