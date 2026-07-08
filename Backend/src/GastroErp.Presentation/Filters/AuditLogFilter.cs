using GastroErp.Application.Common.Interfaces.Logging;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GastroErp.Presentation.Filters;

public class AuditLogFilter : IAsyncActionFilter
{
    private readonly IAuditLogger _auditLogger;

    public AuditLogFilter(IAuditLogger auditLogger)
    {
        _auditLogger = auditLogger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        
        // Only log mutating actions (POST, PUT, DELETE, PATCH)
        if (method == HttpMethods.Get || method == HttpMethods.Options || method == HttpMethods.Head)
        {
            await next();
            return;
        }

        var executedContext = await next();

        // If the request was successful
        if (executedContext.Exception == null && executedContext.Result != null)
        {
            var path = context.HttpContext.Request.Path.Value ?? string.Empty;
            var actionName = context.ActionDescriptor.DisplayName ?? "UnknownAction";
            var userId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var tenantId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;

            // Simplistic mapping. In reality, you'd extract entity and ID from route or response.
            string action = method switch
            {
                "POST" => "Create",
                "PUT" => "Update",
                "DELETE" => "Delete",
                "PATCH" => "Update",
                _ => "Unknown"
            };

            if (path.Contains("/activate", StringComparison.OrdinalIgnoreCase)) action = "Activate";
            if (path.Contains("/deactivate", StringComparison.OrdinalIgnoreCase)) action = "Deactivate";
            if (path.Contains("/approve", StringComparison.OrdinalIgnoreCase)) action = "Approve";
            if (path.Contains("/publish", StringComparison.OrdinalIgnoreCase)) action = "Publish";

            // Fire and forget or await the audit log
            // Usage of IAuditLogger interface from application layer
            _auditLogger.LogAction(
                action: action,
                entityName: actionName, // Placeholder for actual entity name
                entityId: "N/A", // Placeholder for actual ID
                details: new { userId, tenantId, path }
            );
        }
    }
}
