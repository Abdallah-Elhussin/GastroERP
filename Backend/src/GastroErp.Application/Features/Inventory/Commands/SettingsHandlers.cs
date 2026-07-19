using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Domain.Entities.Inventory.Settings;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

internal static class InventorySettingMapper
{
    public static InventorySettingDto ToDto(InventorySetting s) =>
        new(
            s.Id,
            s.TenantId,
            s.CompanyId,
            s.BranchId,
            s.DefaultWarehouseId,
            s.DefaultUnitId,
            s.DefaultCurrencyCode,
            s.AutoGenerateItemCode,
            s.EnableMultiWarehouse,
            s.EnableWarehouseHierarchy,
            s.EnableBatchTracking,
            s.EnableSerialTracking,
            s.EnableExpiryTracking,
            s.EnableBarcode,
            s.EnableQrCode,
            s.CostingMethod,
            s.CostPrecision,
            s.RoundCost,
            s.AutoRecalculateCost,
            s.AllowNegativeStock,
            s.CheckAvailableQuantity,
            s.EnableReservation,
            s.AutoReleaseReservation,
            s.FreezeDuringCount,
            s.AllowZeroCost,
            s.AllowNegativeCost,
            s.ValidateWarehouseBeforePosting,
            s.AutoIssueRecipe,
            s.RequireApprovalBeforePosting,
            s.AutoPostAfterApproval,
            s.AllowUnpost,
            s.CreateReverseEntry,
            s.LockPostedDocuments,
            s.AllowEditDraft,
            s.AllowDeleteDraft,
            s.EnablePurchasingIntegration,
            s.EnablePosIntegration,
            s.EnableProductionIntegration,
            s.EnableAccountingIntegration,
            s.EnableKitchenIntegration,
            s.EnableDeliveryIntegration,
            s.LowStockAlert,
            s.OutOfStockAlert,
            s.NearExpiryAlert,
            s.ExpiredItemsAlert,
            s.CycleCountReminder,
            s.EmailNotifications,
            s.PushNotifications,
            s.EnableMultiCompany,
            s.EnableMultiBranch,
            s.EnableWarehouseZones,
            s.EnableShelves,
            s.EnableBins,
            s.EnableRfid,
            s.EnableMobileScanner,
            s.IsActive,
            (s.UpdatedAt ?? s.CreatedAt).UtcDateTime,
            s.DocumentSeries
                .OrderBy(d => d.DocumentType)
                .Select(d => new InventoryDocumentNumberSeriesDto(
                    d.Id, d.DocumentType, d.Prefix, d.NumberLength, d.NextNumber, d.AutoIncrement))
                .ToList());
}

public sealed class GetInventorySettingQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetInventorySettingQuery, Result<InventorySettingDto>>
{
    public async Task<Result<InventorySettingDto>> Handle(
        GetInventorySettingQuery request,
        CancellationToken cancellationToken)
    {
        var setting = await InventorySettingResolve.ResolveAsync(
            context, request.TenantId, request.BranchId, request.CompanyId, cancellationToken);

        if (setting is null)
        {
            var defaults = new InventorySetting(request.TenantId, request.BranchId, request.CompanyId);
            return Result<InventorySettingDto>.Success(InventorySettingMapper.ToDto(defaults));
        }

        return Result<InventorySettingDto>.Success(InventorySettingMapper.ToDto(setting));
    }
}

public sealed class GetInventorySettingsByCompanyQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetInventorySettingsByCompanyQuery, Result<InventorySettingDto>>
{
    public async Task<Result<InventorySettingDto>> Handle(
        GetInventorySettingsByCompanyQuery request,
        CancellationToken cancellationToken)
    {
        var setting = await context.InventorySettings
            .AsNoTracking()
            .Include(s => s.DocumentSeries)
            .Where(s => s.TenantId == request.TenantId && s.CompanyId == request.CompanyId && s.BranchId == null)
            .FirstOrDefaultAsync(cancellationToken);

        setting ??= await InventorySettingResolve.ResolveAsync(
            context, request.TenantId, null, request.CompanyId, cancellationToken);

        if (setting is null)
        {
            var defaults = new InventorySetting(request.TenantId, null, request.CompanyId);
            return Result<InventorySettingDto>.Success(InventorySettingMapper.ToDto(defaults));
        }

        return Result<InventorySettingDto>.Success(InventorySettingMapper.ToDto(setting));
    }
}

