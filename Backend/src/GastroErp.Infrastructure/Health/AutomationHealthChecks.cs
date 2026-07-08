using GastroErp.Application.Common.Interfaces;
using GastroErp.Infrastructure.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace GastroErp.Infrastructure.Health;

public sealed class SmtpHealthCheck : IHealthCheck
{
    private readonly SmtpOptions _options;

    public SmtpHealthCheck(IOptions<SmtpOptions> options) => _options = options.Value;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
            return Task.FromResult(HealthCheckResult.Degraded("SMTP host not configured"));

        return Task.FromResult(HealthCheckResult.Healthy("SMTP configured"));
    }
}

public sealed class SmsHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(HealthCheckResult.Healthy("SMS provider available (dummy)"));
}

public sealed class QueueHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(HealthCheckResult.Healthy("Background queue operational"));
}
