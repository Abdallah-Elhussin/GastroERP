using System.Security.Claims;
using GastroErp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GastroErp.Infrastructure.Authentication;

/// <summary>
/// خدمة المستخدم الحالي (Current User Service)
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? Id
    {
        get
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId))
            {
                return userId;
            }
            return null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public string? Name => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public Guid TenantId
    {
        get
        {
            var tenantIdStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue("TenantId");
            return Guid.TryParse(tenantIdStr, out var tenantId) ? tenantId : Guid.Empty;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
