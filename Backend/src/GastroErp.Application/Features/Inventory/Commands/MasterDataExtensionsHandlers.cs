using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Domain.Entities.Inventory.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

public sealed class AddWarehouseShelfCommandHandler
    : IRequestHandler<AddWarehouseShelfCommand, Result<WarehouseShelfDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AddWarehouseShelfCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<WarehouseShelfDto>> Handle(AddWarehouseShelfCommand request, CancellationToken cancellationToken)
    {
        var wh = await _context.Warehouses
            .Include(w => w.Zones).ThenInclude(z => z.Shelves)
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (wh is null) return Result<WarehouseShelfDto>.Failure("WarehouseNotFound", "Warehouse not found.");

        var zone = wh.Zones.FirstOrDefault(z => z.Id == request.ZoneId);
        if (zone is null) return Result<WarehouseShelfDto>.Failure("ZoneNotFound", "Warehouse zone not found.");

        var shelf = zone.AddShelf(request.Dto.NameAr, request.Dto.NameEn, request.Dto.Code);
        _context.Warehouses.Update(wh);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<WarehouseShelfDto>.Success(_mapper.Map<WarehouseShelfDto>(shelf));
    }
}

public sealed class RemoveWarehouseShelfCommandHandler : IRequestHandler<RemoveWarehouseShelfCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveWarehouseShelfCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveWarehouseShelfCommand request, CancellationToken cancellationToken)
    {
        var wh = await _context.Warehouses
            .Include(w => w.Zones).ThenInclude(z => z.Shelves)
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (wh is null) return Result.Failure("WarehouseNotFound", "Warehouse not found.");

        var zone = wh.Zones.FirstOrDefault(z => z.Id == request.ZoneId);
        if (zone is null) return Result.Failure("ZoneNotFound", "Warehouse zone not found.");

        zone.RemoveShelf(request.ShelfId);
        _context.Warehouses.Update(wh);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class AddWarehouseBinCommandHandler : IRequestHandler<AddWarehouseBinCommand, Result<WarehouseBinDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AddWarehouseBinCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<WarehouseBinDto>> Handle(AddWarehouseBinCommand request, CancellationToken cancellationToken)
    {
        var wh = await _context.Warehouses
            .Include(w => w.Zones).ThenInclude(z => z.Shelves).ThenInclude(s => s.Bins)
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (wh is null) return Result<WarehouseBinDto>.Failure("WarehouseNotFound", "Warehouse not found.");

        var zone = wh.Zones.FirstOrDefault(z => z.Id == request.ZoneId);
        if (zone is null) return Result<WarehouseBinDto>.Failure("ZoneNotFound", "Warehouse zone not found.");

        var shelf = zone.Shelves.FirstOrDefault(s => s.Id == request.ShelfId);
        if (shelf is null) return Result<WarehouseBinDto>.Failure("ShelfNotFound", "Warehouse shelf not found.");

        var bin = shelf.AddBin(request.Dto.NameAr, request.Dto.NameEn, request.Dto.Code);
        _context.Warehouses.Update(wh);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<WarehouseBinDto>.Success(_mapper.Map<WarehouseBinDto>(bin));
    }
}

public sealed class RemoveWarehouseBinCommandHandler : IRequestHandler<RemoveWarehouseBinCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveWarehouseBinCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveWarehouseBinCommand request, CancellationToken cancellationToken)
    {
        var wh = await _context.Warehouses
            .Include(w => w.Zones).ThenInclude(z => z.Shelves).ThenInclude(s => s.Bins)
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (wh is null) return Result.Failure("WarehouseNotFound", "Warehouse not found.");

        var zone = wh.Zones.FirstOrDefault(z => z.Id == request.ZoneId);
        if (zone is null) return Result.Failure("ZoneNotFound", "Warehouse zone not found.");

        var shelf = zone.Shelves.FirstOrDefault(s => s.Id == request.ShelfId);
        if (shelf is null) return Result.Failure("ShelfNotFound", "Warehouse shelf not found.");

        shelf.RemoveBin(request.BinId);
        _context.Warehouses.Update(wh);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── Brands ───────────────────────────────────────────────────────────────────

public sealed class CreateInventoryBrandCommandHandler
    : IRequestHandler<CreateInventoryBrandCommand, Result<InventoryBrandDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateInventoryBrandCommandHandler> _logger;

    public CreateInventoryBrandCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateInventoryBrandCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<InventoryBrandDto>> Handle(CreateInventoryBrandCommand request, CancellationToken cancellationToken)
    {
        var entity = new InventoryBrand(request.Dto.TenantId, request.Dto.NameAr, request.Dto.NameEn, request.Dto.Code);
        _context.InventoryBrands.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("InventoryBrand created: {Id}", entity.Id);
        return Result<InventoryBrandDto>.Success(_mapper.Map<InventoryBrandDto>(entity));
    }
}

