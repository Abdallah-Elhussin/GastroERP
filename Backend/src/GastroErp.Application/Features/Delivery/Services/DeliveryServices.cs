using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Delivery.DTOs;
using GastroErp.Application.Features.Invoicing.Services;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Application.Features.Sales.Services;
using GastroErp.Domain.Entities.Delivery;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Delivery.Services;

public sealed class DeliveryNumberGenerator : IDeliveryNumberGenerator
{
    private readonly IApplicationDbContext _context;

    public DeliveryNumberGenerator(IApplicationDbContext context) => _context = context;

    public async Task<string> GenerateAsync(Guid tenantId, Guid branchId, CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.DeliveryOrders
            .CountAsync(d => d.TenantId == tenantId && d.BranchId == branchId && d.CreatedAt.Year == year, ct);
        return $"DLV-{year}-{(count + 1):D6}";
    }
}

public sealed class DeliveryFeeCalculationService : IDeliveryFeeCalculationService
{
    private readonly IApplicationDbContext _context;

    public DeliveryFeeCalculationService(IApplicationDbContext context) => _context = context;

    public async Task<Result<DeliveryFeeQuoteDto>> CalculateFeeAsync(
        Guid tenantId, Guid branchId, Guid? zoneId, decimal? latitude, decimal? longitude,
        CancellationToken ct = default)
    {
        DeliveryZone? zone = null;

        if (zoneId.HasValue)
        {
            zone = await _context.DeliveryZones.AsNoTracking()
                .FirstOrDefaultAsync(z => z.Id == zoneId && z.TenantId == tenantId && z.IsActive, ct);
        }
        else if (latitude.HasValue && longitude.HasValue)
        {
            var zones = await _context.DeliveryZones.AsNoTracking()
                .Where(z => z.TenantId == tenantId && z.BranchId == branchId && z.IsActive)
                .ToListAsync(ct);
            zone = zones.FirstOrDefault(z => z.ContainsPoint(latitude.Value, longitude.Value));
        }

        if (zone is null)
            return Result<DeliveryFeeQuoteDto>.Failure("DeliveryZoneNotFound", "No delivery zone found for this location.");

        if (!latitude.HasValue || !longitude.HasValue)
            return Result<DeliveryFeeQuoteDto>.Success(
                new DeliveryFeeQuoteDto(zone.Id, zone.NameAr, zone.FixedFee, zone.EstimatedMinutes));

        var fee = zone.CalculateFee(latitude.Value, longitude.Value);
        return Result<DeliveryFeeQuoteDto>.Success(
            new DeliveryFeeQuoteDto(zone.Id, zone.NameAr, fee, zone.EstimatedMinutes));
    }
}

public sealed class DeliveryAssignmentService : IDeliveryAssignmentService
{
    public Task<Result> AssignDriverAsync(DeliveryOrder delivery, DeliveryDriver driver, CancellationToken ct = default)
    {
        if (!driver.IsActive || driver.Status != DriverStatus.Available)
            return Task.FromResult(Result.Failure("DeliveryDriverNotAvailable", "Driver is not available."));

        delivery.AssignDriver(driver.Id);
        driver.MarkOnDelivery();
        return Task.FromResult(Result.Success());
    }
}

public sealed class DeliveryEtaService : IDeliveryEtaService
{
    public int EstimateMinutes(DeliveryOrder delivery, DeliveryZone? zone) =>
        zone?.EstimatedMinutes ?? delivery.EstimatedMinutes;
}

public sealed class DeliveryOrderSyncService : IDeliveryOrderSyncService
{
    private readonly IApplicationDbContext _context;
    private readonly IInvoiceGenerationService _invoiceGeneration;
    private readonly IAutoPostingService _autoPostingService;

    public DeliveryOrderSyncService(
        IApplicationDbContext context, IInvoiceGenerationService invoiceGeneration,
        IAutoPostingService autoPostingService)
        => (_context, _invoiceGeneration, _autoPostingService) = (context, invoiceGeneration, autoPostingService);

