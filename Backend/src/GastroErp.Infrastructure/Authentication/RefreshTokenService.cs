using System.Security.Claims;
using System.Security.Cryptography;
using GastroErp.Application.Common.Interfaces.Authentication;
using GastroErp.Domain.Entities.Organization;

namespace GastroErp.Infrastructure.Authentication;

/// <summary>
/// خدمة التوكن المتجدد (Refresh Token Service)
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
