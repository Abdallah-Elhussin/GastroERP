using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Realtime;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Services;
using GastroErp.Domain.Entities.Organization;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.Commands;

public record DispatchPosToKitchenCommand(Guid TenantId, Guid UserId, DispatchPosToKitchenDto Dto)
    : IRequest<Result<IReadOnlyList<KdsTicketViewDto>>>;

public sealed class DispatchPosToKitchenCommandHandler(
    IApplicationDbContext context,
    IOrderNumberGenerator orderNumberGenerator,
    IKitchenRoutingService kitchenRoutingService,
    IKdsBoardProjectionService boardProjection,
    IKitchenRealtimeNotifier realtimeNotifier)
    : IRequestHandler<DispatchPosToKitchenCommand, Result<IReadOnlyList<KdsTicketViewDto>>>
{
    public async Task<Result<IReadOnlyList<KdsTicketViewDto>>> Handle(
        DispatchPosToKitchenCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Dto.Items.Count == 0)
            return Result<IReadOnlyList<KdsTicketViewDto>>.Failure("Validation", "Order has no items.");

        var branch = await context.Branches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.TenantId == request.TenantId, cancellationToken);

        if (branch is null)
            return Result<IReadOnlyList<KdsTicketViewDto>>.Failure("NotFound", "Branch not found. Complete restaurant setup first.");

        var device = await context.Devices
            .FirstOrDefaultAsync(d => d.TenantId == request.TenantId && d.DeviceType == DeviceType.POSTerminal, cancellationToken)
            ?? await context.Devices.FirstOrDefaultAsync(d => d.TenantId == request.TenantId, cancellationToken);

        if (device is null)
            return Result<IReadOnlyList<KdsTicketViewDto>>.Failure("NotFound", "POS device not found.");

        var product = await context.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.TenantId == request.TenantId, cancellationToken);

        if (product is null)
            return Result<IReadOnlyList<KdsTicketViewDto>>.Failure("NotFound", "Menu products not found.");

        var orderType = OrderType.TakeAway;
        var salesChannel = request.Dto.OrderType.ToLowerInvariant() switch
        {
            "delivery" => SalesChannel.Delivery,
            "dinein" or "dine_in" or "dine-in" => SalesChannel.DineIn,
            _ => SalesChannel.TakeAway
        };

        var orderNumber = await orderNumberGenerator.GenerateAsync(branch.Id, branch.Code ?? "BR1", cancellationToken);
        var order = SalesOrder.Create(
            request.TenantId,
            branch.CompanyId,
            branch.Id,
            device.Id,
            request.UserId,
            orderType,
            salesChannel,
            orderNumber,
            notes: $"KDS:{request.Dto.TableLabel}");

        foreach (var item in request.Dto.Items)
        {
            var notes = string.IsNullOrWhiteSpace(item.Notes)
                ? item.CategoryKey
                : $"{item.Notes}|{item.CategoryKey}";
            order.AddItem(
                product.Id,
                item.Name,
                item.Name,
                product.SKU,
                item.Quantity,
                product.BasePrice,
                "SAR",
                notes);
        }

        order.Submit(request.UserId, device.Id);
        order.Confirm(request.UserId, device.Id);

        context.SalesOrders.Add(order);
        await kitchenRoutingService.RouteOrderAsync(order.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var createdTickets = await context.KitchenTickets.AsNoTracking()
            .Where(t => t.SalesOrderId == order.Id)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var views = new List<KdsTicketViewDto>();
        foreach (var ticketId in createdTickets)
        {
            var view = await boardProjection.ProjectTicketAsync(ticketId, cancellationToken);
            if (view is not null)
            {
                views.Add(view);
                await realtimeNotifier.NotifyTicketCreatedAsync(view, cancellationToken);
            }
        }

        return Result<IReadOnlyList<KdsTicketViewDto>>.Success(views);
    }
}
