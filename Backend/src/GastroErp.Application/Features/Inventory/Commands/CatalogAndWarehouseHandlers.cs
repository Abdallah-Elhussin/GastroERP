using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Domain.Entities.Inventory.Catalog;
using GastroErp.Domain.Entities.Inventory.Suppliers;
using GastroErp.Domain.Entities.Inventory.Warehouse;
using GastroErp.Domain.Entities.Inventory.Settings;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

// ─── InventoryCategory Handlers ───────────────────────────────────────────────

public class CreateInventoryCategoryCommandHandler : IRequestHandler<CreateInventoryCategoryCommand, Result<InventoryCategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateInventoryCategoryCommandHandler> _logger;

    public CreateInventoryCategoryCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateInventoryCategoryCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<InventoryCategoryDto>> Handle(CreateInventoryCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.Dto.ParentCategoryId.HasValue)
        {
            var parentExists = await _context.InventoryCategories.AnyAsync(
                c => c.Id == request.Dto.ParentCategoryId.Value && c.TenantId == request.Dto.TenantId,
                cancellationToken);
            if (!parentExists)
                return Result<InventoryCategoryDto>.Failure("CategoryNotFound", "Parent category not found.");
        }

        var cat = new InventoryCategory(
            request.Dto.TenantId,
            request.Dto.NameAr,
            request.Dto.NameEn,
            request.Dto.ParentCategoryId,
            request.Dto.Code);
        cat.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.DescriptionAr, request.Dto.DescriptionEn, request.Dto.Code);
        cat.SetVisuals(request.Dto.Icon, request.Dto.ImageUrl, request.Dto.Color);

        var sortOrder = request.Dto.SortOrder;
        if (sortOrder <= 0)
        {
            var max = await _context.InventoryCategories
                .Where(c => c.TenantId == request.Dto.TenantId)
                .Select(c => (int?)c.SortOrder)
                .MaxAsync(cancellationToken) ?? 0;
            sortOrder = max + 1;
        }
        cat.SetSortOrder(sortOrder);

        _context.InventoryCategories.Add(cat);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("InventoryCategory created: {Id}", cat.Id);
        return Result<InventoryCategoryDto>.Success(_mapper.Map<InventoryCategoryDto>(cat));
    }
}

public class UpdateInventoryCategoryCommandHandler : IRequestHandler<UpdateInventoryCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateInventoryCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateInventoryCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = await _context.InventoryCategories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (cat == null) return Result.Failure("CategoryNotFound", "Inventory category not found.");

        if (request.Dto.ParentCategoryId.HasValue)
        {
            if (request.Dto.ParentCategoryId.Value == request.Id)
                return Result.Failure("InvalidParent", "Category cannot be its own parent.");
            var parentExists = await _context.InventoryCategories.AnyAsync(
                c => c.Id == request.Dto.ParentCategoryId.Value && c.TenantId == cat.TenantId,
                cancellationToken);
            if (!parentExists)
                return Result.Failure("CategoryNotFound", "Parent category not found.");
        }

        cat.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.DescriptionAr, request.Dto.DescriptionEn, request.Dto.Code);
        cat.SetParent(request.Dto.ParentCategoryId);
        cat.SetVisuals(request.Dto.Icon, request.Dto.ImageUrl, request.Dto.Color);
        cat.SetSortOrder(request.Dto.SortOrder);
        _context.InventoryCategories.Update(cat);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeactivateInventoryCategoryCommandHandler : IRequestHandler<DeactivateInventoryCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateInventoryCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateInventoryCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = await _context.InventoryCategories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (cat == null) return Result.Failure("CategoryNotFound", "Inventory category not found.");
        cat.Deactivate();
        _context.InventoryCategories.Update(cat);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ActivateInventoryCategoryCommandHandler : IRequestHandler<ActivateInventoryCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateInventoryCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateInventoryCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = await _context.InventoryCategories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (cat == null) return Result.Failure("CategoryNotFound", "Inventory category not found.");
        cat.Activate();
        _context.InventoryCategories.Update(cat);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeleteInventoryCategoryCommandHandler : IRequestHandler<DeleteInventoryCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteInventoryCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteInventoryCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = await _context.InventoryCategories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (cat == null) return Result.Failure("CategoryNotFound", "Inventory category not found.");

        var hasChildren = await _context.InventoryCategories
            .AnyAsync(c => c.ParentCategoryId == request.Id, cancellationToken);
        if (hasChildren)
            return Result.Failure("CategoryHasChildren", "Cannot delete a category that has sub-categories. Deactivate instead.");

        var inUse = await _context.InventoryItems
            .AnyAsync(i => i.CategoryId == request.Id, cancellationToken);
        if (inUse)
            return Result.Failure("CategoryInUse", "Cannot delete a category used by inventory items. Deactivate instead.");

        cat.SoftDeleteCategory("system");
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── InventoryUnit Handlers ───────────────────────────────────────────────────

