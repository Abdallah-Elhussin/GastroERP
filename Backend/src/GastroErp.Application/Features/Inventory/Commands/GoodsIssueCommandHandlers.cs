using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Inventory;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Entities.Inventory.Issuing;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

internal static class GoodsIssueMapper
{
    public static async Task<GoodsIssueDto> ToDtoAsync(
        IApplicationDbContext db,
        GoodsIssue doc,
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

        string? destName = null;
        if (doc.IssueDestinationId.HasValue)
        {
            destName = await db.IssueDestinations.AsNoTracking()
                .Where(d => d.Id == doc.IssueDestinationId.Value)
                .Select(d => d.NameAr)
                .FirstOrDefaultAsync(ct);
        }

        var itemIds = doc.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var unitIds = doc.Lines.Select(l => l.UnitId).Distinct().ToList();
        var whIds = doc.Lines.Select(l => l.WarehouseId).Distinct().ToList();
        var ccIds = doc.Lines.Where(l => l.CostCenterId.HasValue).Select(l => l.CostCenterId!.Value).Distinct().ToList();

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

        var costCenters = ccIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.CostCenters.AsNoTracking()
                .Where(c => ccIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.NameAr, ct);

        var lines = doc.Lines.Select(l =>
        {
            items.TryGetValue(l.InventoryItemId, out var item);
            return new GoodsIssueLineDetailDto(
                l.Id,
                l.InventoryItemId,
                item.NameAr,
                item.Sku,
                l.UnitId,
                units.GetValueOrDefault(l.UnitId),
                l.WarehouseId,
                warehouses.GetValueOrDefault(l.WarehouseId),
                l.Quantity,
                l.UnitCost,
                l.TotalCost,
                l.CostCenterId,
                l.CostCenterId.HasValue ? costCenters.GetValueOrDefault(l.CostCenterId.Value) : null,
                l.Notes);
        }).ToList();

        return new GoodsIssueDto(
            doc.Id,
            doc.TenantId,
            doc.WarehouseId,
            whName,
            doc.IssueDestinationId,
            destName,
            doc.IssueNumber,
            doc.IssueDate,
            doc.ApprovalDate,
            doc.Currency,
            doc.Notes,
            doc.Status.ToString(),
            (byte)doc.Status,
            doc.IsConfirmed,
            doc.IsCompleted,
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
            .FirstOrDefault(s => s.DocumentType == InventoryDocumentSeriesType.GoodsIssue);
        if (series is null)
            return Result<string>.Success($"GI{DateTime.UtcNow:yyyyMMddHHmmss}");

        var number = series.AllocateNext();
        await db.SaveChangesAsync(ct);
        return Result<string>.Success(number);
    }
}

public sealed class CreateGoodsIssueCommandHandler(
    IApplicationDbContext context,
    ILogger<CreateGoodsIssueCommandHandler> logger)
    : IRequestHandler<CreateGoodsIssueCommand, Result<GoodsIssueDto>>
{
    public async Task<Result<GoodsIssueDto>> Handle(CreateGoodsIssueCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        try
        {
            var issueNumber = dto.IssueNumber?.Trim();
            if (string.IsNullOrWhiteSpace(issueNumber) || dto.AutoGenerateNumber)
            {
                var generated = await GoodsIssueMapper.AllocateDocumentNumberAsync(context, dto.TenantId, cancellationToken);
                if (generated.IsFailure)
                    return Result<GoodsIssueDto>.Failure(generated.ErrorCode!, generated.ErrorMessage);
                issueNumber = generated.Data;
            }

            if (await context.GoodsIssues.AnyAsync(
                    g => g.TenantId == dto.TenantId && g.IssueNumber == issueNumber, cancellationToken))
                return Result<GoodsIssueDto>.Failure("DuplicateIssueNumber", "Issue number already exists.");

            if (dto.WarehouseId.HasValue)
            {
                var whOk = await context.Warehouses.AnyAsync(
                    w => w.Id == dto.WarehouseId.Value && w.TenantId == dto.TenantId, cancellationToken);
                if (!whOk) return Result<GoodsIssueDto>.Failure("WarehouseNotFound", "Warehouse not found.");
            }

            if (dto.IssueDestinationId.HasValue)
            {
                var destOk = await context.IssueDestinations.AnyAsync(
                    d => d.Id == dto.IssueDestinationId.Value && d.TenantId == dto.TenantId, cancellationToken);
                if (!destOk) return Result<GoodsIssueDto>.Failure("IssueDestinationNotFound", "Issue destination not found.");
            }

            var doc = new GoodsIssue(
                dto.TenantId,
                issueNumber!,
                dto.IssueDate,
                dto.WarehouseId,
                dto.IssueDestinationId,
                dto.Currency,
                dto.Notes);

            if (dto.Lines is { Count: > 0 })
            {
                foreach (var line in dto.Lines)
                {
                    doc.AddLine(
                        line.InventoryItemId, line.UnitId, line.Quantity, line.UnitCost,
                        line.WarehouseId ?? dto.WarehouseId, line.CostCenterId, line.Notes);
                }
            }

            context.GoodsIssues.Add(doc);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("GoodsIssue created: {Id} ({Num})", doc.Id, doc.IssueNumber);
            return Result<GoodsIssueDto>.Success(await GoodsIssueMapper.ToDtoAsync(context, doc, cancellationToken));
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result<GoodsIssueDto>.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed class UpdateGoodsIssueCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateGoodsIssueCommand, Result<GoodsIssueDto>>
{
    public async Task<Result<GoodsIssueDto>> Handle(UpdateGoodsIssueCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var doc = await context.GoodsIssues.Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == request.Id && g.TenantId == dto.TenantId, cancellationToken);
        if (doc is null)
            return Result<GoodsIssueDto>.Failure("GoodsIssueNotFound", "Goods issue not found.");

        try
        {
            if (dto.WarehouseId.HasValue)
            {
                var whOk = await context.Warehouses.AnyAsync(
                    w => w.Id == dto.WarehouseId.Value && w.TenantId == dto.TenantId, cancellationToken);
                if (!whOk) return Result<GoodsIssueDto>.Failure("WarehouseNotFound", "Warehouse not found.");
            }

            if (dto.IssueDestinationId.HasValue)
            {
                var destOk = await context.IssueDestinations.AnyAsync(
                    d => d.Id == dto.IssueDestinationId.Value && d.TenantId == dto.TenantId, cancellationToken);
                if (!destOk) return Result<GoodsIssueDto>.Failure("IssueDestinationNotFound", "Issue destination not found.");
            }

            doc.UpdateHeader(dto.IssueDate, dto.WarehouseId, dto.IssueDestinationId, dto.Currency, dto.Notes);

            if (dto.Lines is not null)
            {
                doc.ClearLines();
                foreach (var line in dto.Lines)
                {
                    doc.AddLine(
                        line.InventoryItemId, line.UnitId, line.Quantity, line.UnitCost,
                        line.WarehouseId ?? dto.WarehouseId, line.CostCenterId, line.Notes);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            return Result<GoodsIssueDto>.Success(await GoodsIssueMapper.ToDtoAsync(context, doc, cancellationToken));
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result<GoodsIssueDto>.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed class AddGoodsIssueLineCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AddGoodsIssueLineCommand, Result>
{
    public async Task<Result> Handle(AddGoodsIssueLineCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.GoodsIssues.Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == request.GoodsIssueId, cancellationToken);
        if (doc is null) return Result.Failure("GoodsIssueNotFound", "Goods issue not found.");

        try
        {
            doc.AddLine(
                request.Dto.InventoryItemId,
                request.Dto.UnitId,
                request.Dto.Quantity,
                request.Dto.UnitCost,
                request.Dto.WarehouseId,
                request.Dto.CostCenterId,
                request.Dto.Notes);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed class ApproveGoodsIssueCommandHandler(
    IApplicationDbContext context,
    ILogger<ApproveGoodsIssueCommandHandler> logger)
    : IRequestHandler<ApproveGoodsIssueCommand, Result>
{
    public async Task<Result> Handle(ApproveGoodsIssueCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.GoodsIssues.Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure("GoodsIssueNotFound", "Goods issue not found.");

        try
        {
            doc.Approve();
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("GoodsIssue approved: {Id}", doc.Id);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UnapproveGoodsIssueCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UnapproveGoodsIssueCommand, Result>
{
    public async Task<Result> Handle(UnapproveGoodsIssueCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.GoodsIssues.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure("GoodsIssueNotFound", "Goods issue not found.");

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

public sealed class PostGoodsIssueCommandHandler(
    IApplicationDbContext context,
    IInventoryMovementPipeline pipeline,
    ILogger<PostGoodsIssueCommandHandler> logger)
    : IRequestHandler<PostGoodsIssueCommand, Result>
{
    public async Task<Result> Handle(PostGoodsIssueCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.GoodsIssues.Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure("GoodsIssueNotFound", "Goods issue not found.");
        if (doc.IsCompleted) return Result.Failure("AlreadyPosted", "Goods issue is already posted.");
        if (doc.Status != GoodsIssueStatus.Approved)
            return Result.Failure("NotApproved", "Approve the document before posting.");
        if (!doc.Lines.Any()) return Result.Failure("NoLines", "Cannot post goods issue with no lines.");

        var movementLines = doc.Lines.Select(l => new InventoryMovementLine(
            l.InventoryItemId,
            l.WarehouseId,
            l.UnitId,
            l.Quantity,
            l.UnitCost)).ToList();

        var post = await pipeline.ApplyMovementAsync(new InventoryMovementRequest(
            doc.TenantId,
            InventoryMovementType.OUT,
            TransactionType.GoodsIssue,
            doc.Id,
            doc.IssueNumber,
            movementLines,
            doc.Notes,
            doc.IssueDate), cancellationToken);
        if (post.IsFailure) return Result.Failure(post.ErrorCode!, post.ErrorMessage);

        try
        {
            doc.MarkPosted();
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("GoodsIssue posted: {Id}", doc.Id);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class CancelGoodsIssueCommandHandler(
    IApplicationDbContext context,
    ILogger<CancelGoodsIssueCommandHandler> logger)
    : IRequestHandler<CancelGoodsIssueCommand, Result>
{
    public async Task<Result> Handle(CancelGoodsIssueCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.GoodsIssues.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (doc is null) return Result.Failure("GoodsIssueNotFound", "Goods issue not found.");

        try
        {
            doc.Cancel();
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("GoodsIssue cancelled: {Id}", doc.Id);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

/// <summary>Legacy: Approve (if Draft) then Post.</summary>
public sealed class ConfirmGoodsIssueCommandHandler(IMediator mediator)
    : IRequestHandler<ConfirmGoodsIssueCommand, Result>
{
    public async Task<Result> Handle(ConfirmGoodsIssueCommand request, CancellationToken cancellationToken)
    {
        var approve = await mediator.Send(new ApproveGoodsIssueCommand(request.Id), cancellationToken);
        if (approve.IsFailure && approve.ErrorCode != Domain.Common.Localization.ErrorCodes.InvalidStatusTransition)
            return approve;

        return await mediator.Send(new PostGoodsIssueCommand(request.Id), cancellationToken);
    }
}

public sealed class GenerateGoodsIssueNumberCommandHandler(IApplicationDbContext context)
    : IRequestHandler<GenerateGoodsIssueNumberCommand, Result<string>>
{
    public Task<Result<string>> Handle(GenerateGoodsIssueNumberCommand request, CancellationToken cancellationToken)
        => GoodsIssueMapper.AllocateDocumentNumberAsync(context, request.TenantId, cancellationToken);
}
