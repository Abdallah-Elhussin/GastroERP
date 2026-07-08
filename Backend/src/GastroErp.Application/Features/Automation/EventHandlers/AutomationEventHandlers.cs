using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Notifications;
using GastroErp.Application.Features.Automation.DTOs;
using GastroErp.Application.Features.Automation.Services;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Crm;
using GastroErp.Domain.Events.Finance;
using GastroErp.Domain.Events.Inventory;
using GastroErp.Domain.Events.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Automation.EventHandlers;

public sealed class OrderCompletedEventHandler : INotificationHandler<DomainEventNotification<OrderCompletedEvent>>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationOrchestrator _notifications;
    private readonly IWebhookDispatchService _webhooks;

    public OrderCompletedEventHandler(
        IApplicationDbContext context, INotificationOrchestrator notifications, IWebhookDispatchService webhooks)
        => (_context, _notifications, _webhooks) = (context, notifications, webhooks);

    public async Task Handle(DomainEventNotification<OrderCompletedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        var order = await _context.SalesOrders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == evt.OrderId, ct);
        if (order is null) return;

        await _notifications.SendAsync(order.TenantId, new SendNotificationDto(
            "Order Completed", $"Order {order.OrderNumber} completed — {evt.GrandTotal:N2} {evt.Currency}",
            NotificationType.OrderDelivered, NotificationChannel.InApp,
            ReferenceType: nameof(SalesOrder), ReferenceId: order.Id), ct);

        await _webhooks.DispatchAsync(order.TenantId, nameof(OrderCompletedEvent), evt, ct);
    }
}

public sealed class PaymentCompletedEventHandler : INotificationHandler<DomainEventNotification<PaymentCompletedEvent>>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationOrchestrator _notifications;
    private readonly IWebhookDispatchService _webhooks;

    public PaymentCompletedEventHandler(
        IApplicationDbContext context, INotificationOrchestrator notifications, IWebhookDispatchService webhooks)
        => (_context, _notifications, _webhooks) = (context, notifications, webhooks);

    public async Task Handle(DomainEventNotification<PaymentCompletedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        var order = await _context.SalesOrders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == evt.OrderId, ct);
        if (order is null) return;

        await _notifications.SendAsync(order.TenantId, new SendNotificationDto(
            "Payment Received", $"Payment of {evt.Amount:N2} {evt.Currency} received.",
            NotificationType.PaymentReceived, NotificationChannel.InApp,
            ReferenceType: nameof(Payment), ReferenceId: evt.PaymentId), ct);

        await _webhooks.DispatchAsync(order.TenantId, nameof(PaymentCompletedEvent), evt, ct);
    }
}

public sealed class JournalPostedEventHandler : INotificationHandler<DomainEventNotification<JournalPostedEvent>>
{
    private readonly INotificationOrchestrator _notifications;
    private readonly IWebhookDispatchService _webhooks;

    public JournalPostedEventHandler(INotificationOrchestrator notifications, IWebhookDispatchService webhooks)
        => (_notifications, _webhooks) = (notifications, webhooks);

    public async Task Handle(DomainEventNotification<JournalPostedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Journal Posted", $"Journal {evt.EntryNumber} posted.",
            NotificationType.JournalPosted, NotificationChannel.InApp,
            ReferenceType: "JournalEntry", ReferenceId: evt.JournalEntryId), ct);

        await _webhooks.DispatchAsync(evt.TenantId, nameof(JournalPostedEvent), evt, ct);
    }
}

public sealed class FiscalPeriodClosedEventHandler : INotificationHandler<DomainEventNotification<FiscalPeriodClosedEvent>>
{
    private readonly INotificationOrchestrator _notifications;
    private readonly IWebhookDispatchService _webhooks;

    public FiscalPeriodClosedEventHandler(INotificationOrchestrator notifications, IWebhookDispatchService webhooks)
        => (_notifications, _webhooks) = (notifications, webhooks);

    public async Task Handle(DomainEventNotification<FiscalPeriodClosedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Fiscal Period Closed", $"Fiscal year {evt.FiscalYear} period closed.",
            NotificationType.FiscalPeriodClosed, NotificationChannel.InApp,
            ReferenceType: "FiscalPeriod", ReferenceId: evt.FiscalPeriodId), ct);

        await _webhooks.DispatchAsync(evt.TenantId, nameof(FiscalPeriodClosedEvent), evt, ct);
    }
}

public sealed class LoyaltyPointsEarnedEventHandler : INotificationHandler<DomainEventNotification<LoyaltyPointsEarnedEvent>>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationOrchestrator _notifications;

    public LoyaltyPointsEarnedEventHandler(IApplicationDbContext context, INotificationOrchestrator notifications)
        => (_context, _notifications) = (context, notifications);

    public async Task Handle(DomainEventNotification<LoyaltyPointsEarnedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        var account = await _context.LoyaltyAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == evt.LoyaltyAccountId, ct);
        if (account is null) return;

        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Loyalty Points", $"You earned {evt.PointsEarned:N0} loyalty points.",
            NotificationType.LoyaltyReward, NotificationChannel.InApp), ct);
    }
}

public sealed class ReorderLevelReachedEventHandler : INotificationHandler<DomainEventNotification<ReorderLevelReachedEvent>>
{
    private readonly INotificationOrchestrator _notifications;

    public ReorderLevelReachedEventHandler(INotificationOrchestrator notifications) => _notifications = notifications;

    public async Task Handle(DomainEventNotification<ReorderLevelReachedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Low Stock", $"Item stock is {evt.CurrentStock:N2} (reorder level reached).",
            NotificationType.LowStock, NotificationChannel.InApp,
            ReferenceType: "InventoryItem", ReferenceId: evt.ItemId), ct);
    }
}

public sealed class OrderCancelledEventHandler : INotificationHandler<DomainEventNotification<OrderCancelledEvent>>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationOrchestrator _notifications;

    public OrderCancelledEventHandler(IApplicationDbContext context, INotificationOrchestrator notifications)
        => (_context, _notifications) = (context, notifications);

    public async Task Handle(DomainEventNotification<OrderCancelledEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        var order = await _context.SalesOrders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == evt.OrderId, ct);
        if (order is null) return;

        await _notifications.SendAsync(order.TenantId, new SendNotificationDto(
            "Order Cancelled", $"Order {order.OrderNumber} was cancelled: {evt.Reason}",
            NotificationType.OrderCancelled, NotificationChannel.InApp,
            ReferenceType: nameof(SalesOrder), ReferenceId: order.Id), ct);
    }
}
