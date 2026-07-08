using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GastroErp.Application.Common.Interfaces.Authentication;
using GastroErp.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GastroErp.Infrastructure.Authentication;

/// <summary>
/// خدمة توليد التوكن (JWT Token Service)
/// </summary>
public class JwtTokenService : IJwtTokenGenerator
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string GenerateToken(IEnumerable<Claim> claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSigningKey()));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: System.DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GetSigningKey()
    {
        if (!string.IsNullOrWhiteSpace(_jwtOptions.Secret))
        {
            return _jwtOptions.Secret;
        }

        if (!string.IsNullOrWhiteSpace(_jwtOptions.Key))
        {
            return _jwtOptions.Key;
        }

        return "super-secret-key-that-should-be-very-long";
    }
}
