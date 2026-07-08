using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Delivery.DTOs;
using GastroErp.Application.Features.Delivery.Queries;
using GastroErp.Application.Features.Delivery.Services;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Delivery.Queries;

public class GetDeliveryZonesQueryHandler : IRequestHandler<GetDeliveryZonesQuery, Result<IReadOnlyList<DeliveryZoneDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDeliveryZonesQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<DeliveryZoneDto>>> Handle(GetDeliveryZonesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DeliveryZones.AsNoTracking().Where(z => z.TenantId == request.TenantId);
        if (request.BranchId.HasValue) query = query.Where(z => z.BranchId == request.BranchId);
        var zones = await query.OrderBy(z => z.NameAr).ToListAsync(cancellationToken);
        return Result<IReadOnlyList<DeliveryZoneDto>>.Success(_mapper.Map<IReadOnlyList<DeliveryZoneDto>>(zones));
    }
}

public class GetDeliveryZoneByIdQueryHandler : IRequestHandler<GetDeliveryZoneByIdQuery, Result<DeliveryZoneDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDeliveryZoneByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<DeliveryZoneDto>> Handle(GetDeliveryZoneByIdQuery request, CancellationToken cancellationToken)
    {
        var zone = await _context.DeliveryZones.AsNoTracking()
            .FirstOrDefaultAsync(z => z.Id == request.Id, cancellationToken);
        return zone is null
            ? Result<DeliveryZoneDto>.Failure("NotFound", "Delivery zone not found.")
            : Result<DeliveryZoneDto>.Success(_mapper.Map<DeliveryZoneDto>(zone));
    }
}

public class GetDeliveryZoneFeeQueryHandler : IRequestHandler<GetDeliveryZoneFeeQuery, Result<DeliveryFeeQuoteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeliveryFeeCalculationService _feeService;

    public GetDeliveryZoneFeeQueryHandler(IApplicationDbContext context, IDeliveryFeeCalculationService feeService)
        => (_context, _feeService) = (context, feeService);

    public async Task<Result<DeliveryFeeQuoteDto>> Handle(GetDeliveryZoneFeeQuery request, CancellationToken cancellationToken)
    {
        var zone = await _context.DeliveryZones.AsNoTracking()
            .FirstOrDefaultAsync(z => z.Id == request.ZoneId, cancellationToken);
        if (zone is null) return Result<DeliveryFeeQuoteDto>.Failure("NotFound", "Delivery zone not found.");

        return await _feeService.CalculateFeeAsync(
            zone.TenantId, zone.BranchId, zone.Id, request.Latitude, request.Longitude, cancellationToken);
    }
}

public class GetDeliveryDriversQueryHandler : IRequestHandler<GetDeliveryDriversQuery, Result<IReadOnlyList<DeliveryDriverDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDeliveryDriversQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<DeliveryDriverDto>>> Handle(GetDeliveryDriversQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DeliveryDrivers.AsNoTracking().Where(d => d.TenantId == request.TenantId);
        if (request.BranchId.HasValue) query = query.Where(d => d.BranchId == request.BranchId);
        var drivers = await query.OrderBy(d => d.NameAr).ToListAsync(cancellationToken);
        return Result<IReadOnlyList<DeliveryDriverDto>>.Success(_mapper.Map<IReadOnlyList<DeliveryDriverDto>>(drivers));
    }
}

public class GetAvailableDriversQueryHandler : IRequestHandler<GetAvailableDriversQuery, Result<IReadOnlyList<DeliveryDriverDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAvailableDriversQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<DeliveryDriverDto>>> Handle(GetAvailableDriversQuery request, CancellationToken cancellationToken)
    {
        var drivers = await _context.DeliveryDrivers.AsNoTracking()
            .Where(d => d.TenantId == request.TenantId && d.BranchId == request.BranchId
                && d.IsActive && d.Status == DriverStatus.Available)
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<DeliveryDriverDto>>.Success(_mapper.Map<IReadOnlyList<DeliveryDriverDto>>(drivers));
    }
}

