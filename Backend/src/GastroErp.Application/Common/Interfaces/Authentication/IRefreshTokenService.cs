namespace GastroErp.Application.Common.Interfaces.Authentication;

/// <summary>
/// واجهة خدمة التوكن المتجدد (Refresh Token Service Interface)
/// </summary>
public interface IRefreshTokenService
{
    string GenerateRefreshToken();
}