public sealed class UpdateInventoryBrandCommandHandler : IRequestHandler<UpdateInventoryBrandCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateInventoryBrandCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateInventoryBrandCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryBrands.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("BrandNotFound", "Brand not found.");
        entity.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.Code);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class ActivateInventoryBrandCommandHandler : IRequestHandler<ActivateInventoryBrandCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public ActivateInventoryBrandCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateInventoryBrandCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryBrands.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("BrandNotFound", "Brand not found.");
        entity.Activate();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivateInventoryBrandCommandHandler : IRequestHandler<DeactivateInventoryBrandCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeactivateInventoryBrandCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateInventoryBrandCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryBrands.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("BrandNotFound", "Brand not found.");
        entity.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── Manufacturers ────────────────────────────────────────────────────────────

public sealed class CreateInventoryManufacturerCommandHandler
    : IRequestHandler<CreateInventoryManufacturerCommand, Result<InventoryManufacturerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateInventoryManufacturerCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<InventoryManufacturerDto>> Handle(
        CreateInventoryManufacturerCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new InventoryManufacturer(
            request.Dto.TenantId,
            request.Dto.NameAr,
            request.Dto.NameEn,
            request.Dto.Code,
            request.Dto.Country);
        _context.InventoryManufacturers.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<InventoryManufacturerDto>.Success(_mapper.Map<InventoryManufacturerDto>(entity));
    }
}

public sealed class UpdateInventoryManufacturerCommandHandler
    : IRequestHandler<UpdateInventoryManufacturerCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateInventoryManufacturerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateInventoryManufacturerCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryManufacturers.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("ManufacturerNotFound", "Manufacturer not found.");
        entity.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.Code, request.Dto.Country);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class ActivateInventoryManufacturerCommandHandler
    : IRequestHandler<ActivateInventoryManufacturerCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public ActivateInventoryManufacturerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateInventoryManufacturerCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryManufacturers.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("ManufacturerNotFound", "Manufacturer not found.");
        entity.Activate();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivateInventoryManufacturerCommandHandler
    : IRequestHandler<DeactivateInventoryManufacturerCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeactivateInventoryManufacturerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateInventoryManufacturerCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryManufacturers.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("ManufacturerNotFound", "Manufacturer not found.");
        entity.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── Attributes ───────────────────────────────────────────────────────────────

public sealed class CreateInventoryAttributeCommandHandler
    : IRequestHandler<CreateInventoryAttributeCommand, Result<InventoryAttributeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateInventoryAttributeCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<InventoryAttributeDto>> Handle(
        CreateInventoryAttributeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new InventoryAttribute(
            request.Dto.TenantId,
            request.Dto.NameAr,
            request.Dto.DataType,
            request.Dto.NameEn,
            request.Dto.Code);
        _context.InventoryAttributes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<InventoryAttributeDto>.Success(_mapper.Map<InventoryAttributeDto>(entity));
    }
}

public sealed class UpdateInventoryAttributeCommandHandler
    : IRequestHandler<UpdateInventoryAttributeCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateInventoryAttributeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateInventoryAttributeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryAttributes.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("AttributeNotFound", "Attribute not found.");
        entity.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.DataType, request.Dto.Code);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class ActivateInventoryAttributeCommandHandler
    : IRequestHandler<ActivateInventoryAttributeCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public ActivateInventoryAttributeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateInventoryAttributeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryAttributes.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("AttributeNotFound", "Attribute not found.");
        entity.Activate();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivateInventoryAttributeCommandHandler
    : IRequestHandler<DeactivateInventoryAttributeCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeactivateInventoryAttributeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateInventoryAttributeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryAttributes.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("AttributeNotFound", "Attribute not found.");
        entity.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class AddInventoryAttributeValueCommandHandler
    : IRequestHandler<AddInventoryAttributeValueCommand, Result<InventoryAttributeValueDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AddInventoryAttributeValueCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<InventoryAttributeValueDto>> Handle(
        AddInventoryAttributeValueCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryAttributes
            .Include(a => a.Values)
            .FirstOrDefaultAsync(x => x.Id == request.AttributeId, cancellationToken);
        if (entity is null) return Result<InventoryAttributeValueDto>.Failure("AttributeNotFound", "Attribute not found.");

        var value = entity.AddValue(request.Dto.ValueAr, request.Dto.ValueEn, request.Dto.SortOrder);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<InventoryAttributeValueDto>.Success(_mapper.Map<InventoryAttributeValueDto>(value));
    }
}