internal static class InventorySettingResolve
{
    public static async Task<InventorySetting?> ResolveAsync(
        IApplicationDbContext context,
        Guid tenantId,
        Guid? branchId,
        Guid? companyId,
        CancellationToken cancellationToken)
    {
        var query = context.InventorySettings
            .AsNoTracking()
            .Include(s => s.DocumentSeries)
            .Where(s => s.TenantId == tenantId);

        if (branchId.HasValue)
        {
            var branchSpecific = await query
                .FirstOrDefaultAsync(s => s.BranchId == branchId, cancellationToken);
            if (branchSpecific is not null) return branchSpecific;
        }

        if (companyId.HasValue)
        {
            var company = await query
                .FirstOrDefaultAsync(s => s.CompanyId == companyId && s.BranchId == null, cancellationToken);
            if (company is not null) return company;
        }

        return await query.FirstOrDefaultAsync(s => s.BranchId == null, cancellationToken);
    }
}

public sealed class UpsertInventorySettingCommandHandler(IApplicationDbContext context, ILogger<UpsertInventorySettingCommandHandler> logger)
    : IRequestHandler<UpsertInventorySettingCommand, Result<InventorySettingDto>>
{
    public Task<Result<InventorySettingDto>> Handle(
        UpsertInventorySettingCommand request,
        CancellationToken cancellationToken)
        => InventorySettingWrite.UpsertAsync(context, request.Dto, logger, cancellationToken);
}

public sealed class UpdateInventorySettingsCommandHandler(IApplicationDbContext context, ILogger<UpdateInventorySettingsCommandHandler> logger)
    : IRequestHandler<UpdateInventorySettingsCommand, Result<InventorySettingDto>>
{
    public Task<Result<InventorySettingDto>> Handle(
        UpdateInventorySettingsCommand request,
        CancellationToken cancellationToken)
        => InventorySettingWrite.UpsertAsync(context, request.Dto, logger, cancellationToken);
}

