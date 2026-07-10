using System.Security.Cryptography;
using System.Text;

namespace GastroErp.Application.Features.Onboarding;

public static class RestaurantOnboardingCatalog
{
    public static Guid StableId(string seed)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes($"gastroerp:onboarding:{seed}"));
        return new Guid(hash);
    }

    public static string ResolveWindowsTimezone(string timezone) =>
        timezone switch
        {
            "Asia/Riyadh" => "Arab Standard Time",
            "Asia/Dubai" => "Arabian Standard Time",
            "Asia/Kuwait" => "Arab Standard Time",
            "Asia/Qatar" => "Arab Standard Time",
            "Asia/Muscat" => "Arabian Standard Time",
            "Asia/Bahrain" => "Arab Standard Time",
            "Africa/Cairo" => "Egypt Standard Time",
            "Africa/Khartoum" => "Sudan Standard Time",
            _ when timezone.Contains("Standard Time", StringComparison.Ordinal) => timezone,
            _ => "Arab Standard Time"
        };

    public static (string CountryAr, string CountryEn) GetCountryNames(string countryCode) =>
        countryCode.ToUpperInvariant() switch
        {
            "SA" => ("المملكة العربية السعودية", "Saudi Arabia"),
            "AE" => ("الإمارات العربية المتحدة", "United Arab Emirates"),
            "KW" => ("الكويت", "Kuwait"),
            "QA" => ("قطر", "Qatar"),
            "OM" => ("عُمان", "Oman"),
            "BH" => ("البحرين", "Bahrain"),
            "EG" => ("مصر", "Egypt"),
            "SD" => ("السودان", "Sudan"),
            _ => (countryCode, countryCode)
        };

    public static decimal GetVatRate(string countryCode) =>
        countryCode.ToUpperInvariant() switch
        {
            "SA" => 15m,
            "AE" => 5m,
            "KW" => 0m,
            "QA" => 0m,
            "OM" => 5m,
            "BH" => 10m,
            "EG" => 14m,
            "SD" => 17m,
            _ => 0m
        };

    public static string GetVatCode(string countryCode) =>
        countryCode.ToUpperInvariant() switch
        {
            "SA" => "VAT15",
            "AE" => "VAT5",
            "BH" => "VAT10",
            "OM" => "VAT5",
            "EG" => "VAT14",
            "SD" => "VAT17",
            _ => "VAT0"
        };

    public static string GetVatNameAr(string countryCode) =>
        $"ضريبة القيمة المضافة {GetVatRate(countryCode):0.#}%";

    public static string GetVatNameEn(string countryCode) =>
        $"VAT {GetVatRate(countryCode):0.#}%";

    public static string NormalizePhone(string phone, string countryCode)
    {
        var trimmed = phone.Trim();
        if (trimmed.StartsWith('+'))
        {
            return trimmed;
        }

        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        var prefix = countryCode.ToUpperInvariant() switch
        {
            "SA" => "+966",
            "AE" => "+971",
            "KW" => "+965",
            "QA" => "+974",
            "OM" => "+968",
            "BH" => "+973",
            "EG" => "+20",
            "SD" => "+249",
            _ => "+966"
        };

        if (digits.StartsWith('0'))
        {
            digits = digits[1..];
        }

        return $"{prefix}{digits}";
    }
}