    public async Task SyncOnAssignedAsync(Guid salesOrderId, Guid userId, Guid deviceId, CancellationToken ct = default)
    {
        var order = await _context.SalesOrders.FirstOrDefaultAsync(o => o.Id == salesOrderId, ct);
        if (order is null) return;
        order.SyncForDeliveryAssigned(userId, deviceId);
        _context.SalesOrders.Update(order);
    }

    public async Task SyncOnPickupAsync(Guid salesOrderId, Guid userId, Guid deviceId, CancellationToken ct = default)
    {
        var order = await _context.SalesOrders.FirstOrDefaultAsync(o => o.Id == salesOrderId, ct);
        if (order is null) return;
        order.SyncForDeliveryPickup(userId, deviceId);
        _context.SalesOrders.Update(order);
    }

    public async Task SyncOnDeliveredAsync(Guid salesOrderId, Guid userId, Guid deviceId, CancellationToken ct = default)
    {
        var order = await _context.SalesOrders.FirstOrDefaultAsync(o => o.Id == salesOrderId, ct);
        if (order is null) return;
        order.SyncForDeliveryComplete(userId, deviceId);
        await _invoiceGeneration.EnsureInvoiceForCompletedOrderAsync(salesOrderId, ct);
        _context.SalesOrders.Update(order);
        await _autoPostingService.PostSalesOrderCompletedAsync(salesOrderId, userId, ct);
    }
}

public sealed class DeliveryKitchenIntegrationService : IDeliveryKitchenIntegrationService
{
    private readonly IApplicationDbContext _context;

    public DeliveryKitchenIntegrationService(IApplicationDbContext context) => _context = context;

    public async Task CheckAndMarkReadyForPickupAsync(Guid salesOrderId, CancellationToken ct = default)
    {
        var activeStatuses = new[] { KitchenTicketStatus.Pending, KitchenTicketStatus.InProgress };
        var hasActiveTickets = await _context.KitchenTickets
            .AnyAsync(t => t.SalesOrderId == salesOrderId && activeStatuses.Contains(t.Status), ct);

        if (hasActiveTickets) return;

        var hasTickets = await _context.KitchenTickets
            .AnyAsync(t => t.SalesOrderId == salesOrderId, ct);
        if (!hasTickets) return;

        var delivery = await _context.DeliveryOrders
            .FirstOrDefaultAsync(d => d.SalesOrderId == salesOrderId, ct);
        if (delivery is null) return;

        delivery.MarkReadyForPickup();
        _context.DeliveryOrders.Update(delivery);

        var order = await _context.SalesOrders.FirstOrDefaultAsync(o => o.Id == salesOrderId, ct);
        if (order is not null && order.Status == OrderStatus.Preparing)
        {
            order.MarkReady(Guid.Empty, order.DeviceId);
            _context.SalesOrders.Update(order);
        }
    }
}

public sealed class DeliveryOrderFactory : IDeliveryOrderFactory
{
    private readonly IApplicationDbContext _context;
    private readonly IDeliveryNumberGenerator _numberGenerator;
    private readonly IDeliveryFeeCalculationService _feeService;

    public DeliveryOrderFactory(
        IApplicationDbContext context, IDeliveryNumberGenerator numberGenerator,
        IDeliveryFeeCalculationService feeService)
        => (_context, _numberGenerator, _feeService) = (context, numberGenerator, feeService);

