using System.Security.Cryptography;
using GastroErp.Application.Common.Interfaces.Security;

namespace GastroErp.Infrastructure.Security;

/// <summary>
/// خدمة توليد الأرقام والسلاسل العشوائية (Random Generator)
/// </summary>
public class RandomGenerator : IRandomGenerator
{
    public string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[length];
        var random = new Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }

    public int GenerateRandomNumber(int min, int max)
    {
        return RandomNumberGenerator.GetInt32(min, max);
    }
}
