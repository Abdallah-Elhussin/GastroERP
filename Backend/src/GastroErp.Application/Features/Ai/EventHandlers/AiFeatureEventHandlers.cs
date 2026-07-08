using GastroErp.Application.Common.Notifications;
using GastroErp.Application.Features.Ai.Services;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Ai.EventHandlers;

public sealed class OrderCompletedFeatureUpdateHandler : INotificationHandler<DomainEventNotification<OrderCompletedEvent>>
{
    private readonly IFeatureComputationService _features;
    private readonly GastroErp.Application.Common.Interfaces.IApplicationDbContext _context;

    public OrderCompletedFeatureUpdateHandler(
        IFeatureComputationService features,
        GastroErp.Application.Common.Interfaces.IApplicationDbContext context)
        => (_features, _context) = (features, context);

    public async Task Handle(DomainEventNotification<OrderCompletedEvent> notification, CancellationToken ct)
    {
        var order = await _context.SalesOrders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == notification.DomainEvent.OrderId, ct);
        if (order is null) return;

        await _features.ComputeGroupAsync(order.TenantId, AiFeatureGroup.SalesVelocity, ct);
    }
}

public sealed class PaymentCompletedFeatureUpdateHandler : INotificationHandler<DomainEventNotification<PaymentCompletedEvent>>
{
    private readonly IFeatureComputationService _features;
    private readonly GastroErp.Application.Common.Interfaces.IApplicationDbContext _context;

    public PaymentCompletedFeatureUpdateHandler(
        IFeatureComputationService features,
        GastroErp.Application.Common.Interfaces.IApplicationDbContext context)
        => (_features, _context) = (features, context);

    public async Task Handle(DomainEventNotification<PaymentCompletedEvent> notification, CancellationToken ct)
    {
        var order = await _context.SalesOrders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == notification.DomainEvent.OrderId, ct);
        if (order is null) return;

        await _features.ComputeGroupAsync(order.TenantId, AiFeatureGroup.CustomerRfm, ct);
    }
}
