namespace GastroErp.Application.Common.Interfaces.Authentication;

using System.Security.Claims;

/// <summary>
/// واجهة توليد التوكن (JWT Token Generator)
/// </summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(IEnumerable<Claim> claims);
}
