using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Inventory.Mapping;

public static class GoodsReceiptMapper
{
    public static byte ToUnifiedStatusCode(GoodsReceiptStatus status) => status switch
    {
        GoodsReceiptStatus.Draft => 0,
        GoodsReceiptStatus.Approved => 1,
        GoodsReceiptStatus.Posted => 2,
        GoodsReceiptStatus.Reversed => 8,
        GoodsReceiptStatus.Cancelled => 9,
        _ => 0
    };

    public static GoodsReceiptDto Map(
        GoodsReceipt gr,
        string poNumber = "",
        decimal? poCompletionPercent = null,
        string supplierNameAr = "",
        string warehouseNameAr = "",
        IReadOnlyDictionary<Guid, (string NameAr, string? Sku)>? items = null,
        IReadOnlyDictionary<Guid, string>? units = null)
    {
        items ??= new Dictionary<Guid, (string, string?)>();
        units ??= new Dictionary<Guid, string>();

        var lines = gr.Lines.Select(l =>
        {
            items.TryGetValue(l.InventoryItemId, out var item);
            units.TryGetValue(l.UnitId, out var unitName);
            return new GoodsReceiptLineDto(
                l.Id,
                l.InventoryItemId,
                item.NameAr,
                item.Sku,
                l.UnitId,
                unitName,
                l.PurchaseOrderLineId,
                l.OrderedQuantity,
                l.PreviouslyReceivedQuantity,
                l.RemainingAtCreation,
                l.ReceivedQuantity,
                l.AcceptedQuantity,
                l.RejectedQuantity,
                l.UnitCost,
                l.DiscountAmount,
                l.TaxPercent,
                l.TaxAmount,
                l.LineSubTotal,
                l.InvoicedQuantity,
                l.BatchNumber,
                l.ProductionDate,
                l.ExpiryDate,
                l.StorageLocation,
                l.Description);
        }).ToList();

        return new GoodsReceiptDto(
            gr.Id,
            gr.TenantId,
            gr.BranchId,
            gr.PurchaseOrderId,
            poNumber,
            poCompletionPercent,
            gr.SupplierId,
            supplierNameAr,
            gr.WarehouseId,
            warehouseNameAr,
            gr.ReceiptNumber,
            gr.ReferenceNumber,
            gr.ReceiptDate,
            gr.Status,
            ToUnifiedStatusCode(gr.Status),
            gr.Source,
            gr.Currency,
            gr.ExchangeRate,
            gr.ReceiptMethod,
            gr.ReceivedByName,
            gr.SupplierRepName,
            gr.VehicleNumber,
            gr.WaybillNumber,
            gr.Notes,
            gr.InspectionResult,
            gr.InspectedBy,
            gr.InspectionDate,
            gr.QualityNotes,
            gr.RejectionReason,
            gr.QualityCertificateRef,
            gr.ExpiryCertificateRef,
            gr.JournalEntryId,
            lines.Count,
            lines.Sum(x => x.ReceivedQuantity),
            gr.TotalValue,
            gr.TotalTax,
            gr.GrandTotal,
            gr.IsInvoiced,
            gr.IsPartiallyInvoiced,
            lines,
            gr.CreatedAt.UtcDateTime);
    }
}
