using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Entities.Inventory.Warehouse;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

internal static class WarehouseRules
{
    public static async Task<Result> EnsureUniqueAsync(
        IApplicationDbContext db,
        Guid tenantId,
        Guid? branchId,
        string? code,
        string nameAr,
        Guid? excludeId,
        CancellationToken ct)
    {
        var q = db.Warehouses.Where(w => w.TenantId == tenantId && w.BranchId == branchId);
        if (excludeId.HasValue)
            q = q.Where(w => w.Id != excludeId.Value);

        if (!string.IsNullOrWhiteSpace(code)
            && await q.AnyAsync(w => w.Code == code.Trim().ToUpperInvariant(), ct))
            return Result.Failure("DuplicateWarehouseCode", "Warehouse code must be unique within the branch.");

        if (await q.AnyAsync(w => w.NameAr == nameAr.Trim(), ct))
            return Result.Failure("DuplicateWarehouseName", "Warehouse name must be unique within the branch.");

        return Result.Success();
    }

    public static async Task ClearOtherDefaultsAsync(
        IApplicationDbContext db,
        Guid tenantId,
        Guid? branchId,
        Guid keepId,
        CancellationToken ct)
    {
        var others = await db.Warehouses
            .Where(w => w.TenantId == tenantId
                && w.BranchId == branchId
                && w.Id != keepId
                && w.IsDefault)
            .ToListAsync(ct);
        foreach (var w in others)
            w.MarkAsDefault(false);
    }

    public static async Task<(Guid? TypeId, WarehouseType Legacy)> ResolveTypeAsync(
        IApplicationDbContext db,
        Guid tenantId,
        Guid? warehouseTypeId,
        WarehouseType fallback,
        CancellationToken ct)
    {
        if (warehouseTypeId.HasValue)
        {
            var def = await db.WarehouseTypeDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == warehouseTypeId.Value && t.TenantId == tenantId, ct);
            if (def is null)
                return (null, fallback);
            return (def.Id, def.ToLegacyEnum() ?? fallback);
        }

        var byCode = fallback switch
        {
            WarehouseType.DryStore => "DRYSTORE",
            WarehouseType.RawMaterial => "RAW",
            WarehouseType.FinishedGoods => "FINISHED",
            _ => fallback.ToString().ToUpperInvariant()
        };

        var match = await db.WarehouseTypeDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Code == byCode, ct);
        return (match?.Id, fallback);
    }
}

