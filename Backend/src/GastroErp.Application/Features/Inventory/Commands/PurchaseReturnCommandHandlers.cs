using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Services;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

public static class PurchaseReturnMapper
{
    public static byte ToUnifiedStatusCode(PurchasingDocumentStatus status) => (byte)status;

    public static PurchaseReturnDto Map(
        PurchaseReturn doc,
        string supplierNameAr = "",
        string warehouseNameAr = "",
        string? grNumber = null,
        string? invoiceNumber = null,
        string? reasonNameAr = null,
        IReadOnlyDictionary<Guid, (string NameAr, string? Sku)>? items = null,
        IReadOnlyDictionary<Guid, string>? units = null)
    {
        items ??= new Dictionary<Guid, (string, string?)>();
        units ??= new Dictionary<Guid, string>();

        var lines = doc.Lines.Select(l =>
        {
            items.TryGetValue(l.InventoryItemId, out var item);
            units.TryGetValue(l.UnitId, out var unitName);
            return new PurchaseReturnLineDto(
                l.Id, l.InventoryItemId, item.NameAr, item.Sku, l.UnitId, unitName,
                l.GoodsReceiptLineId, l.PurchaseInvoiceLineId,
                l.OriginalQuantity, l.PreviouslyReturnedQuantity, l.AvailableToReturn,
                l.ReturnQuantity, l.UnitCost, l.DiscountAmount, l.TaxPercent, l.TaxAmount,
                l.LineSubTotal, l.LineTotal, l.BatchNumber, l.ExpiryDate,
                l.LineReason, l.Notes, l.ProductTemperature, l.DestroyItem);
        }).ToList();

        return new PurchaseReturnDto(
            doc.Id, doc.TenantId, doc.BranchId, doc.SupplierId, supplierNameAr,
            doc.WarehouseId, warehouseNameAr, doc.ReturnNumber, doc.ReturnDate,
            doc.ReturnType, doc.Status, ToUnifiedStatusCode(doc.Status),
            doc.GoodsReceiptId, grNumber, doc.PurchaseInvoiceId, invoiceNumber,
            doc.ReturnReasonId, reasonNameAr, doc.ReasonNotes, doc.ReferenceNumber, doc.Notes,
            doc.Currency, doc.SubTotal, doc.TaxAmount, doc.TotalAmount,
            doc.JournalEntryId, doc.CreditNoteJournalEntryId, doc.IsCompleted,
            lines.Count, lines, doc.CreatedAt.UtcDateTime);
    }

    public static async Task<PurchaseReturnDto> EnrichAsync(
        IApplicationDbContext context, PurchaseReturn doc, CancellationToken ct)
    {
        var wh = await context.Warehouses.AsNoTracking()
            .Where(w => w.Id == doc.WarehouseId).Select(w => w.NameAr).FirstOrDefaultAsync(ct) ?? "";
        var supplier = await context.Suppliers.AsNoTracking()
            .Where(s => s.Id == doc.SupplierId).Select(s => s.NameAr).FirstOrDefaultAsync(ct) ?? "";
        string? grNumber = null;
        if (doc.GoodsReceiptId.HasValue)
            grNumber = await context.GoodsReceipts.AsNoTracking()
                .Where(g => g.Id == doc.GoodsReceiptId.Value).Select(g => g.ReceiptNumber).FirstOrDefaultAsync(ct);
        string? invNumber = null;
        if (doc.PurchaseInvoiceId.HasValue)
            invNumber = await context.PurchaseInvoices.AsNoTracking()
                .Where(i => i.Id == doc.PurchaseInvoiceId.Value).Select(i => i.InvoiceNumber).FirstOrDefaultAsync(ct);
        string? reasonName = null;
        if (doc.ReturnReasonId.HasValue)
            reasonName = await context.PurchaseReturnReasons.AsNoTracking()
                .Where(r => r.Id == doc.ReturnReasonId.Value).Select(r => r.NameAr).FirstOrDefaultAsync(ct);

        var itemIds = doc.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var unitIds = doc.Lines.Select(l => l.UnitId).Distinct().ToList();
        var items = await context.InventoryItems.AsNoTracking()
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => (i.NameAr, (string?)i.Sku), ct);
        var units = await context.InventoryUnits.AsNoTracking()
            .Where(u => unitIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.NameAr, ct);

        return Map(doc, supplier, wh, grNumber, invNumber, reasonName, items, units);
    }

    public static void AddLine(PurchaseReturn doc, CreatePurchaseReturnLineInputDto line)
        => doc.AddLine(
            line.InventoryItemId, line.UnitId,
            line.OriginalQuantity, line.PreviouslyReturnedQuantity, line.ReturnQuantity, line.UnitCost,
            line.DiscountAmount, line.TaxPercent, line.TaxAmount,
            line.GoodsReceiptLineId, line.PurchaseInvoiceLineId,
            line.BatchNumber, line.ExpiryDate, line.LineReason, line.Notes,
            line.ProductTemperature, line.DestroyItem);
}

