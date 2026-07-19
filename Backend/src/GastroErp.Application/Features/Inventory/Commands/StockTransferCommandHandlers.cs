using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Inventory;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Entities.Inventory.Warehouse;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

internal static class StockTransferMapper
{
    public static async Task<StockTransferDto> ToDtoAsync(
        IApplicationDbContext db,
        StockTransfer doc,
        CancellationToken ct)
    {
        var whIds = new[] { doc.SourceWarehouseId, doc.DestinationWarehouseId };
        var warehouses = await db.Warehouses.AsNoTracking()
            .Where(w => whIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, w => w.NameAr, ct);

        var itemIds = doc.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var unitIds = doc.Lines.Select(l => l.UnitId).Distinct().ToList();

        var items = itemIds.Count == 0
            ? new Dictionary<Guid, (string NameAr, string? Sku)>()
            : await db.InventoryItems.AsNoTracking()
                .Where(i => itemIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, i => (i.NameAr, i.Sku), ct);

        var units = unitIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.InventoryUnits.AsNoTracking()
                .Where(u => unitIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.NameAr, ct);

        var lines = doc.Lines.Select(l =>
        {
            items.TryGetValue(l.InventoryItemId, out var item);
            return new StockTransferLineDetailDto(
                l.Id,
                l.InventoryItemId,
                item.NameAr,
                item.Sku,
                l.UnitId,
                units.GetValueOrDefault(l.UnitId),
                l.Quantity,
                l.UnitCost,
                l.LineTotal,
                l.ReceivedQuantity,
                l.BatchNumber);
        }).ToList();

        return new StockTransferDto(
            doc.Id,
            doc.TenantId,
            doc.SourceWarehouseId,
            warehouses.GetValueOrDefault(doc.SourceWarehouseId) ?? string.Empty,
            doc.DestinationWarehouseId,
            warehouses.GetValueOrDefault(doc.DestinationWarehouseId) ?? string.Empty,
            doc.TransferNumber,
            doc.TransferDate,
            doc.TransferType.ToString(),
            (byte)doc.TransferType,
            doc.Status.ToString(),
            (byte)doc.Status,
            doc.Notes,
            doc.Lines.Count,
            doc.TotalAmount,
            lines,
            doc.CreatedAt.UtcDateTime);
    }

    internal static async Task<Result<string>> AllocateDocumentNumberAsync(
        IApplicationDbContext db,
        Guid tenantId,
        CancellationToken ct)
    {
        var setting = await db.InventorySettings
            .Include(s => s.DocumentSeries)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        if (setting is null)
            return Result<string>.Failure("SettingsNotFound", "Inventory settings not found.");

        var series = setting.DocumentSeries
            .FirstOrDefault(s => s.DocumentType == InventoryDocumentSeriesType.StockTransfer);
        if (series is null)
            return Result<string>.Success($"TR{DateTime.UtcNow:yyyyMMddHHmmss}");

        var number = series.AllocateNext();
        await db.SaveChangesAsync(ct);
        return Result<string>.Success(number);
    }
}

