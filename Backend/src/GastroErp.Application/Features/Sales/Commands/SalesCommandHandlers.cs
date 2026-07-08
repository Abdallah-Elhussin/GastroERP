using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.Commands;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Services;
using GastroErp.Application.Features.Invoicing.Services;
using GastroErp.Application.Features.Delivery.Services;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Sales.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly ITableService _tableService;
    private readonly IDeliveryOrderFactory _deliveryOrderFactory;

    public CreateOrderCommandHandler(
        IApplicationDbContext context, IMapper mapper,
        IOrderNumberGenerator orderNumberGenerator,
        ILogger<CreateOrderCommandHandler> logger,
        ITableService tableService,
        IDeliveryOrderFactory deliveryOrderFactory)
        => (_context, _mapper, _orderNumberGenerator, _logger, _tableService, _deliveryOrderFactory) =
            (context, mapper, orderNumberGenerator, logger, tableService, deliveryOrderFactory);

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Dto.BranchId && b.TenantId == request.TenantId, cancellationToken);

        if (branch is null)
            return Result<OrderDto>.Failure("NotFound", "Branch not found.");

        var deviceExists = await _context.Devices
            .AnyAsync(d => d.Id == request.Dto.DeviceId && d.TenantId == request.TenantId, cancellationToken);

        if (!deviceExists)
            return Result<OrderDto>.Failure("NotFound", "Device not found.");

        var orderNumber = await _orderNumberGenerator.GenerateAsync(branch.Id, branch.Code, cancellationToken);

        var order = SalesOrder.Create(
            request.TenantId,
            branch.CompanyId,
            branch.Id,
            request.Dto.DeviceId,
            request.CashierId,
            request.Dto.OrderType,
            request.Dto.SalesChannel,
            orderNumber,
            "SAR",
            request.Dto.TableId,
            request.Dto.GuestCount,
            request.Dto.WaiterId,
            request.Dto.PriceLevelId,
            request.Dto.Notes);

        _context.SalesOrders.Add(order);

        if (request.Dto.TableId.HasValue)
            await _tableService.OccupyTableAsync(request.Dto.TableId.Value, order.Id, cancellationToken);

        if (request.Dto.OrderType == OrderType.Delivery)
        {
            if (request.Dto.Delivery is null)
                return Result<OrderDto>.Failure("DeliveryAddressRequired", "Delivery details are required for delivery orders.");

            var deliveryResult = await _deliveryOrderFactory.CreateForSalesOrderAsync(
                request.TenantId, branch.CompanyId, branch.Id, order.Id,
                request.Dto.Delivery, cancellationToken);

            if (!deliveryResult.IsSuccess)
                return Result<OrderDto>.Failure(deliveryResult.ErrorCode!, deliveryResult.ErrorMessage!);

            var delivery = deliveryResult.Data!;
            order.ApplyDeliveryFee(delivery.DeliveryFee);
            order.LinkDeliveryOrder(delivery.Id);
            _context.DeliveryOrders.Add(delivery);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Sales order created: {OrderId} ({OrderNumber})", order.Id, order.OrderNumber);
        return Result<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }
}

public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand, Result<OrderItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMenuPricingService _pricingService;

    public AddOrderItemCommandHandler(
        IApplicationDbContext context, IMapper mapper,
        IMenuPricingService pricingService)
        => (_context, _mapper, _pricingService) = (context, mapper, pricingService);

    public async Task<Result<OrderItemDto>> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Items).ThenInclude(i => i.Modifiers)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result<OrderItemDto>.Failure("NotFound", "Order not found.");

        var snapshot = await _pricingService.GetProductPriceAsync(
            request.Dto.ProductId, order.TenantId, order.PriceLevelId, cancellationToken);

        if (snapshot is null)
            return Result<OrderItemDto>.Failure("NotFound", "Product not found.");

        if (!snapshot.IsAvailable)
            return Result<OrderItemDto>.Failure("ProductNotAvailable", "Product is not available.");

        var item = order.AddItem(
            snapshot.ProductId, snapshot.NameAr, snapshot.NameEn, snapshot.Sku,
            request.Dto.Quantity, snapshot.UnitPrice, snapshot.Currency, request.Dto.Notes);

        if (request.Dto.Modifiers is not null)
        {
            foreach (var mod in request.Dto.Modifiers)
            {
                var modSnapshot = await _pricingService.GetModifierPriceAsync(mod.ModifierId, order.TenantId, cancellationToken);
                if (modSnapshot is null)
                    return Result<OrderItemDto>.Failure("NotFound", $"Modifier not found: {mod.ModifierId}");

                item.AddModifier(modSnapshot.ModifierId, modSnapshot.NameAr, modSnapshot.NameEn,
                    modSnapshot.ExtraPrice, mod.Quantity);
            }
        }

        _context.SalesOrders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<OrderItemDto>.Success(_mapper.Map<OrderItemDto>(item));
    }
}

