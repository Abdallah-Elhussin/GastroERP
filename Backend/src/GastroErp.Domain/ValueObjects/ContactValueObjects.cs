using System.Text.RegularExpressions;

namespace GastroErp.Domain.ValueObjects;

/// <summary>
/// PhoneNumber — رقم الهاتف (Value Object)
/// <para>
/// يمثّل رقم هاتف دولي معتمد بصيغة E.164.
/// Represents an internationally validated phone number in E.164 format.
/// </para>
/// <para>
/// الصيغة المقبولة / Accepted format: +[كود الدولة][الرقم] مثال: +966501234567
/// </para>
/// </summary>
public sealed record PhoneNumber
{
    /// <summary>
    /// نمط التحقق: يبدأ بـ '+' يليه 7 إلى 15 رقماً.
    /// Validation pattern: starts with '+' followed by 7–15 digits.
    /// </summary>
    private static readonly Regex E164Pattern = new(@"^\+[1-9]\d{6,14}$", RegexOptions.Compiled);

    /// <summary>القيمة الخام لرقم الهاتف / Raw phone number value.</summary>
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    /// <summary>
    /// أنشئ كائن PhoneNumber بعد التحقق من الصيغة.
    /// Creates a validated PhoneNumber object.
    /// </summary>
    /// <param name="value">رقم الهاتف الدولي / International phone number.</param>
    /// <exception cref="ArgumentException">
    /// يُرفع عندما تكون القيمة فارغة أو لا تطابق صيغة E.164.
    /// Thrown when value is empty or does not match E.164 format.
    /// </exception>
    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "رقم الهاتف لا يمكن أن يكون فارغاً. / Phone number cannot be empty.",
                nameof(value));

        var trimmed = value.Trim();
        if (!E164Pattern.IsMatch(trimmed))
            throw new ArgumentException(
                "صيغة رقم الهاتف غير صحيحة. يجب أن يبدأ بـ '+' ويحتوي 7-15 رقماً (E.164). " +
                "/ Invalid phone number format. Must start with '+' and contain 7-15 digits (E.164). " +
                $"Received: '{trimmed}'",
                nameof(value));

        return new PhoneNumber(trimmed);
    }

    /// <summary>
    /// أنشئ كائن PhoneNumber بشكل آمن — يُعيد null إذا كانت القيمة فارغة.
    /// Safely creates a PhoneNumber — returns null if value is empty.
    /// </summary>
    public static PhoneNumber? TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Create(value);
    }

    public override string ToString() => Value;
}

/// <summary>
/// EmailAddress — البريد الإلكتروني (Value Object)
/// <para>
/// يمثّل عنوان بريد إلكتروني مُتحقَّق منه.
/// Represents a validated email address.
/// </para>
/// </summary>
public sealed record EmailAddress
{
    /// <summary>
    /// نمط التحقق البسيط لعنوان البريد الإلكتروني.
    /// Simple validation pattern for email addresses.
    /// </summary>
    private static readonly Regex EmailPattern = new(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>قيمة البريد الإلكتروني بأحرف صغيرة / Email value in lowercase.</summary>
    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    /// <summary>
    /// أنشئ كائن EmailAddress بعد التحقق من الصيغة.
    /// Creates a validated EmailAddress object.
    /// </summary>
    /// <param name="value">عنوان البريد الإلكتروني / Email address string.</param>
    /// <exception cref="ArgumentException">
    /// يُرفع عندما تكون القيمة فارغة أو بصيغة غير صحيحة.
    /// Thrown when value is empty or has an invalid format.
    /// </exception>
    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "البريد الإلكتروني لا يمكن أن يكون فارغاً. / Email address cannot be empty.",
                nameof(value));

        var trimmed = value.Trim();
        if (!EmailPattern.IsMatch(trimmed))
            throw new ArgumentException(
                $"صيغة البريد الإلكتروني غير صحيحة. / Invalid email address format. Received: '{trimmed}'",
                nameof(value));

        return new EmailAddress(trimmed.ToLowerInvariant());
    }

    /// <summary>
    /// أنشئ كائن EmailAddress بشكل آمن — يُعيد null إذا كانت القيمة فارغة.
    /// Safely creates an EmailAddress — returns null if value is empty.
    /// </summary>
    public static EmailAddress? TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Create(value);
    }

    public override string ToString() => Value;
}
