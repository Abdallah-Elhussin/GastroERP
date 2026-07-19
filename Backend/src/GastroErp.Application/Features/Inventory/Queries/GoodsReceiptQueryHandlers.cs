using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Mapping;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Queries;

public class GetGoodsReceiptsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetGoodsReceiptsQuery, PagedResult<GoodsReceiptDto>>
{
    public async Task<PagedResult<GoodsReceiptDto>> Handle(
        GetGoodsReceiptsQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = context.GoodsReceipts.AsNoTracking()
            .Include(g => g.Lines)
            .Where(g => g.TenantId == request.TenantId);

        if (request.SupplierId.HasValue)
            query = query.Where(g => g.SupplierId == request.SupplierId.Value);
        if (request.Status.HasValue)
            query = query.Where(g => g.Status == request.Status.Value);
        if (request.From.HasValue)
            query = query.Where(g => g.ReceiptDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(g => g.ReceiptDate <= request.To.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(g =>
                g.ReceiptNumber.Contains(s) ||
                (g.ReferenceNumber != null && g.ReferenceNumber.Contains(s)) ||
                (g.Notes != null && g.Notes.Contains(s)));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(g => g.ReceiptDate)
            .ThenByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<GoodsReceiptDto>(rows.Count);
        foreach (var gr in rows)
            items.Add(await CreateGoodsReceiptCommandHandler.EnrichAsync(context, gr, cancellationToken));

        return PagedResult<GoodsReceiptDto>.Success(items, page, pageSize, total);
    }
}

public class GetGoodsReceiptByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetGoodsReceiptByIdQuery, Result<GoodsReceiptDto>>
{
    public async Task<Result<GoodsReceiptDto>> Handle(
        GetGoodsReceiptByIdQuery request, CancellationToken cancellationToken)
    {
        var gr = await context.GoodsReceipts.AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (gr is null)
            return Result<GoodsReceiptDto>.Failure("GoodsReceiptNotFound", "Goods receipt not found.");

        return Result<GoodsReceiptDto>.Success(
            await CreateGoodsReceiptCommandHandler.EnrichAsync(context, gr, cancellationToken));
    }
}

public class PreviewGoodsReceiptFromPoQueryHandler(IApplicationDbContext context)
    : IRequestHandler<PreviewGoodsReceiptFromPoQuery, Result<GoodsReceiptDto>>
{
    public async Task<Result<GoodsReceiptDto>> Handle(
        PreviewGoodsReceiptFromPoQuery request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.AsNoTracking()
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId && p.TenantId == request.TenantId, cancellationToken);
        if (po is null)
            return Result<GoodsReceiptDto>.Failure("PurchaseOrderNotFound", "Purchase order not found.");

        var openLines = po.Lines.Where(l => l.Quantity - l.ReceivedQuantity > 0).ToList();
        if (openLines.Count == 0)
            return Result<GoodsReceiptDto>.Failure("NothingToReceive", "Purchase order has no remaining quantity.");

        var preview = GoodsReceipt.CreateFromPurchaseOrder(
            request.TenantId, po.Id, po.SupplierId, po.DestinationWarehouseId,
            "PREVIEW", po.Currency, notes: po.Notes);

        foreach (var line in openLines)
        {
            var remaining = line.Quantity - line.ReceivedQuantity;
            preview.AddLine(
                line.InventoryItemId, line.UnitId, remaining, line.UnitPrice,
                line.Id, line.Quantity, line.ReceivedQuantity, remaining,
                taxAmount: line.TaxAmount);
        }

        return Result<GoodsReceiptDto>.Success(
            await CreateGoodsReceiptCommandHandler.EnrichAsync(context, preview, cancellationToken));
    }
}
