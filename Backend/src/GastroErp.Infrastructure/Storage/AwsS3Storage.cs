using GastroErp.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Storage;

public class AwsS3Storage : IFileStorage
{
    private readonly ILogger<AwsS3Storage> _logger;

    public AwsS3Storage(ILogger<AwsS3Storage> logger)
    {
        _logger = logger;
    }

    public Task<string> UploadAsync(string fileName, Stream fileStream, string contentType, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading {FileName} to AWS S3 (Skeleton)", fileName);
        return Task.FromResult($"https://s3.amazonaws.com/dummy/{fileName}");
    }

    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting {FileUrl} from AWS S3 (Skeleton)", fileUrl);
        return Task.CompletedTask;
    }
}