public class CreateInventoryUnitCommandHandler : IRequestHandler<CreateInventoryUnitCommand, Result<InventoryUnitDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateInventoryUnitCommandHandler> _logger;

    public CreateInventoryUnitCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateInventoryUnitCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<InventoryUnitDto>> Handle(CreateInventoryUnitCommand request, CancellationToken cancellationToken)
    {
        if (request.Dto.BaseUnitId.HasValue)
        {
            var baseExists = await _context.InventoryUnits.AnyAsync(
                u => u.Id == request.Dto.BaseUnitId.Value && u.TenantId == request.Dto.TenantId,
                cancellationToken);
            if (!baseExists)
                return Result<InventoryUnitDto>.Failure("UnitNotFound", "Base unit not found.");
        }

        var unit = new InventoryUnit(
            request.Dto.TenantId,
            request.Dto.NameAr,
            request.Dto.Symbol,
            request.Dto.NameEn,
            request.Dto.SymbolAr,
            request.Dto.Code,
            request.Dto.DecimalPlaces,
            request.Dto.BaseUnitId,
            request.Dto.ConversionFactor <= 0 ? 1m : request.Dto.ConversionFactor,
            request.Dto.UnitType,
            request.Dto.Classification,
            request.Dto.SortOrder);

        if (request.Dto.SortOrder <= 0)
        {
            var max = await _context.InventoryUnits
                .Where(u => u.TenantId == request.Dto.TenantId)
                .Select(u => (int?)u.SortOrder)
                .MaxAsync(cancellationToken) ?? 0;
            unit.SetSortOrder(max + 1);
        }

        if (!request.Dto.IsActive)
            unit.Deactivate();

        _context.InventoryUnits.Add(unit);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("InventoryUnit created: {Id}", unit.Id);
        return Result<InventoryUnitDto>.Success(_mapper.Map<InventoryUnitDto>(unit));
    }
}

public class UpdateInventoryUnitCommandHandler : IRequestHandler<UpdateInventoryUnitCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateInventoryUnitCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateInventoryUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _context.InventoryUnits.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (unit == null) return Result.Failure("UnitNotFound", "Inventory unit not found.");

        if (request.Dto.BaseUnitId.HasValue)
        {
            if (request.Dto.BaseUnitId.Value == request.Id)
                return Result.Failure("InvalidBaseUnit", "Unit cannot reference itself as base unit.");
            var baseExists = await _context.InventoryUnits.AnyAsync(
                u => u.Id == request.Dto.BaseUnitId.Value && u.TenantId == unit.TenantId,
                cancellationToken);
            if (!baseExists)
                return Result.Failure("UnitNotFound", "Base unit not found.");
        }

        unit.UpdateInfo(
            request.Dto.NameAr,
            request.Dto.Symbol,
            request.Dto.NameEn,
            request.Dto.SymbolAr,
            request.Dto.Code,
            request.Dto.DecimalPlaces);
        unit.SetBaseUnit(request.Dto.BaseUnitId);
        unit.SetConversionFactor(request.Dto.ConversionFactor <= 0 ? 1m : request.Dto.ConversionFactor);
        unit.SetMeasurementProfile(request.Dto.UnitType, request.Dto.Classification);
        if (request.Dto.SortOrder > 0)
            unit.SetSortOrder(request.Dto.SortOrder);
        if (request.Dto.IsActive) unit.Activate();
        else unit.Deactivate();

        _context.InventoryUnits.Update(unit);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeactivateInventoryUnitCommandHandler : IRequestHandler<DeactivateInventoryUnitCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateInventoryUnitCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateInventoryUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _context.InventoryUnits.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (unit == null) return Result.Failure("UnitNotFound", "Inventory unit not found.");
        unit.Deactivate();
        _context.InventoryUnits.Update(unit);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ActivateInventoryUnitCommandHandler : IRequestHandler<ActivateInventoryUnitCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateInventoryUnitCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateInventoryUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _context.InventoryUnits.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (unit == null) return Result.Failure("UnitNotFound", "Inventory unit not found.");
        unit.Activate();
        _context.InventoryUnits.Update(unit);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeleteInventoryUnitCommandHandler : IRequestHandler<DeleteInventoryUnitCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteInventoryUnitCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteInventoryUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _context.InventoryUnits.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (unit == null) return Result.Failure("UnitNotFound", "Inventory unit not found.");

        var referencedAsBase = await _context.InventoryUnits
            .AnyAsync(u => u.BaseUnitId == request.Id, cancellationToken);
        if (referencedAsBase)
            return Result.Failure("UnitInUse", "Cannot delete a unit used as a base unit. Deactivate instead.");

        var usedByItems = await _context.InventoryItems
            .AnyAsync(i => i.BaseUnitId == request.Id
                || i.DefaultPurchaseUnitId == request.Id
                || i.DefaultRecipeUnitId == request.Id, cancellationToken);
        if (usedByItems)
            return Result.Failure("UnitInUse", "Cannot delete a unit used by inventory items. Deactivate instead.");

        unit.SoftDeleteUnit("system");
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CreateUnitConversionCommandHandler : IRequestHandler<CreateUnitConversionCommand, Result<UnitConversionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateUnitConversionCommandHandler> _logger;

    public CreateUnitConversionCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateUnitConversionCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<UnitConversionDto>> Handle(CreateUnitConversionCommand request, CancellationToken cancellationToken)
    {
        var conv = new UnitConversion(request.Dto.TenantId, request.Dto.FromUnitId, request.Dto.ToUnitId, request.Dto.Factor);
        _context.UnitConversions.Add(conv);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("UnitConversion created: {Id}", conv.Id);
        return Result<UnitConversionDto>.Success(_mapper.Map<UnitConversionDto>(conv));
    }
}

