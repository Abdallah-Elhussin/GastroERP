using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GastroErp.Application.Common.Interfaces.Authentication;
using GastroErp.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GastroErp.Infrastructure.Authentication;

/// <summary>
/// خدمة التحقق من التوكن (JWT Token Validator)
/// </summary>
public class JwtTokenValidator : IJwtTokenValidator
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenValidator(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret)),
            ValidateLifetime = false // Here we usually don't validate lifetime if we just want claims, but let's keep it configurable or standard
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
