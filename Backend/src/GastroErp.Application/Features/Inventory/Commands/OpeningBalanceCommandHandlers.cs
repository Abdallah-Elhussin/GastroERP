using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Inventory;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Entities.Inventory.Opening;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

internal static class OpeningBalanceMapper
{
    public static async Task<OpeningBalanceDto> ToDtoAsync(
        IApplicationDbContext db,
        OpeningBalance doc,
        CancellationToken ct)
    {
        string? whName = null;
        if (doc.WarehouseId.HasValue)
        {
            whName = await db.Warehouses.AsNoTracking()
                .Where(w => w.Id == doc.WarehouseId.Value)
                .Select(w => w.NameAr)
                .FirstOrDefaultAsync(ct);
        }

        string? contraName = null;
        if (doc.ContraAccountId.HasValue)
        {
            contraName = await db.ChartOfAccounts.AsNoTracking()
                .Where(a => a.Id == doc.ContraAccountId.Value)
                .Select(a => a.NameAr)
                .FirstOrDefaultAsync(ct);
        }

        string? ccName = null;
        if (doc.CostCenterId.HasValue)
        {
            ccName = await db.CostCenters.AsNoTracking()
                .Where(c => c.Id == doc.CostCenterId.Value)
                .Select(c => c.NameAr)
                .FirstOrDefaultAsync(ct);
        }

        var itemIds = doc.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var unitIds = doc.Lines.Select(l => l.UnitId).Distinct().ToList();
        var whIds = doc.Lines.Select(l => l.WarehouseId).Distinct().ToList();

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

        var warehouses = whIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.Warehouses.AsNoTracking()
                .Where(w => whIds.Contains(w.Id))
                .ToDictionaryAsync(w => w.Id, w => w.NameAr, ct);

        var lines = doc.Lines.Select(l =>
        {
            items.TryGetValue(l.InventoryItemId, out var item);
            return new OpeningBalanceLineDetailDto(
                l.Id,
                l.InventoryItemId,
                item.NameAr,
                item.Sku,
                l.WarehouseId,
                warehouses.GetValueOrDefault(l.WarehouseId),
                l.UnitId,
                units.GetValueOrDefault(l.UnitId),
                l.Quantity,
                l.UnitCost,
                l.BatchNumber,
                l.ExpiryDate,
                l.SerialNumber);
        }).ToList();

        return new OpeningBalanceDto(
            doc.Id,
            doc.TenantId,
            doc.WarehouseId,
            whName,
            doc.DocumentNumber,
            doc.DocumentDate,
            doc.ApprovalDate,
            doc.Notes,
            doc.Status.ToString(),
            (byte)doc.Status,
            doc.EntryMethod.ToString(),
            doc.DisplayMethod.ToString(),
            doc.CostingMethod.ToString(),
            doc.WeightedAverageScope.ToString(),
            doc.UseExpiryDate,
            doc.UseBatchNumbers,
            doc.UseSerialNumbers,
            doc.ContraAccountId,
            contraName,
            doc.CostCenterId,
            ccName,
            doc.IsApproved,
            doc.IsPosted,
            doc.Lines.Count,
            lines,
            doc.CreatedAt.UtcDateTime);
    }
}

public sealed class CreateOpeningBalanceCommandHandler(
    IApplicationDbContext context,
    ILogger<CreateOpeningBalanceCommandHandler> logger)
    : IRequestHandler<CreateOpeningBalanceCommand, Result<OpeningBalanceDto>>
{
    public async Task<Result<OpeningBalanceDto>> Handle(CreateOpeningBalanceCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        try
        {
            var docNumber = dto.DocumentNumber?.Trim();
            if (string.IsNullOrWhiteSpace(docNumber) || dto.AutoGenerateNumber)
            {
                var generated = await AllocateDocumentNumberAsync(context, dto.TenantId, cancellationToken);
                if (generated.IsFailure)
                    return Result<OpeningBalanceDto>.Failure(generated.ErrorCode!, generated.ErrorMessage);
                docNumber = generated.Data;
            }

            if (await context.OpeningBalances.AnyAsync(
                    o => o.TenantId == dto.TenantId && o.DocumentNumber == docNumber, cancellationToken))
                return Result<OpeningBalanceDto>.Failure("DuplicateDocumentNumber", "Document number already exists.");

            if (dto.WarehouseId.HasValue)
            {
                var whOk = await context.Warehouses.AnyAsync(
                    w => w.Id == dto.WarehouseId.Value && w.TenantId == dto.TenantId, cancellationToken);
                if (!whOk) return Result<OpeningBalanceDto>.Failure("WarehouseNotFound", "Warehouse not found.");
            }

            var doc = new OpeningBalance(
                dto.TenantId,
                docNumber!,
                dto.DocumentDate,
                dto.WarehouseId,
                dto.Notes,
                (OpeningBalanceEntryMethod)dto.EntryMethod,
                (OpeningBalanceDisplayMethod)dto.DisplayMethod,
                (InventoryCostingMethod)dto.CostingMethod,
                (WeightedAverageScope)dto.WeightedAverageScope,
                dto.UseExpiryDate,
                dto.UseBatchNumbers,
                dto.UseSerialNumbers,
                dto.ContraAccountId,
                dto.CostCenterId);

            if (dto.Lines is { Count: > 0 })
            {
                foreach (var line in dto.Lines)
                {
                    doc.AddLine(
                        line.InventoryItemId, line.UnitId, line.Quantity, line.UnitCost,
                        line.WarehouseId ?? dto.WarehouseId, line.BatchNumber, line.ExpiryDate, line.SerialNumber);
                }
            }

            context.OpeningBalances.Add(doc);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("OpeningBalance created: {Id} ({Doc})", doc.Id, doc.DocumentNumber);
            return Result<OpeningBalanceDto>.Success(await OpeningBalanceMapper.ToDtoAsync(context, doc, cancellationToken));
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result<OpeningBalanceDto>.Failure("ValidationFailed", ex.Message);
        }
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
            .FirstOrDefault(s => s.DocumentType == InventoryDocumentSeriesType.OpeningBalance);
        if (series is null)
        {
            return Result<string>.Success($"OB{DateTime.UtcNow:yyyyMMddHHmmss}");
        }

        var number = series.AllocateNext();
        await db.SaveChangesAsync(ct);
        return Result<string>.Success(number);
    }
}

