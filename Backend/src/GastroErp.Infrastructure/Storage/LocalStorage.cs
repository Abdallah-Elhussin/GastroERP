using GastroErp.Application.Common.Interfaces;
using GastroErp.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GastroErp.Infrastructure.Storage;

/// <summary>
/// خدمة التخزين المحلي (Local File Storage)
/// </summary>
public class LocalStorage : IFileStorage
{
    private readonly StorageOptions _options;
    private readonly ILogger<LocalStorage> _logger;

    public LocalStorage(IOptions<StorageOptions> options, ILogger<LocalStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> UploadAsync(string fileName, Stream fileStream, string contentType, CancellationToken cancellationToken = default)
    {
        var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _options.BasePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var fullPath = Path.Combine(directoryPath, uniqueFileName);

        using (var fileStreamOutput = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);
        }

        _logger.LogInformation("File uploaded to local storage: {FullPath}", fullPath);

        // Return relative path or URL
        return Path.Combine(_options.BasePath, uniqueFileName).Replace("\\", "/");
    }

    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileUrl.Replace("/", "\\"));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("File deleted from local storage: {FullPath}", fullPath);
        }
        else
        {
            _logger.LogWarning("File not found for deletion: {FullPath}", fullPath);
        }

        return Task.CompletedTask;
    }
}
