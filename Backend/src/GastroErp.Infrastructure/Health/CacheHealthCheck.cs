using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GastroErp.Infrastructure.Health;

public class CacheHealthCheck : IHealthCheck
{
    private readonly IMemoryCache _memoryCache;

    public CacheHealthCheck(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _memoryCache.TryGetValue("HealthCheckKey", out _);
            return Task.FromResult(HealthCheckResult.Healthy("Cache is accessible."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Cache is failing.", ex));
        }
    }
}
