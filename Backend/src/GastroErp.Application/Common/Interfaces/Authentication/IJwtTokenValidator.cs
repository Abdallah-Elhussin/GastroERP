namespace GastroErp.Application.Common.Interfaces.Authentication;

using System.Security.Claims;

/// <summary>
/// واجهة التحقق من التوكن (JWT Token Validator)
/// </summary>
public interface IJwtTokenValidator
{
    ClaimsPrincipal? GetPrincipalFromToken(string token);
}
