using GastroErp.Application.Common.Interfaces.Logging;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Logging;

public class SecurityLogger : ISecurityLogger
{
    private readonly ILogger<SecurityLogger> _logger;

    public SecurityLogger(ILogger<SecurityLogger> logger)
    {
        _logger = logger;
    }

    public void LogLoginSuccess(string userId, string ipAddress)
    {
        _logger.LogInformation("SECURITY LOGIN SUCCESS: User {UserId} from IP {IpAddress}", userId, ipAddress);
    }

    public void LogLoginFailed(string username, string ipAddress, string reason)
    {
        _logger.LogWarning("SECURITY LOGIN FAILED: User {Username} from IP {IpAddress}. Reason: {Reason}", username, ipAddress, reason);
    }

    public void LogUnauthorizedAccess(string userId, string resource, string ipAddress)
    {
        _logger.LogWarning("SECURITY UNAUTHORIZED: User {UserId} attempted to access {Resource} from IP {IpAddress}", userId, resource, ipAddress);
    }
}
