namespace GastroErp.Application.Common.Interfaces.Logging;

public interface ISecurityLogger
{
    void LogLoginSuccess(string userId, string ipAddress);
    void LogLoginFailed(string username, string ipAddress, string reason);
    void LogUnauthorizedAccess(string userId, string resource, string ipAddress);
}
