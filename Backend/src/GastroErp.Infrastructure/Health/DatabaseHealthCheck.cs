using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GastroErp.Infrastructure.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual DB check (e.g. CanConnectAsync on DbContext)
        var isHealthy = true;

        if (isHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Database is responsive."));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy("Database is not responsive."));
    }
}