public sealed class UpdateOpeningBalanceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateOpeningBalanceCommand, Result<OpeningBalanceDto>>
{
    public async Task<Result<OpeningBalanceDto>> Handle(UpdateOpeningBalanceCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var doc = await context.OpeningBalances.Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.TenantId == dto.TenantId, cancellationToken);
        if (doc is null)
            return Result<OpeningBalanceDto>.Failure("OpeningBalanceNotFound", "Opening balance not found.");

        try
        {
            doc.UpdateHeader(
                dto.DocumentDate,
                dto.WarehouseId,
                dto.Notes,
                (OpeningBalanceEntryMethod)dto.EntryMethod,
                (OpeningBalanceDisplayMethod)dto.DisplayMethod,
                (InventoryCostingMethod)dto.CostingMethod,
                (WeightedAverageScope)dto.WeightedAverageScope,
                dto.UseExpiryDate,
                dto.UseBatchNumbers,
                dto.UseSerialNumbers,
                dto.ContraAccountId,
                dto.CostCenterId);

            if (dto.Lines is not null)
            {
                doc.ClearLines();
                foreach (var line in dto.Lines)
                {
                    doc.AddLine(
                        line.InventoryItemId, line.UnitId, line.Quantity, line.UnitCost,
                        line.WarehouseId ?? dto.WarehouseId, line.BatchNumber, line.ExpiryDate, line.SerialNumber);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            return Result<OpeningBalanceDto>.Success(await OpeningBalanceMapper.ToDtoAsync(context, doc, cancellationToken));
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result<OpeningBalanceDto>.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed class AddOpeningBalanceLineCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AddOpeningBalanceLineCommand, Result>
{
    public async Task<Result> Handle(AddOpeningBalanceLineCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.OpeningBalances.Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == request.OpeningBalanceId, cancellationToken);
        if (doc is null) return Result.Failure("OpeningBalanceNotFound", "Opening balance not found.");

        try
        {
            doc.AddLine(
                request.Dto.InventoryItemId,
                request.Dto.UnitId,
                request.Dto.Quantity,
                request.Dto.UnitCost,
                request.Dto.WarehouseId,
                request.Dto.BatchNumber,
                request.Dto.ExpiryDate,
                request.Dto.SerialNumber);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed class ApproveOpeningBalanceCommandHandler(
    IApplicationDbContext context,
    ILogger<ApproveOpeningBalanceCommandHandler> logger)
    : IRequestHandler<ApproveOpeningBalanceCommand, Result>
{
    public async Task<Result> Handle(ApproveOpeningBalanceCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.OpeningBalances.Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure("OpeningBalanceNotFound", "Opening balance not found.");

        try
        {
            doc.Approve();
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("OpeningBalance approved: {Id}", doc.Id);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UnapproveOpeningBalanceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UnapproveOpeningBalanceCommand, Result>
{
    public async Task<Result> Handle(UnapproveOpeningBalanceCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.OpeningBalances
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure("OpeningBalanceNotFound", "Opening balance not found.");

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

public sealed class PostOpeningBalanceCommandHandler(
    IApplicationDbContext context,
    IInventoryMovementPipeline pipeline,
    ILogger<PostOpeningBalanceCommandHandler> logger)
    : IRequestHandler<PostOpeningBalanceCommand, Result>
{
    public async Task<Result> Handle(PostOpeningBalanceCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.OpeningBalances.Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure("OpeningBalanceNotFound", "Opening balance not found.");
        if (doc.IsPosted) return Result.Failure("AlreadyPosted", "Opening balance is already posted.");
        if (!doc.IsApproved) return Result.Failure("NotApproved", "Approve the document before posting.");
        if (!doc.ContraAccountId.HasValue)
            return Result.Failure("ContraAccountRequired", "Contra account is required for posting.");
        if (!doc.Lines.Any()) return Result.Failure("NoLines", "Cannot post opening balance with no lines.");

        // Group by warehouse for pipeline (one movement batch can mix warehouses via lines)
        var movementLines = doc.Lines.Select(l => new InventoryMovementLine(
            l.InventoryItemId,
            l.WarehouseId,
            l.UnitId,
            l.Quantity,
            l.UnitCost)).ToList();

        var post = await pipeline.ApplyMovementAsync(new InventoryMovementRequest(
            doc.TenantId,
            InventoryMovementType.IN,
            TransactionType.OpeningBalance,
            doc.Id,
            doc.DocumentNumber,
            movementLines,
            doc.Notes), cancellationToken);
        if (post.IsFailure) return Result.Failure(post.ErrorCode!, post.ErrorMessage);

        try
        {
            doc.MarkPosted();
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("OpeningBalance posted: {Id}", doc.Id);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class GenerateOpeningBalanceNumberCommandHandler(IApplicationDbContext context)
    : IRequestHandler<GenerateOpeningBalanceNumberCommand, Result<string>>
{
    public Task<Result<string>> Handle(GenerateOpeningBalanceNumberCommand request, CancellationToken cancellationToken)
        => CreateOpeningBalanceCommandHandler.AllocateDocumentNumberAsync(context, request.TenantId, cancellationToken);
}
