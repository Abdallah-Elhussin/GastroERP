namespace GastroErp.Application.Common.Interfaces.Security;

/// <summary>
/// واجهة توليد الأرقام أو السلاسل العشوائية (Random Generator Interface)
/// </summary>
public interface IRandomGenerator
{
    string GenerateRandomString(int length);
    int GenerateRandomNumber(int min, int max);
}
