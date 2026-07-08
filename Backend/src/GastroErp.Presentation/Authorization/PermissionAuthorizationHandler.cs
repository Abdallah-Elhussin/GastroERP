using Microsoft.AspNetCore.Authorization;

namespace GastroErp.Presentation.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null)
        {
            return Task.CompletedTask;
        }

        // 1. Check if user has the specific permission claim
        var permissions = context.User.Claims.Where(x => x.Type == "Permission" || x.Type == "permissions").Select(x => x.Value);
        
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 2. You could also implement Role-to-Permission mapping logic here if permissions aren't directly in JWT.
        
        return Task.CompletedTask;
    }
}
