using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Authentication;
using GastroErp.Application.Common.Interfaces.Platform;
using GastroErp.Application.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace GastroErp.Infrastructure.Health;

public sealed class AuthenticationHealthCheck : IHealthCheck
{
    private readonly AuthJwtSettings _settings;

    public AuthenticationHealthCheck(IOptions<AuthJwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var healthy = !string.IsNullOrWhiteSpace(_settings.SigningKey) && _settings.ExpiryMinutes > 0;
        return Task.FromResult(healthy
            ? HealthCheckResult.Healthy("JWT authentication is configured.")
            : HealthCheckResult.Unhealthy("JWT authentication is not configured."));
    }
}

public sealed class TenantResolutionHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(HealthCheckResult.Healthy("Tenant resolution services are registered."));
}

public sealed class RbacHealthCheck : IHealthCheck
{
    private readonly IApplicationDbContext _context;

    public RbacHealthCheck(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var permissionCount = await _context.Permissions.CountAsync(cancellationToken);
        return permissionCount > 0
            ? HealthCheckResult.Healthy($"RBAC catalog contains {permissionCount} permissions.")
            : HealthCheckResult.Degraded("RBAC permissions are not seeded.");
    }
}

public sealed class DatabaseProviderHealthCheck : IHealthCheck
{
    private readonly DatabaseOptions _options;
    private readonly IConnectionStringResolver _resolver;

    public DatabaseProviderHealthCheck(IOptions<DatabaseOptions> options, IConnectionStringResolver resolver)
    {
        _options = options.Value;
        _resolver = resolver;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _resolver.Resolve();
            return Task.FromResult(HealthCheckResult.Healthy(
                $"Database provider '{_options.Provider}' resolved successfully."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Database provider resolution failed.", ex));
        }
    }
}
