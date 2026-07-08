namespace GastroErp.Application.Common.Options;

public class AuthJwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "GastroErp";
    public string Audience { get; set; } = "GastroErpClient";
    public int ExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;

    public string SigningKey =>
        !string.IsNullOrWhiteSpace(Secret) ? Secret :
        !string.IsNullOrWhiteSpace(Key) ? Key :
        "super-secret-key-that-should-be-very-long";
}
