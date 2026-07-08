using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Delivery.Services;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Services;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.Commands;

public class CreateKitchenStationCommandHandler : IRequestHandler<CreateKitchenStationCommand, Result<KitchenStationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateKitchenStationCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<KitchenStationDto>> Handle(CreateKitchenStationCommand request, CancellationToken cancellationToken)
    {
        var station = KitchenStation.Create(
            request.TenantId, request.Dto.BranchId, request.Dto.NameAr, request.Dto.StationType,
            request.Dto.NameEn, request.Dto.DeviceId, request.Dto.CategoryId, request.Dto.SortOrder);

        _context.KitchenStations.Add(station);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<KitchenStationDto>.Success(_mapper.Map<KitchenStationDto>(station));
    }
}

public class UpdateKitchenStationCommandHandler : IRequestHandler<UpdateKitchenStationCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateKitchenStationCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateKitchenStationCommand request, CancellationToken cancellationToken)
    {
        var station = await _context.KitchenStations.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (station is null) return Result.Failure("NotFound", "Kitchen station not found.");

        station.Update(request.Dto.NameAr, request.Dto.NameEn, request.Dto.StationType,
            request.Dto.DeviceId, request.Dto.CategoryId, request.Dto.SortOrder);
        _context.KitchenStations.Update(station);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class StartKitchenTicketCommandHandler : IRequestHandler<StartKitchenTicketCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public StartKitchenTicketCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(StartKitchenTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.KitchenTickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);
        if (ticket is null) return Result.Failure("NotFound", "Kitchen ticket not found.");
        ticket.Start();
        _context.KitchenTickets.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class MarkKitchenTicketReadyCommandHandler : IRequestHandler<MarkKitchenTicketReadyCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeliveryKitchenIntegrationService _deliveryKitchen;

    public MarkKitchenTicketReadyCommandHandler(
        IApplicationDbContext context, IDeliveryKitchenIntegrationService deliveryKitchen)
        => (_context, _deliveryKitchen) = (context, deliveryKitchen);

    public async Task<Result> Handle(MarkKitchenTicketReadyCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.KitchenTickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);
        if (ticket is null) return Result.Failure("NotFound", "Kitchen ticket not found.");
        ticket.MarkReady();
        _context.KitchenTickets.Update(ticket);

        await _deliveryKitchen.CheckAndMarkReadyForPickupAsync(ticket.SalesOrderId, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CompleteKitchenTicketCommandHandler : IRequestHandler<CompleteKitchenTicketCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CompleteKitchenTicketCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(CompleteKitchenTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.KitchenTickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);
        if (ticket is null) return Result.Failure("NotFound", "Kitchen ticket not found.");
        ticket.Complete();
        _context.KitchenTickets.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class MarkKitchenItemReadyCommandHandler : IRequestHandler<MarkKitchenItemReadyCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public MarkKitchenItemReadyCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(MarkKitchenItemReadyCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.KitchenTickets
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Items.Any(i => i.Id == request.ItemId), cancellationToken);
        if (ticket is null) return Result.Failure("NotFound", "Kitchen ticket item not found.");

        ticket.MarkItemReady(request.ItemId);
        _context.KitchenTickets.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CreateFloorPlanCommandHandler : IRequestHandler<CreateFloorPlanCommand, Result<FloorPlanDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateFloorPlanCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<FloorPlanDto>> Handle(CreateFloorPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = FloorPlan.Create(request.TenantId, request.Dto.BranchId, request.Dto.NameAr, request.Dto.NameEn, request.Dto.SortOrder);
        _context.FloorPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<FloorPlanDto>.Success(_mapper.Map<FloorPlanDto>(plan));
    }
}

public class AddDiningAreaCommandHandler : IRequestHandler<AddDiningAreaCommand, Result<DiningAreaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AddDiningAreaCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<DiningAreaDto>> Handle(AddDiningAreaCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.FloorPlans
            .Include(f => f.DiningAreas).ThenInclude(a => a.Tables)
            .FirstOrDefaultAsync(f => f.Id == request.FloorPlanId, cancellationToken);
        if (plan is null) return Result<DiningAreaDto>.Failure("NotFound", "Floor plan not found.");

        var area = plan.AddArea(request.Dto.NameAr, request.Dto.NameEn, request.Dto.Capacity, request.Dto.SortOrder);
        _context.FloorPlans.Update(plan);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<DiningAreaDto>.Success(_mapper.Map<DiningAreaDto>(area));
    }
}

public class AddRestaurantTableCommandHandler : IRequestHandler<AddRestaurantTableCommand, Result<RestaurantTableDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AddRestaurantTableCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<RestaurantTableDto>> Handle(AddRestaurantTableCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.FloorPlans
            .Include(f => f.DiningAreas).ThenInclude(a => a.Tables)
            .FirstOrDefaultAsync(f => f.DiningAreas.Any(a => a.Id == request.DiningAreaId), cancellationToken);
        if (plan is null) return Result<RestaurantTableDto>.Failure("NotFound", "Dining area not found.");

        var area = plan.DiningAreas.First(a => a.Id == request.DiningAreaId);
        var table = area.AddTable(request.Dto.TableNumber, request.Dto.Capacity, request.Dto.Shape,
            request.Dto.NameAr, request.Dto.NameEn, request.Dto.PositionX, request.Dto.PositionY);

        _context.FloorPlans.Update(plan);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<RestaurantTableDto>.Success(_mapper.Map<RestaurantTableDto>(table));
    }
}

public class OccupyTableCommandHandler : IRequestHandler<OccupyTableCommand, Result>
{
    private readonly ITableService _tableService;
    private readonly IApplicationDbContext _context;

