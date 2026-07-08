using GastroErp.Application.Common.Interfaces.Security;

namespace GastroErp.Infrastructure.Security;

/// <summary>
/// تنفيذ مطابقة كلمات المرور باستخدام BCrypt
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
