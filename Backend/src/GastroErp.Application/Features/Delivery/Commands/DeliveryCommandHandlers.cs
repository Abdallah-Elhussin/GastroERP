using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Delivery.Commands;
using GastroErp.Application.Features.Delivery.DTOs;
using GastroErp.Application.Features.Delivery.Services;
using GastroErp.Domain.Entities.Delivery;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Delivery.Commands;

public class CreateDeliveryZoneCommandHandler : IRequestHandler<CreateDeliveryZoneCommand, Result<DeliveryZoneDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateDeliveryZoneCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<DeliveryZoneDto>> Handle(CreateDeliveryZoneCommand request, CancellationToken cancellationToken)
    {
        var zone = DeliveryZone.Create(
            request.TenantId, request.Dto.BranchId, request.Dto.NameAr,
            request.Dto.CenterLatitude, request.Dto.CenterLongitude, request.Dto.RadiusKm,
            request.Dto.FeeType, request.Dto.FixedFee, request.Dto.FeePerKm,
            request.Dto.EstimatedMinutes, request.Dto.NameEn);

        _context.DeliveryZones.Add(zone);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<DeliveryZoneDto>.Success(_mapper.Map<DeliveryZoneDto>(zone));
    }
}

public class UpdateDeliveryZoneCommandHandler : IRequestHandler<UpdateDeliveryZoneCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateDeliveryZoneCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateDeliveryZoneCommand request, CancellationToken cancellationToken)
    {
        var zone = await _context.DeliveryZones.FirstOrDefaultAsync(z => z.Id == request.Id, cancellationToken);
        if (zone is null) return Result.Failure("NotFound", "Delivery zone not found.");

        zone.Update(request.Dto.NameAr, request.Dto.CenterLatitude, request.Dto.CenterLongitude,
            request.Dto.RadiusKm, request.Dto.FeeType, request.Dto.FixedFee, request.Dto.FeePerKm,
            request.Dto.EstimatedMinutes, request.Dto.NameEn);

        _context.DeliveryZones.Update(zone);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CreateDeliveryDriverCommandHandler : IRequestHandler<CreateDeliveryDriverCommand, Result<DeliveryDriverDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateDeliveryDriverCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<DeliveryDriverDto>> Handle(CreateDeliveryDriverCommand request, CancellationToken cancellationToken)
    {
        var driver = DeliveryDriver.Create(
            request.TenantId, request.Dto.BranchId, request.Dto.NameAr, request.Dto.Phone,
            request.Dto.UserId, request.Dto.NameEn, request.Dto.VehiclePlate);

        _context.DeliveryDrivers.Add(driver);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<DeliveryDriverDto>.Success(_mapper.Map<DeliveryDriverDto>(driver));
    }
}

public class UpdateDeliveryDriverCommandHandler : IRequestHandler<UpdateDeliveryDriverCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateDeliveryDriverCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateDeliveryDriverCommand request, CancellationToken cancellationToken)
    {
        var driver = await _context.DeliveryDrivers.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (driver is null) return Result.Failure("NotFound", "Driver not found.");

        driver.Update(request.Dto.NameAr, request.Dto.Phone, request.Dto.NameEn,
            request.Dto.VehiclePlate, request.Dto.UserId);

        _context.DeliveryDrivers.Update(driver);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class UpdateDriverStatusCommandHandler : IRequestHandler<UpdateDriverStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateDriverStatusCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateDriverStatusCommand request, CancellationToken cancellationToken)
    {
        var driver = await _context.DeliveryDrivers.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (driver is null) return Result.Failure("NotFound", "Driver not found.");

        driver.SetStatus(request.Dto.Status);
        _context.DeliveryDrivers.Update(driver);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CreateDeliveryOrderCommandHandler : IRequestHandler<CreateDeliveryOrderCommand, Result<DeliveryOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDeliveryOrderFactory _factory;

    public CreateDeliveryOrderCommandHandler(
        IApplicationDbContext context, IMapper mapper, IDeliveryOrderFactory factory)
        => (_context, _mapper, _factory) = (context, mapper, factory);

    public async Task<Result<DeliveryOrderDto>> Handle(CreateDeliveryOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.Dto.SalesOrderId && o.TenantId == request.TenantId, cancellationToken);
        if (order is null) return Result<DeliveryOrderDto>.Failure("NotFound", "Sales order not found.");
        if (order.OrderType != OrderType.Delivery)
            return Result<DeliveryOrderDto>.Failure("DeliveryOrderTypeRequired", "Sales order must be delivery type.");

        var deliveryDto = new CreateOrderDeliveryDto(
            request.Dto.Address, request.Dto.PaymentMode, request.Dto.Priority,
            request.Dto.DeliveryZoneId, request.Dto.ScheduledAt);

        var result = await _factory.CreateForSalesOrderAsync(
            request.TenantId, order.CompanyId, order.BranchId, order.Id, deliveryDto, cancellationToken);
        if (!result.IsSuccess) return Result<DeliveryOrderDto>.Failure(result.ErrorCode!, result.ErrorMessage!);

        var delivery = result.Data!;
        if (!string.IsNullOrEmpty(request.Dto.ExternalProviderCode))
            delivery.SetExternalReference(request.Dto.ExternalProviderCode!, request.Dto.ExternalOrderReference ?? "");

        var salesOrder = await _context.SalesOrders.FirstAsync(o => o.Id == order.Id, cancellationToken);
        salesOrder.ApplyDeliveryFee(delivery.DeliveryFee);
        salesOrder.LinkDeliveryOrder(delivery.Id);

        _context.DeliveryOrders.Add(delivery);
        _context.SalesOrders.Update(salesOrder);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<DeliveryOrderDto>.Success(_mapper.Map<DeliveryOrderDto>(delivery));
    }
}

