using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace FacilityInspection.Infrastructure.FileStorage;

/// <summary>
/// <see cref="IFileStorageService"/> backed by the local file system, rooted at
/// <c>wwwroot/uploads</c>. Files are organised by module (<c>incidents</c>, <c>inspections</c>,
/// <c>vendor-updates</c>) and given unique names; the original name is preserved in the
/// attachment metadata. Returns a relative storage path (e.g.
/// <c>incidents/2026/07/{guid}.jpg</c>) so records stay portable when the provider is later
/// swapped for Azure Blob Storage (same interface, different impl).
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _options;
    private readonly string _root;

    public LocalFileStorageService(IOptions<FileStorageOptions> options)
    {
        _options = options.Value;
        _root = string.IsNullOrWhiteSpace(_options.BasePath)
            ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")
            : Path.GetFullPath(_options.BasePath);
    }

    public void ValidateFile(string fileName, long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new BadRequestException("A file name is required.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension))
        {
            throw new BadRequestException(
                $"File '{fileName}' has an unsupported type. Allowed: {string.Join(", ", _options.AllowedExtensions)}.");
        }

        if (sizeBytes <= 0)
        {
            throw new BadRequestException($"File '{fileName}' is empty.");
        }

        if (sizeBytes > _options.MaxFileSizeBytes)
        {
            var maxMb = _options.MaxFileSizeBytes / (1024d * 1024d);
            throw new BadRequestException($"File '{fileName}' exceeds the maximum size of {maxMb:0.#} MB.");
        }
    }

    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        string module,
        CancellationToken cancellationToken = default)
    {
        var safeModule = SanitizeSegment(module);
        var now = DateTime.UtcNow;
        var relativeDir = $"{safeModule}/{now:yyyy}/{now:MM}";

        var absoluteDir = Path.Combine(_root, safeModule, now.ToString("yyyy"), now.ToString("MM"));
        Directory.CreateDirectory(absoluteDir);

        var storedName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var absolutePath = Path.Combine(absoluteDir, storedName);

        await using var fileStream = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);

        return $"{relativeDir}/{storedName}";
    }

    public Task<Stream> GetAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = ResolveWithinRoot(storagePath);
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("The requested file does not exist.", storagePath);
        }

        Stream stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = ResolveWithinRoot(storagePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    private static string SanitizeSegment(string module)
    {
        if (string.IsNullOrWhiteSpace(module))
        {
            return "misc";
        }

        var invalid = Path.GetInvalidFileNameChars();
        return new string(module.Where(c => !invalid.Contains(c)).ToArray());
    }

    private string ResolveWithinRoot(string storagePath)
    {
        var relative = storagePath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        var absolutePath = Path.GetFullPath(Path.Combine(_root, relative));

        // Guard against path traversal escaping the storage root.
        if (!absolutePath.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Invalid storage path.");
        }

        return absolutePath;
    }
}
