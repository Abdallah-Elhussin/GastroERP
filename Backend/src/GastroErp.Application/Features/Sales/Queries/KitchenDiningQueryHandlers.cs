using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Queries;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.Queries;

public class GetKitchenStationsQueryHandler : IRequestHandler<GetKitchenStationsQuery, Result<IReadOnlyList<KitchenStationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetKitchenStationsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<KitchenStationDto>>> Handle(GetKitchenStationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.KitchenStations.AsNoTracking().Where(s => s.TenantId == request.TenantId);
        if (request.BranchId.HasValue) query = query.Where(s => s.BranchId == request.BranchId);
        var stations = await query.OrderBy(s => s.SortOrder).ToListAsync(cancellationToken);
        return Result<IReadOnlyList<KitchenStationDto>>.Success(_mapper.Map<IReadOnlyList<KitchenStationDto>>(stations));
    }
}

public class GetKitchenTicketsQueryHandler : IRequestHandler<GetKitchenTicketsQuery, PagedResult<KitchenTicketDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetKitchenTicketsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<KitchenTicketDto>> Handle(GetKitchenTicketsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.KitchenTickets.AsNoTracking().Where(t => t.TenantId == request.TenantId);
        if (filter.BranchId.HasValue) query = query.Where(t => t.BranchId == filter.BranchId);
        if (filter.StationId.HasValue) query = query.Where(t => t.KitchenStationId == filter.StationId);
        if (filter.Status.HasValue) query = query.Where(t => t.Status == filter.Status);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);

        var tickets = await query.Include(t => t.Items)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<KitchenTicketDto>.Success(_mapper.Map<IReadOnlyList<KitchenTicketDto>>(tickets), page, pageSize, totalCount);
    }
}

public class GetKitchenTicketByIdQueryHandler : IRequestHandler<GetKitchenTicketByIdQuery, Result<KitchenTicketDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetKitchenTicketByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<KitchenTicketDto>> Handle(GetKitchenTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _context.KitchenTickets.AsNoTracking()
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        return ticket is null
            ? Result<KitchenTicketDto>.Failure("NotFound", "Kitchen ticket not found.")
            : Result<KitchenTicketDto>.Success(_mapper.Map<KitchenTicketDto>(ticket));
    }
}

public class GetActiveKitchenTicketsByStationQueryHandler : IRequestHandler<GetActiveKitchenTicketsByStationQuery, Result<IReadOnlyList<KitchenTicketDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetActiveKitchenTicketsByStationQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<KitchenTicketDto>>> Handle(GetActiveKitchenTicketsByStationQuery request, CancellationToken cancellationToken)
    {
        var active = new[] { KitchenTicketStatus.Pending, KitchenTicketStatus.InProgress, KitchenTicketStatus.Ready };
        var tickets = await _context.KitchenTickets.AsNoTracking()
            .Include(t => t.Items)
            .Where(t => t.KitchenStationId == request.StationId && active.Contains(t.Status))
            .OrderBy(t => t.Priority).ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<KitchenTicketDto>>.Success(_mapper.Map<IReadOnlyList<KitchenTicketDto>>(tickets));
    }
}

public class GetFloorPlansQueryHandler : IRequestHandler<GetFloorPlansQuery, Result<IReadOnlyList<FloorPlanDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFloorPlansQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<FloorPlanDto>>> Handle(GetFloorPlansQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FloorPlans.AsNoTracking()
            .Include(f => f.DiningAreas)
            .Where(f => f.TenantId == request.TenantId);
        if (request.BranchId.HasValue) query = query.Where(f => f.BranchId == request.BranchId);
        var plans = await query.OrderBy(f => f.SortOrder).ToListAsync(cancellationToken);
        return Result<IReadOnlyList<FloorPlanDto>>.Success(_mapper.Map<IReadOnlyList<FloorPlanDto>>(plans));
    }
}

public class GetFloorPlanByIdQueryHandler : IRequestHandler<GetFloorPlanByIdQuery, Result<FloorPlanDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFloorPlanByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<FloorPlanDetailDto>> Handle(GetFloorPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await _context.FloorPlans.AsNoTracking()
            .Include(f => f.DiningAreas).ThenInclude(a => a.Tables)
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
        return plan is null
            ? Result<FloorPlanDetailDto>.Failure("NotFound", "Floor plan not found.")
            : Result<FloorPlanDetailDto>.Success(_mapper.Map<FloorPlanDetailDto>(plan));
    }
}

public class GetTablesByFloorPlanQueryHandler : IRequestHandler<GetTablesByFloorPlanQuery, Result<IReadOnlyList<RestaurantTableDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTablesByFloorPlanQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<RestaurantTableDto>>> Handle(GetTablesByFloorPlanQuery request, CancellationToken cancellationToken)
    {
        var plan = await _context.FloorPlans.AsNoTracking()
            .Include(f => f.DiningAreas).ThenInclude(a => a.Tables)
            .FirstOrDefaultAsync(f => f.Id == request.FloorPlanId, cancellationToken);
        if (plan is null) return Result<IReadOnlyList<RestaurantTableDto>>.Failure("NotFound", "Floor plan not found.");

        var tables = plan.DiningAreas.SelectMany(a => a.Tables).ToList();
        return Result<IReadOnlyList<RestaurantTableDto>>.Success(_mapper.Map<IReadOnlyList<RestaurantTableDto>>(tables));
    }
}

public class GetTableByIdQueryHandler : IRequestHandler<GetTableByIdQuery, Result<RestaurantTableDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTableByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<RestaurantTableDto>> Handle(GetTableByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await _context.FloorPlans.AsNoTracking()
            .Include(f => f.DiningAreas).ThenInclude(a => a.Tables)
            .FirstOrDefaultAsync(f => f.DiningAreas.Any(a => a.Tables.Any(t => t.Id == request.Id)), cancellationToken);
        if (plan is null) return Result<RestaurantTableDto>.Failure("NotFound", "Table not found.");

        var table = plan.DiningAreas.SelectMany(a => a.Tables).First(t => t.Id == request.Id);
        return Result<RestaurantTableDto>.Success(_mapper.Map<RestaurantTableDto>(table));
    }
}

public class GetTableReservationsQueryHandler : IRequestHandler<GetTableReservationsQuery, PagedResult<TableReservationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTableReservationsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<TableReservationDto>> Handle(GetTableReservationsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.TableReservations.AsNoTracking().Where(r => r.TenantId == request.TenantId);
        if (filter.BranchId.HasValue) query = query.Where(r => r.BranchId == filter.BranchId);
        if (filter.Status.HasValue) query = query.Where(r => r.Status == filter.Status);
        if (filter.FromDate.HasValue) query = query.Where(r => r.ReservationDate >= filter.FromDate);
        if (filter.ToDate.HasValue) query = query.Where(r => r.ReservationDate <= filter.ToDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);

        var reservations = await query.OrderBy(r => r.ReservationDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<TableReservationDto>.Success(_mapper.Map<IReadOnlyList<TableReservationDto>>(reservations), page, pageSize, totalCount);
    }
}

public class GetTableReservationByIdQueryHandler : IRequestHandler<GetTableReservationByIdQuery, Result<TableReservationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTableReservationByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<TableReservationDto>> Handle(GetTableReservationByIdQuery request, CancellationToken cancellationToken)
    {
        var reservation = await _context.TableReservations.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        return reservation is null
            ? Result<TableReservationDto>.Failure("NotFound", "Reservation not found.")
            : Result<TableReservationDto>.Success(_mapper.Map<TableReservationDto>(reservation));
    }
}