    public async Task<Result<DeliveryOrder>> CreateForSalesOrderAsync(
        Guid tenantId, Guid companyId, Guid branchId, Guid salesOrderId,
        CreateOrderDeliveryDto deliveryDto, CancellationToken ct = default)
    {
        var exists = await _context.DeliveryOrders
            .AnyAsync(d => d.SalesOrderId == salesOrderId, ct);
        if (exists)
            return Result<DeliveryOrder>.Failure("DeliveryAlreadyExistsForOrder", "Delivery order already exists.");

        decimal fee = 0;
        int eta = 30;
        Guid? zoneId = deliveryDto.DeliveryZoneId;

        if (deliveryDto.DeliveryZoneId.HasValue || deliveryDto.Address.Latitude.HasValue)
        {
            var feeResult = await _feeService.CalculateFeeAsync(
                tenantId, branchId, deliveryDto.DeliveryZoneId,
                deliveryDto.Address.Latitude, deliveryDto.Address.Longitude, ct);
            if (feeResult.IsSuccess)
            {
                fee = feeResult.Data!.DeliveryFee;
                eta = feeResult.Data.EstimatedMinutes;
                zoneId = feeResult.Data.ZoneId;
            }
        }

        var number = await _numberGenerator.GenerateAsync(tenantId, branchId, ct);
        var addr = deliveryDto.Address;

        var delivery = DeliveryOrder.Create(
            tenantId, companyId, branchId, salesOrderId, number,
            addr.CustomerName, addr.CustomerPhone, addr.DeliveryAddress,
            deliveryDto.PaymentMode, deliveryDto.Priority,
            DeliveryProviderType.Internal, fee, eta, zoneId,
            addr.AddressLine2, addr.City, addr.Notes,
            addr.Latitude, addr.Longitude, deliveryDto.ScheduledAt);

        return Result<DeliveryOrder>.Success(delivery);
    }
}

/// <summary>محول افتراضي — جاهز للتوسعة بمزودين خارجيين</summary>
public sealed class InternalDeliveryProviderAdapter : IDeliveryProviderAdapter
{
    public DeliveryProviderType ProviderType => DeliveryProviderType.Internal;

    public Task<Result<string>> PushOrderAsync(DeliveryOrder delivery, CancellationToken ct = default) =>
        Task.FromResult(Result<string>.Success(delivery.DeliveryNumber));

    public Task<Result> SyncStatusAsync(DeliveryOrder delivery, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}

public sealed class DeliveryCodPaymentService
{
    private readonly IApplicationDbContext _context;
    private readonly IReceiptNumberGenerator _receiptGenerator;
    private readonly ILogger<DeliveryCodPaymentService> _logger;

    public DeliveryCodPaymentService(
        IApplicationDbContext context, IReceiptNumberGenerator receiptGenerator,
        ILogger<DeliveryCodPaymentService> logger)
        => (_context, _receiptGenerator, _logger) = (context, receiptGenerator, logger);

    public async Task<Result> ProcessCodPaymentAsync(
        DeliveryOrder delivery, Guid cashierShiftId, Guid userId, CancellationToken ct = default)
    {
        if (delivery.PaymentMode != DeliveryPaymentMode.CashOnDelivery)
            return Result.Success();

        var order = await _context.SalesOrders.FirstOrDefaultAsync(o => o.Id == delivery.SalesOrderId, ct);
        if (order is null) return Result.Failure("NotFound", "Sales order not found.");
        if (order.PaymentStatus == OrderPaymentStatus.Paid) return Result.Success();

        var amount = order.RemainingBalance;
        if (amount <= 0) return Result.Success();

        var shift = await _context.CashierShifts.FirstOrDefaultAsync(s => s.Id == cashierShiftId, ct);
        if (shift is null) return Result.Failure("NotFound", "Shift not found.");

        var receiptNumber = await _receiptGenerator.GenerateAsync(order.BranchId, ct);
        var payment = Payment.Create(
            order.TenantId, order.BranchId, shift.Id, receiptNumber,
            PaymentMethodType.Cash, amount, order.Currency, userId);

        payment.AllocateToOrder(order.Id, amount);
        payment.Complete(order.Id);
        order.RecordPayment(amount, shift.Id);

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.SalesOrderId == order.Id && i.Status == InvoiceStatus.Finalized, ct);
        invoice?.RecordPayment(amount);

        _context.Payments.Add(payment);
        _context.SalesOrders.Update(order);
        if (invoice is not null) _context.Invoices.Update(invoice);

        _logger.LogInformation("COD payment {Amount} recorded for delivery {DeliveryId}", amount, delivery.Id);
        return Result.Success();
    }
}
