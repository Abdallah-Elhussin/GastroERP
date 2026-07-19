namespace GastroErp.Application.Common.Interfaces;

/// <summary>
/// خدمة مركزية لأسعار الصرف — يجب أن تستخدمها جميع الوحدات بدلاً من القراءة المباشرة من قاعدة البيانات.
/// </summary>
public interface IExchangeRateService
{
    /// <summary>سعر العملة مقابل عملة الشركة في تاريخ المستند. عملة الشركة تعيد 1 دائماً.</summary>
    Task<decimal> GetRateAsync(Guid tenantId, Guid currencyId, DateOnly asOfDate, CancellationToken cancellationToken = default);

    Task<decimal> GetRateByCodeAsync(Guid tenantId, string currencyCode, DateOnly asOfDate, CancellationToken cancellationToken = default);

    /// <summary>null إذا لم يوجد سجل يغطي التاريخ.</summary>
    Task<decimal?> TryGetRateAsync(Guid tenantId, Guid currencyId, DateOnly asOfDate, CancellationToken cancellationToken = default);
}
