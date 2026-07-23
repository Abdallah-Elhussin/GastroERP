using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Services;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Queries;

public class GetPurchaseReturnsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPurchaseReturnsQuery, PagedResult<PurchaseReturnDto>>
{
    public async Task<PagedResult<PurchaseReturnDto>> Handle(
        GetPurchaseReturnsQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = context.PurchaseReturns.AsNoTracking()
            .Include(r => r.Lines)
            .Where(r => r.TenantId == request.TenantId);

        if (request.SupplierId.HasValue)
            query = query.Where(r => r.SupplierId == request.SupplierId.Value);
        if (request.WarehouseId.HasValue)
            query = query.Where(r => r.WarehouseId == request.WarehouseId.Value);
        if (request.ReturnType.HasValue)
            query = query.Where(r => r.ReturnType == request.ReturnType.Value);
        else if (request.InvoiceBasedOnly)
            query = query.Where(r =>
                r.ReturnType == PurchaseReturnType.AfterInvoice ||
                r.ReturnType == PurchaseReturnType.Direct);
        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);
        if (request.From.HasValue)
            query = query.Where(r => r.ReturnDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(r => r.ReturnDate <= request.To.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(r =>
                r.ReturnNumber.Contains(s) ||
                (r.ReferenceNumber != null && r.ReferenceNumber.Contains(s)) ||
                (r.Notes != null && r.Notes.Contains(s)) ||
                (r.ReasonNotes != null && r.ReasonNotes.Contains(s)));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(r => r.ReturnDate)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<PurchaseReturnDto>(rows.Count);
        foreach (var row in rows)
            items.Add(await PurchaseReturnMapper.EnrichAsync(context, row, cancellationToken));

        return PagedResult<PurchaseReturnDto>.Success(items, page, pageSize, total);
    }
}

public class GetPurchaseReturnByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPurchaseReturnByIdQuery, Result<PurchaseReturnDto>>
{
    public async Task<Result<PurchaseReturnDto>> Handle(
        GetPurchaseReturnByIdQuery request, CancellationToken cancellationToken)
    {
        var doc = await context.PurchaseReturns.AsNoTracking()
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<PurchaseReturnDto>.Failure("PurchaseReturnNotFound", "Purchase return not found.");

        return Result<PurchaseReturnDto>.Success(
            await PurchaseReturnMapper.EnrichAsync(context, doc, cancellationToken));
    }
}

public class GetNextPurchaseReturnNumberQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetNextPurchaseReturnNumberQuery, Result<string>>
{
    public async Task<Result<string>> Handle(
        GetNextPurchaseReturnNumberQuery request, CancellationToken cancellationToken)
    {
        var number = await PurchaseReturnNumberAllocator.PeekNextAsync(
            context, request.TenantId, cancellationToken);
        return Result<string>.Success(number);
    }
}

public class PreviewPurchaseReturnFromGrnQueryHandler(IApplicationDbContext context)
    : IRequestHandler<PreviewPurchaseReturnFromGrnQuery, Result<PurchaseReturnDto>>
{
    public async Task<Result<PurchaseReturnDto>> Handle(
        PreviewPurchaseReturnFromGrnQuery request, CancellationToken cancellationToken)
    {
        var gr = await context.GoodsReceipts.AsNoTracking().Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == request.GoodsReceiptId && g.TenantId == request.TenantId, cancellationToken);
        if (gr is null)
            return Result<PurchaseReturnDto>.Failure("GoodsReceiptNotFound", "Goods receipt not found.");
        if (gr.Status != GoodsReceiptStatus.Posted)
            return Result<PurchaseReturnDto>.Failure("InvalidStatus", "Can only return against a posted goods receipt.");

        var preview = Domain.Entities.Inventory.Purchasing.PurchaseReturn.CreateFromGoodsReceipt(
            request.TenantId, gr.SupplierId, gr.WarehouseId, "PREVIEW", gr.Id, gr.Currency);
        foreach (var l in gr.Lines.Where(x => x.RemainingToReturn > 0))
        {
            preview.AddLine(l.InventoryItemId, l.UnitId, l.AcceptedQuantity, l.ReturnedQuantity, l.RemainingToReturn,
                l.UnitCost, l.DiscountAmount, l.TaxPercent,
                l.RemainingToReturn > 0 && l.AcceptedQuantity > 0
                    ? Math.Round(l.TaxAmount * (l.RemainingToReturn / l.AcceptedQuantity), 4) : 0,
                l.Id, batchNumber: l.BatchNumber, expiryDate: l.ExpiryDate);
        }

        if (!preview.Lines.Any())
            return Result<PurchaseReturnDto>.Failure("NothingToReturn", "No remaining quantities to return.");

        return Result<PurchaseReturnDto>.Success(
            await PurchaseReturnMapper.EnrichAsync(context, preview, cancellationToken));
    }
}

