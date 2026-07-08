namespace GastroErp.Application.Common.Interfaces.Authentication;

using System.Security.Claims;
using GastroErp.Domain.Entities.Identity;

/// <summary>
/// واجهة توليد Claims (Claims Factory Interface)
/// </summary>
public interface IClaimsFactory
{
    IEnumerable<Claim> CreateClaims(AppUser user, IReadOnlyCollection<string> roleNames);
}
