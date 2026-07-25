namespace FacilityInspection.Application.Features.Files.Common;

/// <summary>Resolved, authorized attachment metadata (includes the internal storage path).</summary>
public record FileAttachmentInfo(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string StoragePath,
    string Module);
