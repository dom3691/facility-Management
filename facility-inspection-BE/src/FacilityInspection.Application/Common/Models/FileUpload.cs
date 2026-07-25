namespace FacilityInspection.Application.Common.Models;

/// <summary>
/// A framework-agnostic uploaded file. The API maps <c>IFormFile</c> onto this so the
/// Application layer never depends on ASP.NET Core types.
/// </summary>
public record FileUpload(string FileName, string ContentType, byte[] Content)
{
    public long SizeBytes => Content.LongLength;
}
