using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Platform.Services;

public interface IPlatformJobExecutor
{
    Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default);
    Task CleanupRefreshTokensAsync(CancellationToken cancellationToken = default);
    Task CheckInactiveTenantsAsync(CancellationToken cancellationToken = default);
    Task SendTrialExpiryRemindersAsync(CancellationToken cancellationToken = default);
    Task SendSubscriptionExpiryRemindersAsync(CancellationToken cancellationToken = default);
}

public sealed class PlatformJobExecutor : IPlatformJobExecutor
{
    private readonly IAuthSessionService _authSessionService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<PlatformJobExecutor> _logger;

    public PlatformJobExecutor(
        IAuthSessionService authSessionService,
        IApplicationDbContext context,
        ILogger<PlatformJobExecutor> logger)
    {
        _authSessionService = authSessionService;
        _context = context;
        _logger = logger;
    }

    public Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default)
        => _authSessionService.CleanupExpiredAsync(cancellationToken);

    public Task CleanupRefreshTokensAsync(CancellationToken cancellationToken = default)
        => _authSessionService.CleanupExpiredAsync(cancellationToken);

    public async Task CheckInactiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        var suspended = await _context.Tenants
            .Where(t => t.Status == Domain.Enums.TenantStatus.Suspended)
            .CountAsync(cancellationToken);
        _logger.LogInformation("Inactive tenant check completed. Suspended tenants: {Count}", suspended);
    }

    public async Task SendTrialExpiryRemindersAsync(CancellationToken cancellationToken = default)
    {
        var expiring = await _context.Subscriptions
            .Where(s => s.Status == Domain.Enums.SubscriptionStatus.Trial && s.EndDate <= DateTimeOffset.UtcNow.AddDays(3))
            .CountAsync(cancellationToken);
        _logger.LogInformation("Trial expiry reminder scan completed. Expiring trials: {Count}", expiring);
    }

    public async Task SendSubscriptionExpiryRemindersAsync(CancellationToken cancellationToken = default)
    {
        var expiring = await _context.Subscriptions
            .Where(s => s.Status == Domain.Enums.SubscriptionStatus.Active && s.EndDate <= DateTimeOffset.UtcNow.AddDays(7))
            .CountAsync(cancellationToken);
        _logger.LogInformation("Subscription expiry reminder scan completed. Expiring subscriptions: {Count}", expiring);
    }
}