public sealed class ResetInventorySettingsCommandHandler(IApplicationDbContext context, ILogger<ResetInventorySettingsCommandHandler> logger)
    : IRequestHandler<ResetInventorySettingsCommand, Result<InventorySettingDto>>
{
    public async Task<Result<InventorySettingDto>> Handle(
        ResetInventorySettingsCommand request,
        CancellationToken cancellationToken)
    {
        var setting = await context.InventorySettings
            .Include(s => s.DocumentSeries)
            .FirstOrDefaultAsync(
                s => s.TenantId == request.TenantId && s.BranchId == request.BranchId,
                cancellationToken);

        if (setting is null)
        {
            setting = new InventorySetting(request.TenantId, request.BranchId, request.CompanyId);
            context.InventorySettings.Add(setting);
        }
        else
        {
            setting.ResetToDefaults();
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("InventorySettings reset for Tenant {TenantId} Branch {BranchId}", request.TenantId, request.BranchId);
        return Result<InventorySettingDto>.Success(InventorySettingMapper.ToDto(setting));
    }
}

internal static class InventorySettingWrite
{
    public static async Task<Result<InventorySettingDto>> UpsertAsync(
        IApplicationDbContext context,
        UpsertInventorySettingDto dto,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (dto.TenantId == Guid.Empty)
            return Result<InventorySettingDto>.Failure("InvalidTenant", "TenantId is required.");

        if (!Enum.IsDefined(typeof(InventoryCostingMethod), dto.CostingMethod))
            return Result<InventorySettingDto>.Failure("InvalidCostingMethod", "Costing method is invalid.");

        if (dto.DefaultWarehouseId.HasValue)
        {
            var warehouseOk = await context.Warehouses.AnyAsync(
                w => w.Id == dto.DefaultWarehouseId.Value && w.TenantId == dto.TenantId,
                cancellationToken);
            if (!warehouseOk)
                return Result<InventorySettingDto>.Failure("WarehouseNotFound", "Default warehouse not found for this tenant.");
        }

        if (dto.DefaultUnitId.HasValue)
        {
            var unitOk = await context.InventoryUnits.AnyAsync(
                u => u.Id == dto.DefaultUnitId.Value && u.TenantId == dto.TenantId,
                cancellationToken);
            if (!unitOk)
                return Result<InventorySettingDto>.Failure("UnitNotFound", "Default unit not found for this tenant.");
        }

        var setting = await context.InventorySettings
            .Include(s => s.DocumentSeries)
            .FirstOrDefaultAsync(
                s => s.TenantId == dto.TenantId && s.BranchId == dto.BranchId,
                cancellationToken);

        if (setting is null)
        {
            setting = new InventorySetting(dto.TenantId, dto.BranchId, dto.CompanyId);
            context.InventorySettings.Add(setting);
        }

        var costingChanging = setting.CostingMethod != dto.CostingMethod;
        var allowCostingChange = true;
        if (costingChanging)
        {
            var hasPostedMovements = await context.InventoryTransactions.AnyAsync(
                m => m.TenantId == dto.TenantId,
                cancellationToken);
            allowCostingChange = !hasPostedMovements;
        }

        try
        {
            setting.UpdateGeneral(
                dto.DefaultWarehouseId,
                dto.DefaultUnitId,
                dto.DefaultCurrencyCode,
                dto.AutoGenerateItemCode,
                dto.EnableMultiWarehouse,
                dto.EnableWarehouseHierarchy,
                dto.EnableBatchTracking,
                dto.EnableSerialTracking,
                dto.EnableExpiryTracking,
                dto.EnableBarcode,
                dto.EnableQrCode);

            setting.UpdateCosting(
                dto.CostingMethod,
                dto.CostPrecision,
                dto.RoundCost,
                dto.AutoRecalculateCost,
                allowCostingChange);

            setting.UpdateInventoryControl(
                dto.AllowNegativeStock,
                dto.CheckAvailableQuantity,
                dto.EnableReservation,
                dto.AutoReleaseReservation,
                dto.FreezeDuringCount,
                dto.AllowZeroCost,
                dto.AllowNegativeCost,
                dto.ValidateWarehouseBeforePosting,
                dto.AutoIssueRecipe);

            setting.UpdatePosting(
                dto.RequireApprovalBeforePosting,
                dto.AutoPostAfterApproval,
                dto.AllowUnpost,
                dto.CreateReverseEntry,
                dto.LockPostedDocuments,
                dto.AllowEditDraft,
                dto.AllowDeleteDraft);

            setting.UpdateIntegrations(
                dto.EnablePurchasingIntegration,
                dto.EnablePosIntegration,
                dto.EnableProductionIntegration,
                dto.EnableAccountingIntegration,
                dto.EnableKitchenIntegration,
                dto.EnableDeliveryIntegration);

            setting.UpdateNotifications(
                dto.LowStockAlert,
                dto.OutOfStockAlert,
                dto.NearExpiryAlert,
                dto.ExpiredItemsAlert,
                dto.CycleCountReminder,
                dto.EmailNotifications,
                dto.PushNotifications);

            setting.UpdateAdvanced(
                dto.EnableMultiCompany,
                dto.EnableMultiBranch,
                dto.EnableWarehouseZones,
                dto.EnableShelves,
                dto.EnableBins,
                dto.EnableRfid,
                dto.EnableMobileScanner);

            if (dto.DocumentSeries is { Count: > 0 })
            {
                setting.ReplaceDocumentSeries(dto.DocumentSeries.Select(s =>
                    (s.DocumentType, s.Prefix, s.NumberLength, s.NextNumber, s.AutoIncrement)));
            }

            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "InventorySetting upserted for Tenant {TenantId} Branch {BranchId}",
                dto.TenantId,
                dto.BranchId);

            return Result<InventorySettingDto>.Success(InventorySettingMapper.ToDto(setting));
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result<InventorySettingDto>.Failure("SettingsUpdateFailed", ex.Message);
        }
    }
}