// ─── InventoryItem Handlers ───────────────────────────────────────────────────

public class CreateInventoryItemCommandHandler : IRequestHandler<CreateInventoryItemCommand, Result<InventoryItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateInventoryItemCommandHandler> _logger;

    public CreateInventoryItemCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateInventoryItemCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<InventoryItemDto>> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var catExists = await _context.InventoryCategories.AnyAsync(c => c.Id == request.Dto.CategoryId, cancellationToken);
        if (!catExists) return Result<InventoryItemDto>.Failure("CategoryNotFound", "Inventory category not found.");

        var item = new InventoryItem(request.Dto.TenantId, request.Dto.CategoryId, request.Dto.NameAr, request.Dto.BaseUnitId,
            request.Dto.NameEn, request.Dto.Sku, request.Dto.Barcode, request.Dto.ItemKind, request.Dto.ImageUrl);
        if (!string.IsNullOrWhiteSpace(request.Dto.DescriptionAr) || !string.IsNullOrWhiteSpace(request.Dto.DescriptionEn))
            item.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.DescriptionAr, request.Dto.DescriptionEn,
                request.Dto.Sku, request.Dto.Barcode, request.Dto.ItemKind, request.Dto.ImageUrl);
        if (request.Dto.DefaultPurchaseUnitId.HasValue || request.Dto.DefaultRecipeUnitId.HasValue)
            item.SetUnits(request.Dto.DefaultPurchaseUnitId, request.Dto.DefaultRecipeUnitId);
        if (request.Dto.ReorderLevel > 0 || request.Dto.ReorderQuantity > 0)
            item.SetReorderInfo(request.Dto.ReorderLevel, request.Dto.ReorderQuantity);
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("InventoryItem created: {Id}", item.Id);
        var projected = await InventoryItemDtoProjector.ProjectAsync(_context, [item], cancellationToken);
        return Result<InventoryItemDto>.Success(projected[0]);
    }
}

public class UpdateInventoryItemCommandHandler : IRequestHandler<UpdateInventoryItemCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateInventoryItemCommandHandler> _logger;

    public UpdateInventoryItemCommandHandler(IApplicationDbContext context, ILogger<UpdateInventoryItemCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (item == null) return Result.Failure("ItemNotFound", "Inventory item not found.");
        item.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.DescriptionAr, request.Dto.DescriptionEn,
            request.Dto.Sku, request.Dto.Barcode, request.Dto.ItemKind, request.Dto.ImageUrl);
        if (request.Dto.CategoryId.HasValue)
        {
            var catExists = await _context.InventoryCategories.AnyAsync(c => c.Id == request.Dto.CategoryId.Value, cancellationToken);
            if (!catExists) return Result.Failure("CategoryNotFound", "Inventory category not found.");
            item.SetCategory(request.Dto.CategoryId.Value);
        }
        if (request.Dto.BaseUnitId.HasValue && request.Dto.BaseUnitId.Value != Guid.Empty)
        {
            var unitExists = await _context.InventoryUnits.AnyAsync(u => u.Id == request.Dto.BaseUnitId.Value, cancellationToken);
            if (!unitExists) return Result.Failure("UnitNotFound", "Inventory unit not found.");
            item.SetBaseUnit(request.Dto.BaseUnitId.Value);
        }
        item.SetUnits(request.Dto.DefaultPurchaseUnitId, request.Dto.DefaultRecipeUnitId);
        if (request.Dto.ReorderLevel.HasValue && request.Dto.ReorderQuantity.HasValue)
            item.SetReorderInfo(request.Dto.ReorderLevel.Value, request.Dto.ReorderQuantity.Value);
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("InventoryItem updated: {Id}", item.Id);
        return Result.Success();
    }
}

