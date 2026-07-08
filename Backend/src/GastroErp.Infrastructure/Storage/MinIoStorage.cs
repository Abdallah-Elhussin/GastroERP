using GastroErp.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Storage;

public class MinIoStorage : IFileStorage
{
    private readonly ILogger<MinIoStorage> _logger;

    public MinIoStorage(ILogger<MinIoStorage> logger)
    {
        _logger = logger;
    }

    public Task<string> UploadAsync(string fileName, Stream fileStream, string contentType, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading {FileName} to MinIO (Skeleton)", fileName);
        return Task.FromResult($"http://minio.local/dummy/{fileName}");
    }

    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting {FileUrl} from MinIO (Skeleton)", fileUrl);
        return Task.CompletedTask;
    }
}
