using GastroErp.Application.Common.Responses;

namespace GastroErp.Application.Features.Sales.BackOffice.Fulfillment;

/// <summary>
/// نقطة الترحيل/العكس لمستندات المبيعات الإدارية غير الفواتير:
/// إذن تسليم — مرتجع مبيعات — إشعار مدين.
/// </summary>
public interface IBackOfficeSalesFulfillmentService
{
    Task<Result> PostDeliveryAsync(Guid deliveryNoteId, Guid userId, CancellationToken ct = default);
    Task<Result> UnpostDeliveryAsync(Guid deliveryNoteId, Guid userId, CancellationToken ct = default);

    Task<Result> PostReturnAsync(Guid returnId, Guid userId, CancellationToken ct = default);
    Task<Result> UnpostReturnAsync(Guid returnId, Guid userId, CancellationToken ct = default);

    Task<Result> PostDebitNoteAsync(Guid debitNoteId, Guid userId, CancellationToken ct = default);
    Task<Result> UnpostDebitNoteAsync(Guid debitNoteId, Guid userId, CancellationToken ct = default);
}
