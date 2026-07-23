using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Mapping;
using GastroErp.Application.Features.Inventory.Services;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

public class CreateGoodsReceiptCommandHandler(
    IApplicationDbContext context,
    ILogger<CreateGoodsReceiptCommandHandler> logger)
    : IRequestHandler<CreateGoodsReceiptCommand, Result<GoodsReceiptDto>>
{
    public async Task<Result<GoodsReceiptDto>> Handle(CreateGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        GoodsReceipt gr;
        PurchaseOrder? po = null;

        if (dto.DirectReceipt)
        {
            if (!dto.SupplierId.HasValue || dto.SupplierId == Guid.Empty)
                return Result<GoodsReceiptDto>.Failure("RequiredField", "Supplier is required for direct receipt.");

            var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == dto.SupplierId.Value, cancellationToken);
            if (supplier is null)
                return Result<GoodsReceiptDto>.Failure("SupplierNotFound", "Supplier not found.");
            try { supplier.EnsureCanPurchase(); }
            catch (Domain.Common.Exceptions.BusinessException ex)
            { return Result<GoodsReceiptDto>.Failure(ex.ErrorCode, ex.Message); }

            var number = string.IsNullOrWhiteSpace(dto.GrnNumber)
                ? await GoodsReceiptNumberAllocator.PeekNextAsync(context, dto.TenantId, cancellationToken)
                : dto.GrnNumber.Trim();

            gr = GoodsReceipt.CreateDirect(
                dto.TenantId, dto.SupplierId.Value, dto.WarehouseId, number,
                string.IsNullOrWhiteSpace(dto.Currency) ? supplier.Currency : dto.Currency,
                dto.BranchId, dto.ReceiptDate, dto.Notes);
        }
        else
        {
            if (!dto.PurchaseOrderId.HasValue)
                return Result<GoodsReceiptDto>.Failure("RequiredField", "Purchase order is required.");

            po = await context.PurchaseOrders.Include(p => p.Lines)
                .FirstOrDefaultAsync(p => p.Id == dto.PurchaseOrderId.Value, cancellationToken);
            if (po is null)
                return Result<GoodsReceiptDto>.Failure("PurchaseOrderNotFound", "Purchase order not found.");
            if (po.Status is PurchaseOrderStatus.Cancelled or PurchaseOrderStatus.Closed or PurchaseOrderStatus.Rejected)
                return Result<GoodsReceiptDto>.Failure("InvalidStatus", "Cannot receive against a closed/cancelled PO.");

            var number = string.IsNullOrWhiteSpace(dto.GrnNumber)
                ? await GoodsReceiptNumberAllocator.PeekNextAsync(context, dto.TenantId, cancellationToken)
                : dto.GrnNumber.Trim();

            gr = GoodsReceipt.CreateFromPurchaseOrder(
                dto.TenantId, po.Id, po.SupplierId,
                dto.WarehouseId == Guid.Empty ? po.DestinationWarehouseId : dto.WarehouseId,
                number, po.Currency, dto.BranchId, dto.ReceiptDate, dto.Notes);
        }

        ApplyHeader(gr, dto);

        var lines = dto.Lines;
        if (lines is null || lines.Count == 0)
            return Result<GoodsReceiptDto>.Failure("NoLines", "No remaining quantities to receive.");

        foreach (var line in lines)
            AddLine(gr, line);

        context.GoodsReceipts.Add(gr);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("GoodsReceipt created: {Id} source={Source}", gr.Id, gr.Source);
        return Result<GoodsReceiptDto>.Success(await EnrichAsync(context, gr, cancellationToken));
    }

    internal static void ApplyHeader(GoodsReceipt gr, CreateGoodsReceiptDto dto)
    {
        gr.UpdateHeader(
            dto.ReceiptDate ?? DateTimeOffset.UtcNow,
            dto.WarehouseId == Guid.Empty ? gr.WarehouseId : dto.WarehouseId,
            dto.ReferenceNumber,
            dto.Notes,
            dto.ReceiptMethod,
            dto.ReceivedByName,
            dto.SupplierRepName,
            dto.VehicleNumber,
            dto.WaybillNumber,
            dto.Currency,
            dto.ExchangeRate <= 0 ? 1 : dto.ExchangeRate,
            dto.BranchId);

        gr.SetInspection(
            dto.InspectionResult,
            dto.InspectedBy,
            dto.InspectionDate,
            dto.QualityNotes,
            dto.RejectionReason,
            dto.QualityCertificateRef,
            dto.ExpiryCertificateRef);
    }

    internal static void AddLine(GoodsReceipt gr, CreateGoodsReceiptLineInputDto line)
    {
        var accepted = line.AcceptedQuantity ?? line.ReceivedQuantity;
        gr.AddLine(
            line.InventoryItemId,
            line.UnitId,
            line.ReceivedQuantity,
            line.UnitCost,
            line.PurchaseOrderLineId,
            line.OrderedQuantity,
            line.PreviouslyReceivedQuantity,
            accepted,
            line.RejectedQuantity,
            line.DiscountAmount,
            line.TaxPercent,
            line.TaxAmount,
            line.BatchNumber,
            line.ProductionDate,
            line.ExpiryDate,
            line.StorageLocation,
            line.Description);
    }

    internal static async Task<GoodsReceiptDto> EnrichAsync(
        IApplicationDbContext context, GoodsReceipt gr, CancellationToken ct)
    {
        var wh = await context.Warehouses.AsNoTracking()
            .Where(w => w.Id == gr.WarehouseId).Select(w => w.NameAr).FirstOrDefaultAsync(ct) ?? "";
        var supplier = await context.Suppliers.AsNoTracking()
            .Where(s => s.Id == gr.SupplierId).Select(s => s.NameAr).FirstOrDefaultAsync(ct) ?? "";
        string poNumber = "";
        decimal? completion = null;
        if (gr.PurchaseOrderId.HasValue)
        {
            var po = await context.PurchaseOrders.AsNoTracking().Include(p => p.Lines)
                .FirstOrDefaultAsync(p => p.Id == gr.PurchaseOrderId.Value, ct);
            if (po is not null)
            {
                poNumber = po.PoNumber;
                completion = po.CompletionPercent;
            }
        }

        var itemIds = gr.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var unitIds = gr.Lines.Select(l => l.UnitId).Distinct().ToList();
        var items = await context.InventoryItems.AsNoTracking()
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => (i.NameAr, (string?)i.Sku), ct);
        var units = await context.InventoryUnits.AsNoTracking()
            .Where(u => unitIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.NameAr, ct);

        return GoodsReceiptMapper.Map(gr, poNumber, completion, supplier, wh, items, units);
    }
}

public class UpdateGoodsReceiptCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateGoodsReceiptCommand, Result<GoodsReceiptDto>>
{
    public async Task<Result<GoodsReceiptDto>> Handle(UpdateGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        var gr = await context.GoodsReceipts.Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (gr is null)
            return Result<GoodsReceiptDto>.Failure("GoodsReceiptNotFound", "Goods receipt not found.");

        var dto = request.Dto;
        gr.UpdateHeader(
            dto.ReceiptDate, dto.WarehouseId, dto.ReferenceNumber, dto.Notes,
            dto.ReceiptMethod, dto.ReceivedByName, dto.SupplierRepName,
            dto.VehicleNumber, dto.WaybillNumber, dto.Currency,
            dto.ExchangeRate <= 0 ? 1 : dto.ExchangeRate, dto.BranchId);

        gr.SetInspection(
            dto.InspectionResult, dto.InspectedBy, dto.InspectionDate,
            dto.QualityNotes, dto.RejectionReason,
            dto.QualityCertificateRef, dto.ExpiryCertificateRef);

        if (dto.Lines is not null)
        {
            gr.ClearLines();
            foreach (var line in dto.Lines)
                CreateGoodsReceiptCommandHandler.AddLine(gr, line);
        }

        context.GoodsReceipts.Update(gr);
        await context.SaveChangesAsync(cancellationToken);
        return Result<GoodsReceiptDto>.Success(
            await CreateGoodsReceiptCommandHandler.EnrichAsync(context, gr, cancellationToken));
    }
}

