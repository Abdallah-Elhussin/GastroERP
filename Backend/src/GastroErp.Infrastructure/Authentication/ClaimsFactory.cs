using System.Security.Claims;
using GastroErp.Application.Common.Interfaces.Authentication;
using GastroErp.Domain.Entities.Identity;

namespace GastroErp.Infrastructure.Authentication;

/// <summary>
/// مصنع الـ Claims
/// </summary>
public class ClaimsFactory : IClaimsFactory
{
    public IEnumerable<Claim> CreateClaims(AppUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("TenantId", user.TenantId.ToString())
        };

        // يمكن إضافة Roles وغيرها هنا
        return claims;
    }
}