public class PreviewPurchaseReturnFromInvoiceQueryHandler(IApplicationDbContext context)
    : IRequestHandler<PreviewPurchaseReturnFromInvoiceQuery, Result<PurchaseReturnDto>>
{
    public async Task<Result<PurchaseReturnDto>> Handle(
        PreviewPurchaseReturnFromInvoiceQuery request, CancellationToken cancellationToken)
    {
        var forReturn = await GetPurchaseInvoiceForReturnQueryHandler.BuildAsync(
            context, request.TenantId, request.PurchaseInvoiceId, cancellationToken);
        if (!forReturn.IsSuccess)
            return Result<PurchaseReturnDto>.Failure(forReturn.ErrorCode!, forReturn.ErrorMessage!);

        var data = forReturn.Data!;
        if (!data.Header.CanCreateReturn)
        {
            var code = data.Header.BlockReasonCode ?? "InvalidStatus";
            return Result<PurchaseReturnDto>.Failure(code, data.Header.BlockReason ?? "Cannot create return from this invoice.");
        }

        var warehouseId = data.Header.WarehouseId
            ?? data.Items.Select(i => i.WarehouseId).FirstOrDefault(id => id.HasValue);
        if (warehouseId is null)
            return Result<PurchaseReturnDto>.Failure("RequiredField", "Invoice warehouse is required.");

        var type = data.Header.Kind == PurchaseInvoiceKind.Direct
            ? PurchaseReturnType.Direct
            : PurchaseReturnType.AfterInvoice;

        var preview = Domain.Entities.Inventory.Purchasing.PurchaseReturn.CreateFromInvoice(
            request.TenantId, data.Header.SupplierId, warehouseId.Value, "PREVIEW",
            data.Header.Id, type, data.Header.Currency, null);

        var baseDto = await PurchaseReturnMapper.EnrichAsync(context, preview, cancellationToken);
        var lines = data.Items
            .Where(i => !i.IsDisabled)
            .Select(i => new PurchaseReturnLineDto(
                Guid.Empty,
                i.InventoryItemId,
                i.ItemNameAr,
                i.ItemSku,
                i.UnitId,
                i.UnitNameAr,
                null,
                i.PurchaseInvoiceLineId,
                i.OriginalQuantity,
                i.PreviouslyReturnedQuantity,
                i.RemainingQuantity,
                0,
                i.UnitPrice,
                i.DiscountAmount,
                i.TaxPercent,
                0,
                0,
                0,
                null,
                null,
                i.Description,
                null,
                null,
                false))
            .ToList();

        return Result<PurchaseReturnDto>.Success(baseDto with
        {
            SupplierNameAr = data.Header.SupplierNameAr,
            WarehouseId = warehouseId.Value,
            WarehouseNameAr = data.Header.WarehouseNameAr ?? baseDto.WarehouseNameAr,
            PurchaseInvoiceId = data.Header.Id,
            PurchaseInvoiceNumber = data.Header.InvoiceNumber,
            Notes = data.Header.Notes,
            Currency = data.Header.Currency,
            ReferenceNumber = data.Header.ExternalReference ?? data.Header.SupplierInvoiceNumber,
            Lines = lines,
            LineCount = lines.Count,
            SubTotal = 0,
            TaxAmount = 0,
            TotalAmount = 0
        });
    }
}

public class GetPurchaseInvoiceForReturnQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPurchaseInvoiceForReturnQuery, Result<PurchaseInvoiceForReturnDto>>
{
    public Task<Result<PurchaseInvoiceForReturnDto>> Handle(
        GetPurchaseInvoiceForReturnQuery request, CancellationToken cancellationToken)
        => BuildAsync(context, request.TenantId, request.PurchaseInvoiceId, cancellationToken);

    internal static async Task<Result<PurchaseInvoiceForReturnDto>> BuildAsync(
        IApplicationDbContext context,
        Guid tenantId,
        Guid purchaseInvoiceId,
        CancellationToken cancellationToken)
    {
        var inv = await context.PurchaseInvoices.AsNoTracking()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == purchaseInvoiceId && i.TenantId == tenantId, cancellationToken);

        if (inv is null)
            return Result<PurchaseInvoiceForReturnDto>.Failure("PurchaseInvoiceNotFound", "Purchase invoice not found.");

        string? blockCode = null;
        string? blockReason = null;
        if (inv.Status == PurchasingDocumentStatus.Cancelled)
        {
            blockCode = "InvoiceCancelled";
            blockReason = "Cannot create a return from a cancelled invoice.";
        }
        else if (inv.Status == PurchasingDocumentStatus.Reversed)
        {
            blockCode = "InvoiceReversed";
            blockReason = "Cannot create a return from a reversed invoice.";
        }
        else if (inv.Status != PurchasingDocumentStatus.Posted)
        {
            blockCode = "InvalidStatus";
            blockReason = "Can only return against a posted invoice.";
        }

        var supplier = await context.Suppliers.AsNoTracking()
            .Where(s => s.Id == inv.SupplierId)
            .Select(s => new { s.NameAr, s.ApAccountId })
            .FirstOrDefaultAsync(cancellationToken);

        string? apAccountName = null;
        if (supplier?.ApAccountId is Guid apId)
        {
            apAccountName = await context.ChartOfAccounts.AsNoTracking()
                .Where(a => a.Id == apId)
                .Select(a => a.NameAr)
                .FirstOrDefaultAsync(cancellationToken);
        }

        string? costCenterName = null;
        if (inv.CostCenterId is Guid ccId)
        {
            costCenterName = await context.CostCenters.AsNoTracking()
                .Where(c => c.Id == ccId)
                .Select(c => c.NameAr)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var itemIds = inv.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var unitIds = inv.Lines.Select(l => l.UnitId).Distinct().ToList();
        var lineWhIds = inv.Lines.Where(l => l.LineWarehouseId.HasValue)
            .Select(l => l.LineWarehouseId!.Value).Distinct().ToList();
        if (inv.WarehouseId.HasValue) lineWhIds.Add(inv.WarehouseId.Value);
        lineWhIds = lineWhIds.Distinct().ToList();

        var items = await context.InventoryItems.AsNoTracking()
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => new { i.NameAr, i.Sku }, cancellationToken);
        var units = await context.InventoryUnits.AsNoTracking()
            .Where(u => unitIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.NameAr, cancellationToken);
        var warehouses = await context.Warehouses.AsNoTracking()
            .Where(w => lineWhIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, w => w.NameAr, cancellationToken);