public sealed class CreateWarehouseV2CommandHandler(
    IApplicationDbContext context,
    IMapper mapper,
    ILogger<CreateWarehouseV2CommandHandler> logger)
    : IRequestHandler<CreateWarehouseCommand, Result<WarehouseDto>>
{
    public async Task<Result<WarehouseDto>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var unique = await WarehouseRules.EnsureUniqueAsync(
            context, dto.TenantId, dto.BranchId, dto.Code, dto.NameAr, null, cancellationToken);
        if (unique.IsFailure) return Result<WarehouseDto>.Failure(unique.ErrorCode!, unique.ErrorMessage);

        var (typeId, legacy) = await WarehouseRules.ResolveTypeAsync(
            context, dto.TenantId, dto.WarehouseTypeId, dto.WarehouseType, cancellationToken);

        var wh = new Warehouse(
            dto.TenantId,
            dto.NameAr,
            dto.NameEn,
            dto.Code,
            dto.BranchId,
            legacy,
            dto.CompanyId,
            typeId ?? dto.WarehouseTypeId,
            dto.ParentWarehouseId);

        wh.UpdateInfo(dto.NameAr, dto.NameEn, dto.Code, dto.Address, dto.Phone, dto.Email, dto.Notes, dto.BranchId, dto.CompanyId);
        wh.AssignStaff(dto.ManagerUserId, dto.ResponsibleEmployeeId);
        wh.SetPermissions(
            dto.AllowPurchase, dto.AllowSales, dto.AllowTransfer, dto.AllowInventoryCount, dto.AllowManufacturing,
            dto.AllowNegativeStock, dto.AllowReservation, dto.AllowReceiving, dto.AllowIssue, dto.AllowAdjustment);
        wh.SetFlags(dto.IsPosWarehouse || legacy == WarehouseType.POS, dto.IsDefault, dto.UseBins);

        context.Warehouses.Add(wh);
        await context.SaveChangesAsync(cancellationToken);

        if (dto.IsDefault)
        {
            await WarehouseRules.ClearOtherDefaultsAsync(context, dto.TenantId, dto.BranchId, wh.Id, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Warehouse created: {Id}", wh.Id);
        return Result<WarehouseDto>.Success(mapper.Map<WarehouseDto>(wh));
    }
}

public sealed class UpdateWarehouseV2CommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateWarehouseCommand, Result>
{
    public async Task<Result> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var wh = await context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (wh is null) return Result.Failure("WarehouseNotFound", "Warehouse not found.");

        var dto = request.Dto;
        var unique = await WarehouseRules.EnsureUniqueAsync(
            context, wh.TenantId, dto.BranchId, dto.Code, dto.NameAr, wh.Id, cancellationToken);
        if (unique.IsFailure) return unique;

        var (typeId, legacy) = await WarehouseRules.ResolveTypeAsync(
            context, wh.TenantId, dto.WarehouseTypeId, dto.WarehouseType, cancellationToken);

        wh.UpdateInfo(dto.NameAr, dto.NameEn, dto.Code, dto.Address, dto.Phone, dto.Email, dto.Notes, dto.BranchId, dto.CompanyId);
        wh.SetWarehouseType(typeId ?? dto.WarehouseTypeId, legacy);
        wh.SetParent(dto.ParentWarehouseId);
        wh.AssignStaff(dto.ManagerUserId, dto.ResponsibleEmployeeId);
        wh.SetPermissions(
            dto.AllowPurchase, dto.AllowSales, dto.AllowTransfer, dto.AllowInventoryCount, dto.AllowManufacturing,
            dto.AllowNegativeStock, dto.AllowReservation, dto.AllowReceiving, dto.AllowIssue, dto.AllowAdjustment);
        wh.SetFlags(dto.IsPosWarehouse, dto.IsDefault, dto.UseBins);
        if (dto.IsActive) wh.Activate();
        else wh.Deactivate();

        if (dto.IsDefault)
            await WarehouseRules.ClearOtherDefaultsAsync(context, wh.TenantId, dto.BranchId, wh.Id, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeleteWarehouseCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteWarehouseCommand, Result>
{
    public async Task<Result> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var wh = await context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (wh is null) return Result.Failure("WarehouseNotFound", "Warehouse not found.");
        if (wh.IsSystem)
            return Result.Failure("CannotDeleteSystemWarehouse", "System warehouses cannot be deleted. Deactivate instead.");

        var hasBalance = await context.InventoryBalances.AnyAsync(
            b => b.WarehouseId == request.Id && b.QtyOnHand != 0, cancellationToken);
        if (hasBalance)
            return Result.Failure("WarehouseHasStock", "Cannot delete a warehouse with stock balance. Deactivate instead.");

        var hasMovements = await context.StockMovements.AnyAsync(m => m.WarehouseId == request.Id, cancellationToken);
        if (hasMovements)
            return Result.Failure("WarehouseHasMovements", "Cannot delete a warehouse with stock movements. Deactivate instead.");

        var hasChildren = await context.Warehouses.AnyAsync(w => w.ParentWarehouseId == request.Id, cancellationToken);
        if (hasChildren)
            return Result.Failure("WarehouseHasChildren", "Cannot delete a parent warehouse. Deactivate instead.");

        try
        {
            wh.SoftDeleteWarehouse("system");
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class CreateWarehouseTypeDefinitionCommandHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<CreateWarehouseTypeDefinitionCommand, Result<WarehouseTypeDefinitionDto>>
{
    public async Task<Result<WarehouseTypeDefinitionDto>> Handle(
        CreateWarehouseTypeDefinitionCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var code = dto.Code.Trim().ToUpperInvariant();
        if (await context.WarehouseTypeDefinitions.AnyAsync(
                t => t.TenantId == dto.TenantId && t.Code == code, cancellationToken))
            return Result<WarehouseTypeDefinitionDto>.Failure("DuplicateWarehouseTypeCode", "Warehouse type code must be unique.");

        var entity = new WarehouseTypeDefinition(
            dto.TenantId, code, dto.NameAr, dto.NameEn, dto.Description, dto.SortOrder, isSystem: false);
        context.WarehouseTypeDefinitions.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return Result<WarehouseTypeDefinitionDto>.Success(mapper.Map<WarehouseTypeDefinitionDto>(entity));
    }
}

public sealed class UpdateWarehouseTypeDefinitionCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateWarehouseTypeDefinitionCommand, Result>
{
    public async Task<Result> Handle(UpdateWarehouseTypeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.WarehouseTypeDefinitions
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("WarehouseTypeNotFound", "Warehouse type not found.");
        entity.Update(request.Dto.NameAr, request.Dto.NameEn, request.Dto.Description, request.Dto.SortOrder);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class ActivateWarehouseTypeDefinitionCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ActivateWarehouseTypeDefinitionCommand, Result>
{
    public async Task<Result> Handle(ActivateWarehouseTypeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.WarehouseTypeDefinitions
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("WarehouseTypeNotFound", "Warehouse type not found.");
        if (request.IsActive) entity.Activate();
        else entity.Deactivate();
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
