using GastroErp.Domain.Common;

namespace GastroErp.Domain.Entities.Settings;

/// <summary>
/// Currency — العملة
/// كيان نظامي يحدد العملات المتاحة في النظام.
/// يُستخدم للربط بالفروع، الشركات، وعمليات البيع.
/// </summary>
public sealed class Currency
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? NameAr { get; private set; }
    public string Symbol { get; private set; }
    public byte DecimalPlaces { get; private set; }
    public bool IsActive { get; private set; }

    private Currency()
    {
        Code = string.Empty;
        Name = string.Empty;
        Symbol = string.Empty;
    }

    public Currency(string code, string name, string symbol,
                    string? nameAr = null, byte decimalPlaces = 2)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
            throw new ArgumentException("Currency code must be a 3-letter ISO code.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

        Code = code.ToUpperInvariant();
        Name = name;
        NameAr = nameAr;
        Symbol = symbol;
        DecimalPlaces = decimalPlaces;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

/// <summary>
/// Language — اللغة
/// كيان نظامي يحدد اللغات المدعومة في النظام.
/// </summary>
public sealed class Language
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string NativeName { get; private set; }
    public bool IsRtl { get; private set; }
    public bool IsActive { get; private set; }

    private Language()
    {
        Code = string.Empty;
        Name = string.Empty;
        NativeName = string.Empty;
    }

    public Language(string code, string name, string nativeName, bool isRtl = false)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(nativeName)) throw new ArgumentException("Native name cannot be empty.", nameof(nativeName));

        Code = code.ToLowerInvariant();
        Name = name;
        NativeName = nativeName;
        IsRtl = isRtl;
        IsActive = true;
    }
}

/// <summary>
/// AppTimezone — المنطقة الزمنية
/// كيان نظامي يحدد المناطق الزمنية المتاحة.
/// يستخدم الـ Windows/IANA Timezone ID للتوافق عبر الأنظمة.
/// </summary>
public sealed class AppTimezone
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string SystemId { get; private set; }
    public string DisplayName { get; private set; }
    public string UtcOffset { get; private set; }
    public bool IsActive { get; private set; }

    private AppTimezone()
    {
        SystemId = string.Empty;
        DisplayName = string.Empty;
        UtcOffset = string.Empty;
    }

    public AppTimezone(string systemId, string displayName, string utcOffset)
    {
        if (string.IsNullOrWhiteSpace(systemId)) throw new ArgumentException("SystemId cannot be empty.", nameof(systemId));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("DisplayName cannot be empty.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(utcOffset)) throw new ArgumentException("UtcOffset cannot be empty.", nameof(utcOffset));

        SystemId = systemId;
        DisplayName = displayName;
        UtcOffset = utcOffset;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
}