public class SetInventoryItemReorderInfoCommandHandler : IRequestHandler<SetInventoryItemReorderInfoCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetInventoryItemReorderInfoCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SetInventoryItemReorderInfoCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (item == null) return Result.Failure("ItemNotFound", "Inventory item not found.");
        item.SetReorderInfo(request.Dto.ReorderLevel, request.Dto.ReorderQuantity);
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SetInventoryItemUnitsCommandHandler : IRequestHandler<SetInventoryItemUnitsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetInventoryItemUnitsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SetInventoryItemUnitsCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (item == null) return Result.Failure("ItemNotFound", "Inventory item not found.");
        item.SetUnits(request.PurchaseUnitId, request.RecipeUnitId);
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SetInventoryItemCategoryCommandHandler : IRequestHandler<SetInventoryItemCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetInventoryItemCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SetInventoryItemCategoryCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (item == null) return Result.Failure("ItemNotFound", "Inventory item not found.");
        var catExists = await _context.InventoryCategories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!catExists) return Result.Failure("CategoryNotFound", "Inventory category not found.");
        item.SetCategory(request.CategoryId);
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeactivateInventoryItemCommandHandler : IRequestHandler<DeactivateInventoryItemCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateInventoryItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (item == null) return Result.Failure("ItemNotFound", "Inventory item not found.");
        item.Deactivate();
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ActivateInventoryItemCommandHandler : IRequestHandler<ActivateInventoryItemCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateInventoryItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (item == null) return Result.Failure("ItemNotFound", "Inventory item not found.");
        item.Activate();
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── Warehouse Handlers ───────────────────────────────────────────────────────

// Create/Update warehouse handlers moved to WarehouseManagementHandlers.cs

public class AddWarehouseZoneCommandHandler : IRequestHandler<AddWarehouseZoneCommand, Result<WarehouseZoneDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AddWarehouseZoneCommandHandler> _logger;

    public AddWarehouseZoneCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<AddWarehouseZoneCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<WarehouseZoneDto>> Handle(AddWarehouseZoneCommand request, CancellationToken cancellationToken)
    {
        var wh = await _context.Warehouses.Include(w => w.Zones).FirstOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (wh == null) return Result<WarehouseZoneDto>.Failure("WarehouseNotFound", "Warehouse not found.");

        var zone = wh.AddZone(request.Dto.NameAr, request.Dto.NameEn, request.Dto.Code);
        _context.Warehouses.Update(wh);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("WarehouseZone added to {WarehouseId}: {ZoneId}", wh.Id, zone.Id);
        return Result<WarehouseZoneDto>.Success(_mapper.Map<WarehouseZoneDto>(zone));
    }
}

public class RemoveWarehouseZoneCommandHandler : IRequestHandler<RemoveWarehouseZoneCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveWarehouseZoneCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveWarehouseZoneCommand request, CancellationToken cancellationToken)
    {
        var wh = await _context.Warehouses.Include(w => w.Zones).FirstOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (wh == null) return Result.Failure("WarehouseNotFound", "Warehouse not found.");
        wh.RemoveZone(request.ZoneId);
        _context.Warehouses.Update(wh);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeactivateWarehouseCommandHandler : IRequestHandler<DeactivateWarehouseCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeactivateWarehouseCommandHandler> _logger;

    public DeactivateWarehouseCommandHandler(IApplicationDbContext context, ILogger<DeactivateWarehouseCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(DeactivateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var wh = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (wh == null) return Result.Failure("WarehouseNotFound", "Warehouse not found.");
        wh.Deactivate();
        _context.Warehouses.Update(wh);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Warehouse deactivated: {Id}", wh.Id);
        return Result.Success();
    }
}

public class ActivateWarehouseCommandHandler : IRequestHandler<ActivateWarehouseCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateWarehouseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var wh = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (wh == null) return Result.Failure("WarehouseNotFound", "Warehouse not found.");
        wh.Activate();
        _context.Warehouses.Update(wh);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
