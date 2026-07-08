using Microsoft.Extensions.Diagnostics.HealthChecks;
using GastroErp.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace GastroErp.Infrastructure.Health;

public class StorageHealthCheck : IHealthCheck
{
    private readonly StorageOptions _options;

    public StorageHealthCheck(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _options.BasePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var testFile = Path.Combine(directoryPath, "health_check.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);

            return Task.FromResult(HealthCheckResult.Healthy("Storage is writable."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Storage write failed.", ex));
        }
    }
}