public class CreatePurchaseReturnCommandHandler(
    IApplicationDbContext context,
    ILogger<CreatePurchaseReturnCommandHandler> logger)
    : IRequestHandler<CreatePurchaseReturnCommand, Result<PurchaseReturnDto>>
{
    public async Task<Result<PurchaseReturnDto>> Handle(
        CreatePurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var number = string.IsNullOrWhiteSpace(dto.ReturnNumber)
            ? await PurchaseReturnNumberAllocator.PeekNextAsync(context, request.TenantId, cancellationToken)
            : dto.ReturnNumber.Trim();

        PurchaseReturn doc;
        IReadOnlyList<CreatePurchaseReturnLineInputDto> lines = dto.Lines ?? [];

        if (dto.ReturnType == PurchaseReturnType.BeforeInvoice)
        {
            if (!dto.GoodsReceiptId.HasValue)
                return Result<PurchaseReturnDto>.Failure("RequiredField", "Goods receipt is required for before-invoice returns.");

            var gr = await context.GoodsReceipts.Include(g => g.Lines)
                .FirstOrDefaultAsync(g => g.Id == dto.GoodsReceiptId.Value, cancellationToken);
            if (gr is null)
                return Result<PurchaseReturnDto>.Failure("GoodsReceiptNotFound", "Goods receipt not found.");
            if (gr.Status is not (GoodsReceiptStatus.Posted))
                return Result<PurchaseReturnDto>.Failure("InvalidStatus", "Can only return against a posted goods receipt.");

            doc = PurchaseReturn.CreateFromGoodsReceipt(
                request.TenantId, gr.SupplierId,
                dto.WarehouseId == Guid.Empty ? gr.WarehouseId : dto.WarehouseId,
                number, gr.Id, gr.Currency, dto.BranchId, dto.ReturnDate,
                dto.ReturnReasonId, dto.ReasonNotes, dto.ReferenceNumber, dto.Notes);

            if (lines.Count == 0)
            {
                lines = gr.Lines.Where(l => l.RemainingToReturn > 0).Select(l => new CreatePurchaseReturnLineInputDto(
                    l.InventoryItemId, l.UnitId, l.AcceptedQuantity, l.ReturnedQuantity, l.RemainingToReturn,
                    l.UnitCost, l.DiscountAmount, l.TaxPercent,
                    TaxAmount: l.RemainingToReturn > 0 && l.AcceptedQuantity > 0
                        ? Math.Round(l.TaxAmount * (l.RemainingToReturn / l.AcceptedQuantity), 4) : 0,
                    GoodsReceiptLineId: l.Id, BatchNumber: l.BatchNumber, ExpiryDate: l.ExpiryDate)).ToList();
            }
        }
        else
        {
            if (!dto.PurchaseInvoiceId.HasValue)
                return Result<PurchaseReturnDto>.Failure("RequiredField", "Purchase invoice is required.");

            var inv = await context.PurchaseInvoices.Include(i => i.Lines)
                .FirstOrDefaultAsync(i => i.Id == dto.PurchaseInvoiceId.Value, cancellationToken);
            if (inv is null)
                return Result<PurchaseReturnDto>.Failure("PurchaseInvoiceNotFound", "Purchase invoice not found.");
            if (inv.Status != PurchasingDocumentStatus.Posted)
                return Result<PurchaseReturnDto>.Failure("InvalidStatus", "Can only return against a posted invoice.");
            if (inv.Status == PurchasingDocumentStatus.Cancelled)
                return Result<PurchaseReturnDto>.Failure("InvalidStatus", "Cannot return against a cancelled invoice.");

            var returnType = inv.Kind == PurchaseInvoiceKind.Direct
                ? PurchaseReturnType.Direct
                : PurchaseReturnType.AfterInvoice;

            var warehouseId = dto.WarehouseId != Guid.Empty
                ? dto.WarehouseId
                : inv.WarehouseId ?? Guid.Empty;
            if (warehouseId == Guid.Empty)
                return Result<PurchaseReturnDto>.Failure("RequiredField", "Warehouse is required.");

            doc = PurchaseReturn.CreateFromInvoice(
                request.TenantId, inv.SupplierId, warehouseId, number, inv.Id, returnType,
                inv.Currency, dto.BranchId ?? inv.BranchId, dto.ReturnDate,
                dto.ReturnReasonId, dto.ReasonNotes, dto.ReferenceNumber, dto.Notes);

            if (lines.Count == 0)
            {
                return Result<PurchaseReturnDto>.Failure(
                    "RequiredField",
                    "Return lines with quantities greater than zero are required.");
            }
        }

        if (lines.Count == 0)
            return Result<PurchaseReturnDto>.Failure("NoLines", "No remaining quantities available to return.");

        foreach (var line in lines)
            PurchaseReturnMapper.AddLine(doc, line);

        context.PurchaseReturns.Add(doc);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("PurchaseReturn created: {Id} type={Type}", doc.Id, doc.ReturnType);
        return Result<PurchaseReturnDto>.Success(await PurchaseReturnMapper.EnrichAsync(context, doc, cancellationToken));
    }
}

public class UpdatePurchaseReturnCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdatePurchaseReturnCommand, Result<PurchaseReturnDto>>
{
    public async Task<Result<PurchaseReturnDto>> Handle(
        UpdatePurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.PurchaseReturns.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<PurchaseReturnDto>.Failure("PurchaseReturnNotFound", "Purchase return not found.");

        var dto = request.Dto;
        doc.UpdateHeader(dto.ReturnDate, dto.ReturnReasonId, dto.ReasonNotes, dto.ReferenceNumber, dto.Notes);

        if (dto.Lines is not null)
        {
            doc.ClearLines();
            foreach (var line in dto.Lines)
                PurchaseReturnMapper.AddLine(doc, line);
        }

        context.PurchaseReturns.Update(doc);
        await context.SaveChangesAsync(cancellationToken);
        return Result<PurchaseReturnDto>.Success(await PurchaseReturnMapper.EnrichAsync(context, doc, cancellationToken));
    }
}

public class AddPurchaseReturnLineCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AddPurchaseReturnLineCommand, Result>
{
    public async Task<Result> Handle(AddPurchaseReturnLineCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.PurchaseReturns.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == request.PurchaseReturnId, cancellationToken);
        if (doc is null) return Result.Failure("PurchaseReturnNotFound", "Purchase return not found.");

        doc.AddLine(
            request.Dto.InventoryItemId, request.Dto.UnitId,
            request.Dto.ReturnQuantity, 0, request.Dto.ReturnQuantity, request.Dto.UnitCost,
            notes: request.Dto.Notes);
        context.PurchaseReturns.Update(doc);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ApprovePurchaseReturnCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ApprovePurchaseReturnCommand, Result>
{
    public async Task<Result> Handle(ApprovePurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.PurchaseReturns.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure("PurchaseReturnNotFound", "Purchase return not found.");
        doc.Approve();
        context.PurchaseReturns.Update(doc);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class PostPurchaseReturnCommandHandler(IPurchaseAccountingService accounting)
    : IRequestHandler<PostPurchaseReturnCommand, Result>
{
    public Task<Result> Handle(PostPurchaseReturnCommand request, CancellationToken cancellationToken)
        => accounting.PostPurchaseReturnAsync(request.Id, request.UserId, cancellationToken);
}

public class UnpostPurchaseReturnCommandHandler(IPurchaseAccountingService accounting)
    : IRequestHandler<UnpostPurchaseReturnCommand, Result>
{
    public Task<Result> Handle(UnpostPurchaseReturnCommand request, CancellationToken cancellationToken)
        => accounting.UnpostPurchaseReturnAsync(request.Id, request.UserId, cancellationToken);
}

public class CancelPurchaseReturnCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelPurchaseReturnCommand, Result>
{
    public async Task<Result> Handle(CancelPurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.PurchaseReturns.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure("PurchaseReturnNotFound", "Purchase return not found.");
        doc.Cancel();
        context.PurchaseReturns.Update(doc);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SeedPurchaseReturnReasonsCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SeedPurchaseReturnReasonsCommand, Result<IReadOnlyList<PurchaseReturnReasonDto>>>
{
    private static readonly (string Code, string Ar, string En, int Order)[] Defaults =
    [
        ("EXPIRY", "انتهاء الصلاحية", "Expired", 1),
        ("DAMAGE", "تلف أثناء النقل", "Damaged in transit", 2),
        ("QUALITY", "جودة غير مطابقة", "Quality mismatch", 3),
        ("OVERQTY", "كمية زائدة", "Excess quantity", 4),
        ("SUPPLY_ERR", "خطأ في التوريد", "Supply error", 5),
        ("INV_ERR", "خطأ في الفاتورة", "Invoice error", 6),
        ("RECALL", "استدعاء من المورد", "Supplier recall", 7),
        ("OTHER", "سبب آخر", "Other", 99)
    ];

    public async Task<Result<IReadOnlyList<PurchaseReturnReasonDto>>> Handle(
        SeedPurchaseReturnReasonsCommand request, CancellationToken cancellationToken)
    {
        var existing = await context.PurchaseReturnReasons
            .Where(r => r.TenantId == request.TenantId).ToListAsync(cancellationToken);
        var codes = existing.Select(r => r.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var (code, ar, en, order) in Defaults)
        {
            if (codes.Contains(code)) continue;
            context.PurchaseReturnReasons.Add(new PurchaseReturnReason(request.TenantId, code, ar, en, order));
        }

        await context.SaveChangesAsync(cancellationToken);
        var all = await context.PurchaseReturnReasons.AsNoTracking()
            .Where(r => r.TenantId == request.TenantId && r.IsActive)
            .OrderBy(r => r.SortOrder)
            .Select(r => new PurchaseReturnReasonDto(r.Id, r.Code, r.NameAr, r.NameEn, r.SortOrder, r.IsActive))
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<PurchaseReturnReasonDto>>.Success(all);
    }
}
