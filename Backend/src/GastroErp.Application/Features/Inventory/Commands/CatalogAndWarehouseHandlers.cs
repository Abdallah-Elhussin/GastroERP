using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
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
        var cat = new InventoryCategory(request.Dto.TenantId, request.Dto.NameAr, request.Dto.NameEn);
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
        cat.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, null, null);
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
        var unit = new InventoryUnit(request.Dto.TenantId, request.Dto.NameAr, request.Dto.Symbol ?? request.Dto.NameAr, request.Dto.NameEn);
        _context.InventoryUnits.Add(unit);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("InventoryUnit created: {Id}", unit.Id);
        return Result<InventoryUnitDto>.Success(_mapper.Map<InventoryUnitDto>(unit));
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

        var item = new InventoryItem(request.Dto.TenantId, request.Dto.CategoryId, request.Dto.NameAr, request.Dto.BaseUnitId, request.Dto.NameEn, request.Dto.Sku, request.Dto.Barcode);
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("InventoryItem created: {Id}", item.Id);
        return Result<InventoryItemDto>.Success(_mapper.Map<InventoryItemDto>(item));
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
        item.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.DescriptionAr, request.Dto.DescriptionEn, request.Dto.Sku, request.Dto.Barcode);
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

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, Result<WarehouseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateWarehouseCommandHandler> _logger;

    public CreateWarehouseCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateWarehouseCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<WarehouseDto>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var wh = new Warehouse(request.Dto.TenantId, request.Dto.NameAr, request.Dto.NameEn, request.Dto.Code, request.Dto.BranchId);
        _context.Warehouses.Add(wh);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Warehouse created: {Id}", wh.Id);
        return Result<WarehouseDto>.Success(_mapper.Map<WarehouseDto>(wh));
    }
}

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateWarehouseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var wh = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (wh == null) return Result.Failure("WarehouseNotFound", "Warehouse not found.");
        wh.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.Code, request.Dto.Address);
        _context.Warehouses.Update(wh);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

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
