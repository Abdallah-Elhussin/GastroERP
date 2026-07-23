using System.Security.Claims;
using GastroErp.Application.Common.Interfaces.Authentication;
using GastroErp.Domain.Entities.Identity;

namespace GastroErp.Infrastructure.Authentication;

/// <summary>
/// مصنع الـ Claims
/// </summary>
public class ClaimsFactory : IClaimsFactory
{
    public IEnumerable<Claim> CreateClaims(
        AppUser user,
        IReadOnlyCollection<string> roleNames,
        IReadOnlyCollection<string>? permissionNames = null,
        Guid? defaultBranchId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("TenantId", user.TenantId.ToString())
        };

        if (defaultBranchId.HasValue && defaultBranchId.Value != Guid.Empty)
            claims.Add(new Claim("BranchId", defaultBranchId.Value.ToString()));

        foreach (var role in roleNames)
            claims.Add(new Claim(ClaimTypes.Role, role));

        if (permissionNames is { Count: > 0 })
        {
            foreach (var permission in permissionNames.Distinct(StringComparer.OrdinalIgnoreCase))
                claims.Add(new Claim("Permission", permission));
        }

        return claims;
    }
}
