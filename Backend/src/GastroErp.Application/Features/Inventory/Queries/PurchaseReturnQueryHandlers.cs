using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
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
        var inv = await context.PurchaseInvoices.AsNoTracking().Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == request.PurchaseInvoiceId && i.TenantId == request.TenantId, cancellationToken);
        if (inv is null)
            return Result<PurchaseReturnDto>.Failure("PurchaseInvoiceNotFound", "Purchase invoice not found.");
        if (inv.Status != PurchasingDocumentStatus.Posted)
            return Result<PurchaseReturnDto>.Failure("InvalidStatus", "Can only return against a posted invoice.");
        if (inv.WarehouseId is null)
            return Result<PurchaseReturnDto>.Failure("RequiredField", "Invoice warehouse is required.");

        var type = inv.Kind == PurchaseInvoiceKind.Direct
            ? PurchaseReturnType.Direct
            : PurchaseReturnType.AfterInvoice;

        var preview = Domain.Entities.Inventory.Purchasing.PurchaseReturn.CreateFromInvoice(
            request.TenantId, inv.SupplierId, inv.WarehouseId.Value, "PREVIEW", inv.Id, type, inv.Currency, inv.BranchId);

        foreach (var l in inv.Lines.Where(x => x.RemainingToReturn > 0))
        {
            preview.AddLine(l.InventoryItemId, l.UnitId, l.Quantity, l.ReturnedQuantity, l.RemainingToReturn,
                l.UnitPrice,
                taxAmount: l.RemainingToReturn > 0 && l.Quantity > 0
                    ? Math.Round(l.TaxAmount * (l.RemainingToReturn / l.Quantity), 4) : 0,
                purchaseInvoiceLineId: l.Id);
        }

        if (!preview.Lines.Any())
            return Result<PurchaseReturnDto>.Failure("NothingToReturn", "No remaining quantities to return.");

        return Result<PurchaseReturnDto>.Success(
            await PurchaseReturnMapper.EnrichAsync(context, preview, cancellationToken));
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
