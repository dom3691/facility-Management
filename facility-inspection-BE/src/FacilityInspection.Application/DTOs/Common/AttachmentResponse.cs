namespace FacilityInspection.Application.DTOs.Common;

/// <summary>A stored file attachment, shared by incident and inspection responses.</summary>
public record AttachmentResponse
{
    public Guid Id { get; init; }

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long FileSizeBytes { get; init; }

    public string StoragePath { get; init; } = string.Empty;
}
