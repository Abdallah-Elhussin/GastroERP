using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Mapping;

public static class PurchaseOrderMapper
{
    public static async Task<PurchaseOrderDto> ToDtoAsync(
        IApplicationDbContext context,
        PurchaseOrder po,
        CancellationToken cancellationToken = default)
    {
        var supplierName = await context.Suppliers.AsNoTracking()
            .Where(s => s.Id == po.SupplierId)
            .Select(s => s.NameAr)
            .FirstOrDefaultAsync(cancellationToken);

        var warehouseName = await context.Warehouses.AsNoTracking()
            .Where(w => w.Id == po.DestinationWarehouseId)
            .Select(w => w.NameAr)
            .FirstOrDefaultAsync(cancellationToken);

        var itemIds = po.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var unitIds = po.Lines.Select(l => l.UnitId).Distinct().ToList();

        var items = await context.InventoryItems.AsNoTracking()
            .Where(i => itemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.NameAr, i.Sku })
            .ToListAsync(cancellationToken);

        var units = await context.InventoryUnits.AsNoTracking()
            .Where(u => unitIds.Contains(u.Id))
            .Select(u => new { u.Id, u.NameAr })
            .ToListAsync(cancellationToken);

        var itemMap = items.ToDictionary(x => x.Id);
        var unitMap = units.ToDictionary(x => x.Id);

        var lines = po.Lines.Select(l =>
        {
            itemMap.TryGetValue(l.InventoryItemId, out var item);
            unitMap.TryGetValue(l.UnitId, out var unit);
            return new PurchaseOrderLineDto(
                l.Id,
                l.InventoryItemId,
                item?.NameAr,
                item?.Sku,
                l.UnitId,
                unit?.NameAr,
                l.WarehouseId,
                l.Quantity,
                l.UnitPrice,
                l.DiscountAmount,
                l.TaxAmount,
                l.LineSubTotal,
                l.LineTotal,
                l.ReceivedQuantity,
                l.InvoicedQuantity,
                l.RemainingQuantity,
                l.Description,
                l.LineNotes);
        }).ToList();

        return new PurchaseOrderDto(
            po.Id,
            po.TenantId,
            po.SupplierId,
            supplierName,
            po.DestinationWarehouseId,
            warehouseName,
            po.BranchId,
            po.CostCenterId,
            po.ResponsibleEmployeeId,
            po.PoNumber,
            po.OrderType,
            po.OrderDate,
            po.ExpectedDeliveryDate,
            po.Status,
            (byte)po.Status,
            po.Currency,
            po.ExchangeRate,
            po.PaymentMethod,
            po.PaymentTerms,
            po.ExternalReference,
            po.Notes,
            po.TotalAmount,
            po.CompletionPercent,
            po.RemainingQuantity,
            po.Lines.Count,
            po.LastReceiptDate,
            po.CreatedAt,
            lines);
    }
}
