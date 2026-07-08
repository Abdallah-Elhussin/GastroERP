using GastroErp.Application.Common.Interfaces.Logging;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GastroErp.Infrastructure.Logging;

public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(ILogger<AuditLogger> logger)
    {
        _logger = logger;
    }

    public void LogCreate(string entityName, string entityId, object newValues)
    {
        var serialized = JsonSerializer.Serialize(newValues);
        _logger.LogInformation("AUDIT CREATE: {EntityName} ({EntityId}) - {Values}", entityName, entityId, serialized);
    }

    public void LogUpdate(string entityName, string entityId, object oldValues, object newValues)
    {
        var oldSer = JsonSerializer.Serialize(oldValues);
        var newSer = JsonSerializer.Serialize(newValues);
        _logger.LogInformation("AUDIT UPDATE: {EntityName} ({EntityId}) - Old: {OldValues}, New: {NewValues}", entityName, entityId, oldSer, newSer);
    }

    public void LogDelete(string entityName, string entityId)
    {
        _logger.LogInformation("AUDIT DELETE: {EntityName} ({EntityId})", entityName, entityId);
    }

    public void LogAction(string action, string entityName, string entityId, object details)
    {
        var serialized = JsonSerializer.Serialize(details);
        _logger.LogInformation("AUDIT {Action}: {EntityName} ({EntityId}) - {Details}", action.ToUpperInvariant(), entityName, entityId, serialized);
    }
}
