namespace GastroErp.Application.Common.Interfaces.Logging;

public interface IAuditLogger
{
    void LogCreate(string entityName, string entityId, object newValues);
    void LogUpdate(string entityName, string entityId, object oldValues, object newValues);
    void LogDelete(string entityName, string entityId);
    void LogAction(string action, string entityName, string entityId, object details);
}
