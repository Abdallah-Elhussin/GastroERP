using System.Text.RegularExpressions;

namespace GastroErp.Domain.ValueObjects;

/// <summary>
/// VatNumber — رقم ضريبة القيمة المضافة (Value Object)
/// <para>
/// يمثّل رقم تسجيل ضريبة القيمة المضافة وفق معايير هيئة الزكاة والضريبة والجمارك (ZATCA).
/// Represents a VAT registration number compliant with Saudi ZATCA standards.
/// </para>
/// <para>
/// قواعد التحقق / Validation Rules:
/// - 15 رقماً بالضبط / Exactly 15 digits
/// - يبدأ بالرقم 3 / Starts with digit '3'
/// - ينتهي بالرقم 3 / Ends with digit '3'
/// - جميع المحارف أرقام فقط / All characters must be digits
/// </para>
/// </summary>
public sealed record VatNumber
{
    private static readonly Regex ZatcaPattern = new(@"^3[0-9]{13}3$", RegexOptions.Compiled);

    /// <summary>القيمة الخام للرقم الضريبي / Raw VAT number value.</summary>
    public string Value { get; }

    /// <summary>
    /// المُنشئ الخاص — مخصص للـ EF Core وإنشاء النسخة عبر Factory method فقط.
    /// Private constructor — reserved for EF Core and internal factory use only.
    /// </summary>
    private VatNumber(string value) => Value = value;

    /// <summary>
    /// أنشئ كائن VatNumber بعد التحقق من صيغة ZATCA.
    /// Creates a validated VatNumber according to ZATCA format.
    /// </summary>
    /// <param name="value">رقم ضريبة القيمة المضافة / VAT number string.</param>
    /// <exception cref="ArgumentException">
    /// يُرفع عندما تكون القيمة فارغة أو لا تطابق معيار ZATCA.
    /// Thrown when value is empty or does not match ZATCA standard.
    /// </exception>
    public static VatNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "رقم ضريبة القيمة المضافة لا يمكن أن يكون فارغاً. / VAT number cannot be empty.",
                nameof(value));

        if (!ZatcaPattern.IsMatch(value))
            throw new ArgumentException(
                "رقم ضريبة القيمة المضافة غير صحيح. يجب أن يكون 15 رقماً يبدأ وينتهي بالرقم 3 (ZATCA). " +
                "/ Invalid Saudi VAT number. Must be 15 digits, starting and ending with '3' (ZATCA).",
                nameof(value));

        return new VatNumber(value);
    }

    /// <summary>
    /// إنشاء نسخة فارغة (Null Object Pattern) لتجنب null checks.
    /// Creates a nullable-safe empty instance (for optional VAT contexts).
    /// </summary>
    public static VatNumber? TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Create(value);
    }

    public override string ToString() => Value;
}