public class RemoveOrderItemCommandHandler : IRequestHandler<RemoveOrderItemCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveOrderItemCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null) return Result.Failure("NotFound", "Order not found.");

        order.RemoveItem(request.ItemId);
        _context.SalesOrders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class VoidOrderItemCommandHandler : IRequestHandler<VoidOrderItemCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public VoidOrderItemCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
        => (_context, _currentUser) = (context, currentUser);

    public async Task<Result> Handle(VoidOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null) return Result.Failure("NotFound", "Order not found.");

        var userId = _currentUser.Id ?? Guid.Empty;
        order.VoidItem(request.ItemId, request.Dto.Reason, userId);
        _context.SalesOrders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ApplyOrderDiscountCommandHandler : IRequestHandler<ApplyOrderDiscountCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ApplyOrderDiscountCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
        => (_context, _currentUser) = (context, currentUser);

    public async Task<Result> Handle(ApplyOrderDiscountCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null) return Result.Failure("NotFound", "Order not found.");

        var userId = _currentUser.Id ?? Guid.Empty;
        order.ApplyDiscount(request.Dto.DiscountType, request.Dto.Value, userId, request.Dto.Description);
        _context.SalesOrders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SubmitOrderCommandHandler : IRequestHandler<SubmitOrderCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SubmitOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SubmitOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null) return Result.Failure("NotFound", "Order not found.");

        order.Submit(request.UserId, request.DeviceId);
        _context.SalesOrders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrderInventoryService _inventoryService;
    private readonly IKitchenRoutingService _kitchenRoutingService;

    public ConfirmOrderCommandHandler(
        IApplicationDbContext context,
        IOrderInventoryService inventoryService,
        IKitchenRoutingService kitchenRoutingService)
        => (_context, _inventoryService, _kitchenRoutingService) = (context, inventoryService, kitchenRoutingService);

    public async Task<Result> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Items).ThenInclude(i => i.Modifiers)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null) return Result.Failure("NotFound", "Order not found.");

        order.Confirm(request.UserId, request.DeviceId);

        await _inventoryService.ReserveStockForOrderAsync(
            new SalesOrderContext(
                order.TenantId,
                order.BranchId,
                order.OrderNumber,
                order.Items.Where(i => !i.IsVoided)
                    .Select(i => new SalesOrderItemContext(i.ProductId, i.Quantity))
                    .ToList()),
            cancellationToken);

        await _kitchenRoutingService.RouteOrderAsync(order.Id, cancellationToken);

        _context.SalesOrders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null) return Result.Failure("NotFound", "Order not found.");

        switch (request.Dto.TargetStatus)
        {
            case OrderStatus.Preparing:
                order.StartPreparing(request.UserId, request.DeviceId);
                break;
            case OrderStatus.Ready:
                order.MarkReady(request.UserId, request.DeviceId);
                break;
            case OrderStatus.Served:
                order.MarkServed(request.UserId, request.DeviceId);
                break;
            default:
                return Result.Failure("InvalidStatus", $"Cannot transition to {request.Dto.TargetStatus} via this endpoint.");
        }

        _context.SalesOrders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ITableService _tableService;
    private readonly IInvoiceGenerationService _invoiceGenerationService;
    private readonly IAutoPostingService _autoPostingService;

    public CompleteOrderCommandHandler(
        IApplicationDbContext context, ITableService tableService,
        IInvoiceGenerationService invoiceGenerationService,
        IAutoPostingService autoPostingService)
        => (_context, _tableService, _invoiceGenerationService, _autoPostingService)
            = (context, tableService, invoiceGenerationService, autoPostingService);

    public async Task<Result> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null) return Result.Failure("NotFound", "Order not found.");

        order.Complete(request.UserId, request.DeviceId);
        await _tableService.ReleaseTableForOrderAsync(order.TableId, cancellationToken);

        if (order.OrderType != OrderType.Delivery)
        {
            var invoiceResult = await _invoiceGenerationService.EnsureInvoiceForCompletedOrderAsync(order.Id, cancellationToken);
            if (!invoiceResult.IsSuccess)
                return Result.Failure(invoiceResult.ErrorCode!, invoiceResult.ErrorMessage!);
        }

        _context.SalesOrders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);

        var postingResult = await _autoPostingService.PostSalesOrderCompletedAsync(order.Id, request.UserId, cancellationToken);
        if (!postingResult.IsSuccess)
            return Result.Failure(postingResult.ErrorCode!, postingResult.ErrorMessage!);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrderInventoryService _inventoryService;
    private readonly ITableService _tableService;

    public CancelOrderCommandHandler(
        IApplicationDbContext context,
        IOrderInventoryService inventoryService,
        ITableService tableService)
        => (_context, _inventoryService, _tableService) = (context, inventoryService, tableService);

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null) return Result.Failure("NotFound", "Order not found.");

        order.Cancel(request.Dto.Reason, request.UserId, request.DeviceId);
        await _inventoryService.ReleaseStockForOrderAsync(order.OrderNumber, cancellationToken);
        await _tableService.ReleaseTableForOrderAsync(order.TableId, cancellationToken);

        _context.SalesOrders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ReopenOrderCommandHandler : IRequestHandler<ReopenOrderCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ReopenOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ReopenOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null) return Result.Failure("NotFound", "Order not found.");

        order.Reopen(request.Dto.Reason, request.UserId, request.DeviceId);
        _context.SalesOrders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