public class AddGoodsReceiptLineCommandHandler(
    IApplicationDbContext context,
    ILogger<AddGoodsReceiptLineCommandHandler> logger)
    : IRequestHandler<AddGoodsReceiptLineCommand, Result>
{
    public async Task<Result> Handle(AddGoodsReceiptLineCommand request, CancellationToken cancellationToken)
    {
        var gr = await context.GoodsReceipts.Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == request.GoodsReceiptId, cancellationToken);
        if (gr is null) return Result.Failure("GoodsReceiptNotFound", "Goods receipt not found.");

        decimal ordered = 0, previously = 0;
        if (request.Dto.PurchaseOrderLineId != Guid.Empty && gr.PurchaseOrderId.HasValue)
        {
            var poLine = await context.PurchaseOrders.AsNoTracking()
                .Where(p => p.Id == gr.PurchaseOrderId.Value)
                .SelectMany(p => p.Lines)
                .FirstOrDefaultAsync(l => l.Id == request.Dto.PurchaseOrderLineId, cancellationToken);
            if (poLine is not null)
            {
                ordered = poLine.Quantity;
                previously = poLine.ReceivedQuantity;
            }
        }

        gr.AddLine(
            request.Dto.InventoryItemId,
            request.Dto.UnitId,
            request.Dto.ReceivedQuantity,
            request.Dto.UnitCost,
            request.Dto.PurchaseOrderLineId == Guid.Empty ? null : request.Dto.PurchaseOrderLineId,
            ordered,
            previously,
            request.Dto.ReceivedQuantity);

        context.GoodsReceipts.Update(gr);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Line added to GoodsReceipt {GrId}", gr.Id);
        return Result.Success();
    }
}

public class ApproveGoodsReceiptCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ApproveGoodsReceiptCommand, Result>
{
    public async Task<Result> Handle(ApproveGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        var gr = await context.GoodsReceipts.Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (gr is null) return Result.Failure("GoodsReceiptNotFound", "Goods receipt not found.");
        gr.Approve();
        context.GoodsReceipts.Update(gr);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CancelGoodsReceiptCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelGoodsReceiptCommand, Result>
{
    public async Task<Result> Handle(CancelGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        var gr = await context.GoodsReceipts.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (gr is null) return Result.Failure("GoodsReceiptNotFound", "Goods receipt not found.");
        gr.Cancel();
        context.GoodsReceipts.Update(gr);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ConfirmGoodsReceiptCommandHandler(
    IPurchaseAccountingService purchaseAccounting,
    ICurrentUser currentUser,
    ILogger<ConfirmGoodsReceiptCommandHandler> logger)
    : IRequestHandler<ConfirmGoodsReceiptCommand, Result>
{
    public async Task<Result> Handle(ConfirmGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        var result = await purchaseAccounting.PostGoodsReceiptAsync(
            request.Id, currentUser.Id ?? Guid.Empty, cancellationToken);
        if (result.IsSuccess)
            logger.LogInformation("GoodsReceipt posted: {Id}", request.Id);
        return result;
    }
}

public class UnpostGoodsReceiptCommandHandler(
    IPurchaseAccountingService purchaseAccounting,
    ICurrentUser currentUser)
    : IRequestHandler<UnpostGoodsReceiptCommand, Result>
{
    public Task<Result> Handle(UnpostGoodsReceiptCommand request, CancellationToken cancellationToken)
        => purchaseAccounting.UnpostGoodsReceiptAsync(
            request.Id, currentUser.Id ?? Guid.Empty, cancellationToken);
}
