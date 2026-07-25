namespace FacilityInspection.Application.Common.Interfaces;

/// <summary>
/// Abstraction over file/blob storage for attachments (incident photos, inspection
/// evidence, vendor updates). Implemented in Infrastructure.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Validates a file against the configured constraints (allowed extensions, max size).
    /// Throws <see cref="Exceptions.BadRequestException"/> when the file is not acceptable.
    /// Call before <see cref="SaveAsync"/> to reject a batch without persisting any of it.
    /// </summary>
    void ValidateFile(string fileName, long sizeBytes);

    /// <summary>
    /// Stores a file under the given <c>module</c> folder (see <c>FileModules</c>) and returns
    /// its relative storage path.
    /// </summary>
    Task<string> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        string module,
        CancellationToken cancellationToken = default);

    Task<Stream> GetAsync(string storagePath, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
