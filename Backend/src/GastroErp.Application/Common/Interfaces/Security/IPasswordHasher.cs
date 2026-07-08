namespace GastroErp.Application.Common.Interfaces.Security;

/// <summary>
/// واجهة تشفير ومطابقة كلمات المرور (Password Hashing Interface)
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}