public class AssignDeliveryCommandHandler : IRequestHandler<AssignDeliveryCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeliveryAssignmentService _assignmentService;
    private readonly IDeliveryOrderSyncService _syncService;

    public AssignDeliveryCommandHandler(
        IApplicationDbContext context, IDeliveryAssignmentService assignmentService,
        IDeliveryOrderSyncService syncService)
        => (_context, _assignmentService, _syncService) = (context, assignmentService, syncService);

    public async Task<Result> Handle(AssignDeliveryCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _context.DeliveryOrders.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (delivery is null) return Result.Failure("NotFound", "Delivery order not found.");

        var driver = await _context.DeliveryDrivers.FirstOrDefaultAsync(d => d.Id == request.Dto.DriverId, cancellationToken);
        if (driver is null) return Result.Failure("NotFound", "Driver not found.");

        var assignResult = await _assignmentService.AssignDriverAsync(delivery, driver, cancellationToken);
        if (!assignResult.IsSuccess) return assignResult;

        var salesOrder = await _context.SalesOrders.AsNoTracking()
            .FirstAsync(o => o.Id == delivery.SalesOrderId, cancellationToken);
        await _syncService.SyncOnAssignedAsync(delivery.SalesOrderId, request.UserId, salesOrder.DeviceId, cancellationToken);

        _context.DeliveryOrders.Update(delivery);
        _context.DeliveryDrivers.Update(driver);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class PickUpDeliveryCommandHandler : IRequestHandler<PickUpDeliveryCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeliveryOrderSyncService _syncService;

    public PickUpDeliveryCommandHandler(IApplicationDbContext context, IDeliveryOrderSyncService syncService)
        => (_context, _syncService) = (context, syncService);

    public async Task<Result> Handle(PickUpDeliveryCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _context.DeliveryOrders.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (delivery is null) return Result.Failure("NotFound", "Delivery order not found.");

        delivery.PickUp(request.Dto.DriverId, request.Dto.Latitude, request.Dto.Longitude);
        delivery.StartTransit();

        var order = await _context.SalesOrders.FirstAsync(o => o.Id == delivery.SalesOrderId, cancellationToken);
        await _syncService.SyncOnPickupAsync(delivery.SalesOrderId, request.Dto.DriverId, order.DeviceId, cancellationToken);

        _context.DeliveryOrders.Update(delivery);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CompleteDeliveryCommandHandler : IRequestHandler<CompleteDeliveryCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IDeliveryOrderSyncService _syncService;
    private readonly DeliveryCodPaymentService _codPayment;

    public CompleteDeliveryCommandHandler(
        IApplicationDbContext context, IDeliveryOrderSyncService syncService,
        DeliveryCodPaymentService codPayment)
        => (_context, _syncService, _codPayment) = (context, syncService, codPayment);

    public async Task<Result> Handle(CompleteDeliveryCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _context.DeliveryOrders.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (delivery is null) return Result.Failure("NotFound", "Delivery order not found.");

        if (delivery.PaymentMode == DeliveryPaymentMode.CashOnDelivery)
        {
            var order = await _context.SalesOrders.AsNoTracking()
                .FirstAsync(o => o.Id == delivery.SalesOrderId, cancellationToken);
            var shiftId = request.Dto.CashierShiftId ?? order.CashierShiftId;
            if (shiftId.HasValue)
            {
                var codResult = await _codPayment.ProcessCodPaymentAsync(
                    delivery, shiftId.Value, request.UserId, cancellationToken);
                if (!codResult.IsSuccess) return codResult;
            }
        }

        delivery.Complete(request.Dto.Latitude, request.Dto.Longitude);

        var salesOrder = await _context.SalesOrders.AsNoTracking()
            .FirstAsync(o => o.Id == delivery.SalesOrderId, cancellationToken);
        await _syncService.SyncOnDeliveredAsync(delivery.SalesOrderId, request.UserId, salesOrder.DeviceId, cancellationToken);

        var driver = delivery.CurrentDriverId.HasValue
            ? await _context.DeliveryDrivers.FirstOrDefaultAsync(d => d.Id == delivery.CurrentDriverId, cancellationToken)
            : null;
        driver?.MarkAvailable();

        _context.DeliveryOrders.Update(delivery);
        if (driver is not null) _context.DeliveryDrivers.Update(driver);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class FailDeliveryCommandHandler : IRequestHandler<FailDeliveryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public FailDeliveryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(FailDeliveryCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _context.DeliveryOrders.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (delivery is null) return Result.Failure("NotFound", "Delivery order not found.");

        delivery.Fail(request.Dto.Reason);

        if (delivery.CurrentDriverId.HasValue)
        {
            var driver = await _context.DeliveryDrivers.FirstOrDefaultAsync(d => d.Id == delivery.CurrentDriverId, cancellationToken);
            driver?.MarkAvailable();
            if (driver is not null) _context.DeliveryDrivers.Update(driver);
        }

        _context.DeliveryOrders.Update(delivery);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CancelDeliveryCommandHandler : IRequestHandler<CancelDeliveryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CancelDeliveryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(CancelDeliveryCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _context.DeliveryOrders.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (delivery is null) return Result.Failure("NotFound", "Delivery order not found.");

        delivery.Cancel(request.Dto.Reason);
        _context.DeliveryOrders.Update(delivery);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