public sealed class RemoveInventoryAttributeValueCommandHandler
    : IRequestHandler<RemoveInventoryAttributeValueCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public RemoveInventoryAttributeValueCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveInventoryAttributeValueCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryAttributes
            .Include(a => a.Values)
            .FirstOrDefaultAsync(x => x.Id == request.AttributeId, cancellationToken);
        if (entity is null) return Result.Failure("AttributeNotFound", "Attribute not found.");
        entity.RemoveValue(request.ValueId);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── Price Lists ──────────────────────────────────────────────────────────────

public sealed class CreateInventoryPriceListCommandHandler
    : IRequestHandler<CreateInventoryPriceListCommand, Result<InventoryPriceListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateInventoryPriceListCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<InventoryPriceListDto>> Handle(
        CreateInventoryPriceListCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new InventoryPriceList(
            request.Dto.TenantId,
            request.Dto.NameAr,
            request.Dto.NameEn,
            request.Dto.Code,
            request.Dto.Currency,
            request.Dto.ValidFrom,
            request.Dto.ValidTo);
        _context.InventoryPriceLists.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<InventoryPriceListDto>.Success(_mapper.Map<InventoryPriceListDto>(entity));
    }
}

public sealed class UpdateInventoryPriceListCommandHandler
    : IRequestHandler<UpdateInventoryPriceListCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateInventoryPriceListCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateInventoryPriceListCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryPriceLists.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("PriceListNotFound", "Price list not found.");
        entity.UpdateInfo(
            request.Dto.NameAr,
            request.Dto.NameEn,
            request.Dto.Code,
            request.Dto.Currency,
            request.Dto.ValidFrom,
            request.Dto.ValidTo);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class ActivateInventoryPriceListCommandHandler
    : IRequestHandler<ActivateInventoryPriceListCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public ActivateInventoryPriceListCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateInventoryPriceListCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryPriceLists.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("PriceListNotFound", "Price list not found.");
        entity.Activate();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivateInventoryPriceListCommandHandler
    : IRequestHandler<DeactivateInventoryPriceListCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeactivateInventoryPriceListCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateInventoryPriceListCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryPriceLists.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null) return Result.Failure("PriceListNotFound", "Price list not found.");
        entity.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class UpsertInventoryPriceListLineCommandHandler
    : IRequestHandler<UpsertInventoryPriceListLineCommand, Result<InventoryPriceListLineDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpsertInventoryPriceListLineCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<InventoryPriceListLineDto>> Handle(
        UpsertInventoryPriceListLineCommand request,
        CancellationToken cancellationToken)
    {
        var itemExists = await _context.InventoryItems.AnyAsync(
            i => i.Id == request.Dto.InventoryItemId,
            cancellationToken);
        if (!itemExists)
            return Result<InventoryPriceListLineDto>.Failure("ItemNotFound", "Inventory item not found.");

        var entity = await _context.InventoryPriceLists
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.PriceListId, cancellationToken);
        if (entity is null)
            return Result<InventoryPriceListLineDto>.Failure("PriceListNotFound", "Price list not found.");

        var line = entity.UpsertLine(request.Dto.InventoryItemId, request.Dto.UnitPrice, request.Dto.UnitId);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<InventoryPriceListLineDto>(line);
        var item = await _context.InventoryItems.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == line.InventoryItemId, cancellationToken);
        return Result<InventoryPriceListLineDto>.Success(
            dto with { InventoryItemNameAr = item?.NameAr });
    }
}

