using System.Security.Claims;
using GastroErp.Application.Common.Interfaces.Authentication;
using GastroErp.Domain.Entities.Identity;

namespace GastroErp.Infrastructure.Authentication;

/// <summary>
/// مصنع الـ Claims
/// </summary>
public class ClaimsFactory : IClaimsFactory
{
    public IEnumerable<Claim> CreateClaims(AppUser user, IReadOnlyCollection<string> roleNames)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("TenantId", user.TenantId.ToString())
        };

        foreach (var role in roleNames)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }
}