public sealed class CreateStockTransferCommandHandler(
    IApplicationDbContext context,
    ILogger<CreateStockTransferCommandHandler> logger)
    : IRequestHandler<CreateStockTransferCommand, Result<StockTransferDto>>
{
    public async Task<Result<StockTransferDto>> Handle(CreateStockTransferCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        try
        {
            var number = dto.TransferNumber?.Trim();
            if (string.IsNullOrWhiteSpace(number) || dto.AutoGenerateNumber)
            {
                var generated = await StockTransferMapper.AllocateDocumentNumberAsync(context, dto.TenantId, cancellationToken);
                if (generated.IsFailure)
                    return Result<StockTransferDto>.Failure(generated.ErrorCode!, generated.ErrorMessage);
                number = generated.Data;
            }

            if (await context.StockTransfers.AnyAsync(
                    t => t.TenantId == dto.TenantId && t.TransferNumber == number, cancellationToken))
                return Result<StockTransferDto>.Failure("DuplicateTransferNumber", "Transfer number already exists.");

            var fromOk = await context.Warehouses.AnyAsync(
                w => w.Id == dto.SourceWarehouseId && w.TenantId == dto.TenantId, cancellationToken);
            if (!fromOk) return Result<StockTransferDto>.Failure("WarehouseNotFound", "Source warehouse not found.");

            var toOk = await context.Warehouses.AnyAsync(
                w => w.Id == dto.DestinationWarehouseId && w.TenantId == dto.TenantId, cancellationToken);
            if (!toOk) return Result<StockTransferDto>.Failure("WarehouseNotFound", "Destination warehouse not found.");

            var doc = new StockTransfer(
                dto.TenantId,
                dto.SourceWarehouseId,
                dto.DestinationWarehouseId,
                number!,
                dto.TransferDate,
                (StockTransferType)dto.TransferType,
                dto.Notes);

            if (dto.Lines is { Count: > 0 })
            {
                foreach (var line in dto.Lines)
                    doc.AddLine(line.InventoryItemId, line.UnitId, line.Quantity, line.UnitCost, line.BatchNumber);
            }

            context.StockTransfers.Add(doc);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("StockTransfer created: {Id} ({Num})", doc.Id, doc.TransferNumber);
            return Result<StockTransferDto>.Success(await StockTransferMapper.ToDtoAsync(context, doc, cancellationToken));
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result<StockTransferDto>.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed class UpdateStockTransferCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateStockTransferCommand, Result<StockTransferDto>>
{
    public async Task<Result<StockTransferDto>> Handle(UpdateStockTransferCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var doc = await context.StockTransfers.Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.TenantId == dto.TenantId, cancellationToken);
        if (doc is null)
            return Result<StockTransferDto>.Failure("StockTransferNotFound", "Stock transfer not found.");

        try
        {
            doc.UpdateHeader(
                dto.TransferDate,
                dto.SourceWarehouseId,
                dto.DestinationWarehouseId,
                (StockTransferType)dto.TransferType,
                dto.Notes);

            if (dto.Lines is not null)
            {
                doc.ClearLines();
                foreach (var line in dto.Lines)
                    doc.AddLine(line.InventoryItemId, line.UnitId, line.Quantity, line.UnitCost, line.BatchNumber);
            }

            await context.SaveChangesAsync(cancellationToken);
            return Result<StockTransferDto>.Success(await StockTransferMapper.ToDtoAsync(context, doc, cancellationToken));
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result<StockTransferDto>.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed class AddTransferLineCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AddTransferLineCommand, Result>
{
    public async Task<Result> Handle(AddTransferLineCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.StockTransfers.Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == request.TransferId, cancellationToken);
        if (doc is null) return Result.Failure("StockTransferNotFound", "Stock transfer not found.");

        try
        {
            doc.AddLine(
                request.Dto.InventoryItemId,
                request.Dto.UnitId,
                request.Dto.Quantity,
                request.Dto.UnitCost,
                request.Dto.BatchNumber);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed class ApproveStockTransferCommandHandler(
    IApplicationDbContext context,
    ILogger<ApproveStockTransferCommandHandler> logger)
    : IRequestHandler<ApproveStockTransferCommand, Result>
{
    public async Task<Result> Handle(ApproveStockTransferCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.StockTransfers.Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure("StockTransferNotFound", "Stock transfer not found.");

        try
        {
            doc.Approve();
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("StockTransfer approved: {Id}", doc.Id);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UnapproveStockTransferCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UnapproveStockTransferCommand, Result>
{
    public async Task<Result> Handle(UnapproveStockTransferCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.StockTransfers.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure("StockTransferNotFound", "Stock transfer not found.");

        try
        {
            doc.Unapprove();
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ShipStockTransferCommandHandler(
    IApplicationDbContext context,
    IInventoryMovementPipeline pipeline,
    ILogger<ShipStockTransferCommandHandler> logger)
    : IRequestHandler<ShipStockTransferCommand, Result>
{
    public async Task<Result> Handle(ShipStockTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await context.StockTransfers.Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (transfer is null) return Result.Failure("StockTransferNotFound", "Stock transfer not found.");
        if (transfer.Status is not (StockTransferStatus.Approved or StockTransferStatus.Draft))
            return Result.Failure("InvalidStatus", "Only approved (or draft) transfers can be posted/shipped.");
        if (!transfer.Lines.Any()) return Result.Failure("NoLines", "Cannot ship transfer with no lines.");

        if (transfer.Status == StockTransferStatus.Draft)
        {
            try { transfer.Approve(); }
            catch (Domain.Common.Exceptions.BusinessException ex)
            {
                return Result.Failure(ex.ErrorCode, ex.Message);
            }
        }

        var tro = await pipeline.ApplyMovementAsync(new InventoryMovementRequest(
            transfer.TenantId,
            InventoryMovementType.TRO,
            TransactionType.StockTransferOut,
            transfer.Id,
            transfer.TransferNumber,
            transfer.Lines.Select(l => new InventoryMovementLine(
                l.InventoryItemId,
                transfer.SourceWarehouseId,
                l.UnitId,
                l.Quantity,
                l.UnitCost)).ToList(),
            transfer.Notes,
            transfer.TransferDate), cancellationToken);
        if (tro.IsFailure) return Result.Failure(tro.ErrorCode!, tro.ErrorMessage);

        try
        {
            transfer.MarkAsInTransit();
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("StockTransfer posted/shipped (TRO): {Id}", transfer.Id);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class CompleteStockTransferCommandHandler(
    IApplicationDbContext context,
    IInventoryMovementPipeline pipeline,
    ILogger<CompleteStockTransferCommandHandler> logger)
    : IRequestHandler<CompleteStockTransferCommand, Result>
{
    public async Task<Result> Handle(CompleteStockTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await context.StockTransfers.Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (transfer is null) return Result.Failure("StockTransferNotFound", "Stock transfer not found.");
        if (!transfer.Lines.Any()) return Result.Failure("NoLines", "Cannot complete transfer with no lines.");

        if (transfer.Status != StockTransferStatus.InTransit)
            return Result.Failure("InvalidStatus", "Post the transfer before receiving.");

        var tri = await pipeline.ApplyMovementAsync(new InventoryMovementRequest(
            transfer.TenantId,
            InventoryMovementType.TRI,
            TransactionType.StockTransferIn,
            transfer.Id,
            transfer.TransferNumber,
            transfer.Lines.Select(l => new InventoryMovementLine(
                l.InventoryItemId,
                transfer.DestinationWarehouseId,
                l.UnitId,
                l.Quantity,
                l.UnitCost)).ToList(),
            transfer.Notes,
            transfer.TransferDate), cancellationToken);
        if (tri.IsFailure) return Result.Failure(tri.ErrorCode!, tri.ErrorMessage);

        try
        {
            transfer.Complete();
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("StockTransfer received (TRI): {Id}", transfer.Id);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class CancelStockTransferCommandHandler(
    IApplicationDbContext context,
    IInventoryMovementPipeline pipeline,
    ILogger<CancelStockTransferCommandHandler> logger)
    : IRequestHandler<CancelStockTransferCommand, Result>
{
    public async Task<Result> Handle(CancelStockTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await context.StockTransfers.Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (transfer is null) return Result.Failure("StockTransferNotFound", "Stock transfer not found.");
        if (transfer.Status == StockTransferStatus.Cancelled)
            return Result.Failure("AlreadyCancelled", "Transfer is already cancelled.");
        if (transfer.Status == StockTransferStatus.Completed)
            return Result.Failure("InvalidStatus", "Received transfers cannot be cancelled.");

        if (transfer.Status == StockTransferStatus.InTransit)
        {
            var revTro = await pipeline.ApplyMovementAsync(new InventoryMovementRequest(
                transfer.TenantId,
                InventoryMovementType.REV,
                TransactionType.StockTransferOutReversal,
                transfer.Id,
                transfer.TransferNumber,
                transfer.Lines.Select(l => new InventoryMovementLine(
                    l.InventoryItemId,
                    transfer.SourceWarehouseId,
                    l.UnitId,
                    l.Quantity,
                    AdjIncreasesOnHand: true)).ToList(),
                $"Reversal of TRO for {transfer.TransferNumber}"), cancellationToken);
            if (revTro.IsFailure) return Result.Failure(revTro.ErrorCode!, revTro.ErrorMessage);
        }

        try
        {
            transfer.Cancel();
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("StockTransfer cancelled: {Id}", transfer.Id);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class DeleteStockTransferCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteStockTransferCommand, Result>
{
    public async Task<Result> Handle(DeleteStockTransferCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.StockTransfers
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.TenantId == request.TenantId, cancellationToken);
        if (doc is null) return Result.Failure("StockTransferNotFound", "Stock transfer not found.");
        if (doc.Status != StockTransferStatus.Draft)
            return Result.Failure("InvalidStatus", "Only draft transfers can be deleted.");

        doc.SoftDelete(null);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class GenerateStockTransferNumberCommandHandler(IApplicationDbContext context)
    : IRequestHandler<GenerateStockTransferNumberCommand, Result<string>>
{
    public Task<Result<string>> Handle(GenerateStockTransferNumberCommand request, CancellationToken cancellationToken)
        => StockTransferMapper.AllocateDocumentNumberAsync(context, request.TenantId, cancellationToken);
}