public sealed class RemoveInventoryPriceListLineCommandHandler
    : IRequestHandler<RemoveInventoryPriceListLineCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public RemoveInventoryPriceListLineCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveInventoryPriceListLineCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryPriceLists
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.PriceListId, cancellationToken);
        if (entity is null) return Result.Failure("PriceListNotFound", "Price list not found.");
        entity.RemoveLine(request.LineId);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── Queries (Phase J) ────────────────────────────────────────────────────────

public sealed class GetInventoryBrandsQueryHandler
    : IRequestHandler<GetInventoryBrandsQuery, Result<List<InventoryBrandDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryBrandsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<InventoryBrandDto>>> Handle(
        GetInventoryBrandsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.InventoryBrands.AsNoTracking().Where(x => x.TenantId == request.TenantId);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        var items = await query.OrderBy(x => x.NameAr).ToListAsync(cancellationToken);
        return Result<List<InventoryBrandDto>>.Success(_mapper.Map<List<InventoryBrandDto>>(items));
    }
}

public sealed class GetInventoryManufacturersQueryHandler
    : IRequestHandler<GetInventoryManufacturersQuery, Result<List<InventoryManufacturerDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryManufacturersQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<InventoryManufacturerDto>>> Handle(
        GetInventoryManufacturersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.InventoryManufacturers.AsNoTracking().Where(x => x.TenantId == request.TenantId);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        var items = await query.OrderBy(x => x.NameAr).ToListAsync(cancellationToken);
        return Result<List<InventoryManufacturerDto>>.Success(_mapper.Map<List<InventoryManufacturerDto>>(items));
    }
}

public sealed class GetInventoryAttributesQueryHandler
    : IRequestHandler<GetInventoryAttributesQuery, Result<List<InventoryAttributeDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryAttributesQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<InventoryAttributeDto>>> Handle(
        GetInventoryAttributesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.InventoryAttributes.AsNoTracking()
            .Include(a => a.Values)
            .Where(x => x.TenantId == request.TenantId);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        var items = await query.OrderBy(x => x.NameAr).ToListAsync(cancellationToken);
        return Result<List<InventoryAttributeDto>>.Success(_mapper.Map<List<InventoryAttributeDto>>(items));
    }
}

public sealed class GetInventoryAttributeByIdQueryHandler
    : IRequestHandler<GetInventoryAttributeByIdQuery, Result<InventoryAttributeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryAttributeByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<InventoryAttributeDto>> Handle(
        GetInventoryAttributeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryAttributes.AsNoTracking()
            .Include(a => a.Values)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return entity is null
            ? Result<InventoryAttributeDto>.Failure("AttributeNotFound", "Attribute not found.")
            : Result<InventoryAttributeDto>.Success(_mapper.Map<InventoryAttributeDto>(entity));
    }
}

public sealed class GetInventoryPriceListsQueryHandler
    : IRequestHandler<GetInventoryPriceListsQuery, Result<List<InventoryPriceListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryPriceListsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<InventoryPriceListDto>>> Handle(
        GetInventoryPriceListsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.InventoryPriceLists.AsNoTracking()
            .Include(p => p.Lines)
            .Where(x => x.TenantId == request.TenantId);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        var items = await query.OrderBy(x => x.NameAr).ToListAsync(cancellationToken);
        return Result<List<InventoryPriceListDto>>.Success(_mapper.Map<List<InventoryPriceListDto>>(items));
    }
}

public sealed class GetInventoryPriceListByIdQueryHandler
    : IRequestHandler<GetInventoryPriceListByIdQuery, Result<InventoryPriceListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryPriceListByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<InventoryPriceListDto>> Handle(
        GetInventoryPriceListByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.InventoryPriceLists.AsNoTracking()
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null)
            return Result<InventoryPriceListDto>.Failure("PriceListNotFound", "Price list not found.");

        var dto = _mapper.Map<InventoryPriceListDto>(entity);
        var itemIds = entity.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var names = await _context.InventoryItems.AsNoTracking()
            .Where(i => itemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.NameAr })
            .ToDictionaryAsync(x => x.Id, x => x.NameAr, cancellationToken);

        var lines = dto.Lines.Select(l =>
            l with { InventoryItemNameAr = names.GetValueOrDefault(l.InventoryItemId) }).ToList();

        return Result<InventoryPriceListDto>.Success(dto with { Lines = lines });
    }
}