    public OccupyTableCommandHandler(ITableService tableService, IApplicationDbContext context)
        => (_tableService, _context) = (tableService, context);

    public async Task<Result> Handle(OccupyTableCommand request, CancellationToken cancellationToken)
    {
        var table = await _tableService.GetTableByIdAsync(request.TableId, cancellationToken);
        if (table is null) return Result.Failure("NotFound", "Table not found.");

        await _tableService.OccupyTableAsync(request.TableId, request.OrderId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ReleaseTableCommandHandler : IRequestHandler<ReleaseTableCommand, Result>
{
    private readonly ITableService _tableService;
    private readonly IApplicationDbContext _context;

    public ReleaseTableCommandHandler(ITableService tableService, IApplicationDbContext context)
        => (_tableService, _context) = (tableService, context);

    public async Task<Result> Handle(ReleaseTableCommand request, CancellationToken cancellationToken)
    {
        await _tableService.ReleaseTableForOrderAsync(request.TableId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class UpdateTableStatusCommandHandler : IRequestHandler<UpdateTableStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateTableStatusCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateTableStatusCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.FloorPlans
            .Include(f => f.DiningAreas).ThenInclude(a => a.Tables)
            .FirstOrDefaultAsync(f => f.DiningAreas.Any(a => a.Tables.Any(t => t.Id == request.TableId)), cancellationToken);
        if (plan is null) return Result.Failure("NotFound", "Table not found.");

        var table = plan.DiningAreas.SelectMany(a => a.Tables).First(t => t.Id == request.TableId);
        table.SetStatus(request.Dto.Status);
        _context.FloorPlans.Update(plan);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CreateTableReservationCommandHandler : IRequestHandler<CreateTableReservationCommand, Result<TableReservationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITableService _tableService;

    public CreateTableReservationCommandHandler(IApplicationDbContext context, IMapper mapper, ITableService tableService)
        => (_context, _mapper, _tableService) = (context, mapper, tableService);

    public async Task<Result<TableReservationDto>> Handle(CreateTableReservationCommand request, CancellationToken cancellationToken)
    {
        if (request.Dto.TableId.HasValue)
        {
            var table = await _tableService.GetTableByIdAsync(request.Dto.TableId.Value, cancellationToken);
            if (table is null) return Result<TableReservationDto>.Failure("NotFound", "Table not found.");
            if (table.Status != TableStatus.Available)
                return Result<TableReservationDto>.Failure("TableNotAvailable", "Table is not available.");
        }

        var reservation = TableReservation.Create(
            request.TenantId, request.Dto.BranchId, request.Dto.CustomerName, request.Dto.CustomerPhone,
            request.Dto.GuestCount, request.Dto.ReservationDate, request.Dto.DurationMinutes,
            request.Dto.TableId, request.Dto.Notes);

        _context.TableReservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<TableReservationDto>.Success(_mapper.Map<TableReservationDto>(reservation));
    }
}

public class ConfirmTableReservationCommandHandler : IRequestHandler<ConfirmTableReservationCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ConfirmTableReservationCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ConfirmTableReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _context.TableReservations.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (reservation is null) return Result.Failure("NotFound", "Reservation not found.");

        if (reservation.TableId.HasValue)
        {
            var plan = await _context.FloorPlans
                .Include(f => f.DiningAreas).ThenInclude(a => a.Tables)
                .FirstOrDefaultAsync(f => f.DiningAreas.Any(a => a.Tables.Any(t => t.Id == reservation.TableId)), cancellationToken);
            if (plan is not null)
            {
                var table = plan.DiningAreas.SelectMany(a => a.Tables).First(t => t.Id == reservation.TableId);
                table.Reserve();
                _context.FloorPlans.Update(plan);
            }
        }

        reservation.Confirm();
        _context.TableReservations.Update(reservation);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CancelTableReservationCommandHandler : IRequestHandler<CancelTableReservationCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CancelTableReservationCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(CancelTableReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _context.TableReservations.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (reservation is null) return Result.Failure("NotFound", "Reservation not found.");

        reservation.Cancel(request.Dto.Reason);
        _context.TableReservations.Update(reservation);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SeatTableReservationCommandHandler : IRequestHandler<SeatTableReservationCommand, Result<OrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly ITableService _tableService;

    public SeatTableReservationCommandHandler(
        IApplicationDbContext context, IMapper mapper,
        IOrderNumberGenerator orderNumberGenerator, ITableService tableService)
        => (_context, _mapper, _orderNumberGenerator, _tableService) = (context, mapper, orderNumberGenerator, tableService);

    public async Task<Result<OrderDto>> Handle(SeatTableReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _context.TableReservations.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (reservation is null) return Result<OrderDto>.Failure("NotFound", "Reservation not found.");

        var branch = await _context.Branches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == reservation.BranchId, cancellationToken);
        if (branch is null) return Result<OrderDto>.Failure("NotFound", "Branch not found.");

        var orderNumber = await _orderNumberGenerator.GenerateAsync(branch.Id, branch.Code, cancellationToken);
        var order = SalesOrder.Create(
            request.TenantId, branch.CompanyId, branch.Id, request.Dto.DeviceId, request.CashierId,
            OrderType.DineIn, SalesChannel.DineIn, orderNumber,
            tableId: reservation.TableId, guestCount: request.Dto.GuestCount ?? reservation.GuestCount,
            waiterId: request.Dto.WaiterId);

        reservation.Seat(order.Id);
        _context.SalesOrders.Add(order);
        _context.TableReservations.Update(reservation);

        if (reservation.TableId.HasValue)
            await _tableService.OccupyTableAsync(reservation.TableId.Value, order.Id, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }
}
