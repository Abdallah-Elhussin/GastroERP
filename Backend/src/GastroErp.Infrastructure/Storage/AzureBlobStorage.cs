using GastroErp.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Storage;

public class AzureBlobStorage : IFileStorage
{
    private readonly ILogger<AzureBlobStorage> _logger;

    public AzureBlobStorage(ILogger<AzureBlobStorage> logger)
    {
        _logger = logger;
    }

    public Task<string> UploadAsync(string fileName, Stream fileStream, string contentType, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading {FileName} to Azure Blob Storage (Skeleton)", fileName);
        return Task.FromResult($"https://azure.blob.core.windows.net/dummy/{fileName}");
    }

    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting {FileUrl} from Azure Blob Storage (Skeleton)", fileUrl);
        return Task.CompletedTask;
    }
}