        // Header warehouse: invoice warehouse, or the only distinct line warehouse.
        Guid? resolvedWarehouseId = inv.WarehouseId;
        string? resolvedWarehouseName = inv.WarehouseId is Guid hw && warehouses.TryGetValue(hw, out var hwn)
            ? hwn
            : null;
        if (resolvedWarehouseId is null)
        {
            var distinctLineWh = inv.Lines
                .Select(l => l.LineWarehouseId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();
            if (distinctLineWh.Count == 1)
            {
                resolvedWarehouseId = distinctLineWh[0];
                warehouses.TryGetValue(resolvedWarehouseId.Value, out resolvedWarehouseName);
            }
        }

        var lineDtos = inv.Lines.Select(l =>
        {
            items.TryGetValue(l.InventoryItemId, out var item);
            units.TryGetValue(l.UnitId, out var unitName);
            var whId = l.LineWarehouseId ?? inv.WarehouseId;
            string? whName = whId is Guid wid && warehouses.TryGetValue(wid, out var wn) ? wn : null;
            var remaining = l.RemainingToReturn;
            var disabled = remaining <= 0;
            return new PurchaseInvoiceForReturnLineDto(
                l.Id,
                l.InventoryItemId,
                item?.NameAr,
                item?.Sku,
                l.Description,
                l.UnitId,
                unitName,
                whId,
                whName,
                l.Quantity,
                l.ReturnedQuantity,
                remaining,
                ReturnQuantity: 0,
                l.UnitPrice,
                l.DiscountPercent,
                l.DiscountAmount,
                l.TaxPercent,
                l.TaxAmount,
                l.LineNet,
                l.LineTotal,
                disabled);
        }).ToList();

        var totalRemaining = lineDtos.Sum(l => l.RemainingQuantity);
        if (blockCode is null && totalRemaining <= 0)
        {
            blockCode = "NothingToReturn";
            blockReason = "No remaining quantities to return.";
        }

        var taxes = lineDtos
            .Where(l => l.TaxPercent > 0 || l.TaxAmount > 0)
            .GroupBy(l => l.TaxPercent)
            .Select(g => new PurchaseInvoiceForReturnTaxDto(
                g.Key,
                g.Sum(x => x.LineSubTotal),
                g.Sum(x => x.TaxAmount)))
            .OrderBy(t => t.TaxPercent)
            .ToList();

        var header = new PurchaseInvoiceForReturnHeaderDto(
            inv.Id,
            inv.InvoiceNumber,
            inv.Kind,
            inv.Status,
            inv.PaymentMode,
            inv.Nature,
            inv.SupplierId,
            supplier?.NameAr ?? string.Empty,
            resolvedWarehouseId,
            resolvedWarehouseName,
            inv.CostCenterId,
            costCenterName,
            inv.InvoiceDate,
            inv.DueDate,
            inv.Currency,
            inv.ExchangeRate,
            inv.SupplierInvoiceNumber,
            inv.ExternalReference,
            inv.Notes,
            supplier?.ApAccountId,
            apAccountName,
            inv.DiscountAmount,
            inv.SubTotal,
            inv.TaxAmount,
            inv.TotalAmount,
            CanCreateReturn: blockCode is null,
            BlockReason: blockReason,
            BlockReasonCode: blockCode);

        return Result<PurchaseInvoiceForReturnDto>.Success(new PurchaseInvoiceForReturnDto(
            header,
            lineDtos,
            taxes,
            totalRemaining,
            inv.TotalAmount));
    }
}

public class GetPurchaseReturnReasonsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPurchaseReturnReasonsQuery, Result<IReadOnlyList<PurchaseReturnReasonDto>>>
{
    public async Task<Result<IReadOnlyList<PurchaseReturnReasonDto>>> Handle(
        GetPurchaseReturnReasonsQuery request, CancellationToken cancellationToken)
    {
        var query = context.PurchaseReturnReasons.AsNoTracking()
            .Where(r => r.TenantId == request.TenantId);
        if (request.ActiveOnly)
            query = query.Where(r => r.IsActive);

        var rows = await query.OrderBy(r => r.SortOrder)
            .Select(r => new PurchaseReturnReasonDto(r.Id, r.Code, r.NameAr, r.NameEn, r.SortOrder, r.IsActive))
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<PurchaseReturnReasonDto>>.Success(rows);
    }
}