public class GetDeliveryOrdersQueryHandler : IRequestHandler<GetDeliveryOrdersQuery, PagedResult<DeliveryOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDeliveryOrdersQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<DeliveryOrderDto>> Handle(GetDeliveryOrdersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.DeliveryOrders.AsNoTracking().Where(d => d.TenantId == request.TenantId);
        if (filter.BranchId.HasValue) query = query.Where(d => d.BranchId == filter.BranchId);
        if (filter.Status.HasValue) query = query.Where(d => d.Status == filter.Status);
        if (filter.DriverId.HasValue) query = query.Where(d => d.CurrentDriverId == filter.DriverId);
        if (filter.ReadyForPickup == true) query = query.Where(d => d.IsReadyForPickup && d.Status == DeliveryStatus.Pending);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);

        var orders = await query.OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<DeliveryOrderDto>.Success(
            _mapper.Map<IReadOnlyList<DeliveryOrderDto>>(orders), page, pageSize, totalCount);
    }
}

public class GetDeliveryOrderByIdQueryHandler : IRequestHandler<GetDeliveryOrderByIdQuery, Result<DeliveryOrderDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDeliveryOrderByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<DeliveryOrderDetailDto>> Handle(GetDeliveryOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var delivery = await _context.DeliveryOrders.AsNoTracking()
            .Include(d => d.TrackingEvents)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        return delivery is null
            ? Result<DeliveryOrderDetailDto>.Failure("NotFound", "Delivery order not found.")
            : Result<DeliveryOrderDetailDto>.Success(_mapper.Map<DeliveryOrderDetailDto>(delivery));
    }
}

public class GetDeliveryBySalesOrderQueryHandler : IRequestHandler<GetDeliveryBySalesOrderQuery, Result<DeliveryOrderDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDeliveryBySalesOrderQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<DeliveryOrderDetailDto>> Handle(GetDeliveryBySalesOrderQuery request, CancellationToken cancellationToken)
    {
        var delivery = await _context.DeliveryOrders.AsNoTracking()
            .Include(d => d.TrackingEvents)
            .FirstOrDefaultAsync(d => d.SalesOrderId == request.SalesOrderId, cancellationToken);
        return delivery is null
            ? Result<DeliveryOrderDetailDto>.Failure("NotFound", "Delivery order not found.")
            : Result<DeliveryOrderDetailDto>.Success(_mapper.Map<DeliveryOrderDetailDto>(delivery));
    }
}

public class GetActiveDeliveriesByDriverQueryHandler : IRequestHandler<GetActiveDeliveriesByDriverQuery, Result<IReadOnlyList<DeliveryOrderDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetActiveDeliveriesByDriverQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<DeliveryOrderDto>>> Handle(GetActiveDeliveriesByDriverQuery request, CancellationToken cancellationToken)
    {
        var active = new[] { DeliveryStatus.Assigned, DeliveryStatus.PickedUp, DeliveryStatus.InTransit };
        var orders = await _context.DeliveryOrders.AsNoTracking()
            .Where(d => d.CurrentDriverId == request.DriverId && active.Contains(d.Status))
            .OrderBy(d => d.Priority).ThenBy(d => d.AssignedAt)
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<DeliveryOrderDto>>.Success(_mapper.Map<IReadOnlyList<DeliveryOrderDto>>(orders));
    }
}

public class GetDeliveryTrackingQueryHandler : IRequestHandler<GetDeliveryTrackingQuery, Result<IReadOnlyList<DeliveryTrackingEventDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDeliveryTrackingQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<DeliveryTrackingEventDto>>> Handle(GetDeliveryTrackingQuery request, CancellationToken cancellationToken)
    {
        var delivery = await _context.DeliveryOrders.AsNoTracking()
            .Include(d => d.TrackingEvents)
            .FirstOrDefaultAsync(d => d.Id == request.DeliveryOrderId, cancellationToken);
        if (delivery is null) return Result<IReadOnlyList<DeliveryTrackingEventDto>>.Failure("NotFound", "Delivery order not found.");

        return Result<IReadOnlyList<DeliveryTrackingEventDto>>.Success(
            _mapper.Map<IReadOnlyList<DeliveryTrackingEventDto>>(delivery.TrackingEvents.OrderBy(t => t.OccurredAt)));
    }
}
