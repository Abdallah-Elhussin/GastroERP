using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Sales;

namespace GastroErp.Domain.Entities.Sales;

/// <summary>KitchenTicket — تذكرة المطبخ (Aggregate Root)</summary>
public sealed class KitchenTicket : AuditableBaseEntity, ITenantEntity, IBranchEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public string TicketNumber { get; private set; }
    public Guid KitchenStationId { get; private set; }
    public KitchenTicketStatus Status { get; private set; }
    public int Priority { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public int? EstimatedPrepMinutes { get; private set; }

    private readonly List<KitchenTicketItem> _items = [];
    public IReadOnlyCollection<KitchenTicketItem> Items => _items.AsReadOnly();

    private KitchenTicket() { TicketNumber = string.Empty; }

    public static KitchenTicket Create(
        Guid tenantId, Guid branchId, Guid salesOrderId, Guid kitchenStationId,
        string ticketNumber, int priority = 0, int? estimatedPrepMinutes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (salesOrderId == Guid.Empty) throw new ArgumentException("SalesOrderId cannot be empty.", nameof(salesOrderId));
        if (kitchenStationId == Guid.Empty) throw new ArgumentException("KitchenStationId cannot be empty.", nameof(kitchenStationId));

        var ticket = new KitchenTicket
        {
            TenantId = tenantId,
            BranchId = branchId,
            SalesOrderId = salesOrderId,
            KitchenStationId = kitchenStationId,
            TicketNumber = ticketNumber,
            Status = KitchenTicketStatus.Pending,
            Priority = priority,
            EstimatedPrepMinutes = estimatedPrepMinutes
        };

        ticket.RaiseDomainEvent(new KitchenTicketCreatedEvent(ticket.Id, salesOrderId, kitchenStationId, branchId));
        return ticket;
    }

    public void AddItem(Guid orderItemId, string productNameAr, string? productNameEn, decimal quantity, string? modifiersSummary)
    {
        if (Status is KitchenTicketStatus.Completed or KitchenTicketStatus.Cancelled)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);

        _items.Add(new KitchenTicketItem(Id, orderItemId, productNameAr, productNameEn, quantity, modifiersSummary));
    }

    public void Start()
    {
        if (Status != KitchenTicketStatus.Pending)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);

        Status = KitchenTicketStatus.InProgress;
        StartedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new KitchenTicketStartedEvent(Id, StartedAt.Value));
    }

    public void MarkReady()
    {
        if (Status != KitchenTicketStatus.InProgress)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = KitchenTicketStatus.Ready;
    }

    public void Complete()
    {
        if (Status is not (KitchenTicketStatus.InProgress or KitchenTicketStatus.Ready))
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);

        Status = KitchenTicketStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        var prepMinutes = StartedAt.HasValue
            ? (int?)(CompletedAt.Value - StartedAt.Value).TotalMinutes
            : null;
        RaiseDomainEvent(new KitchenTicketCompletedEvent(Id, CompletedAt.Value, prepMinutes));
    }

    public void Cancel()
    {
        if (Status == KitchenTicketStatus.Completed) return;
        Status = KitchenTicketStatus.Cancelled;
    }

    public void MarkItemReady(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new BusinessException(ErrorCodes.SalesItemNotFound);
        item.MarkReady();
        RaiseDomainEvent(new KitchenItemReadyEvent(Id, itemId));
    }
}

public sealed class KitchenTicketItem : AuditableBaseEntity
{
    public Guid KitchenTicketId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public string ProductNameAr { get; private set; }
    public string? ProductNameEn { get; private set; }
    public decimal Quantity { get; private set; }
    public string? ModifiersSummary { get; private set; }
    public KitchenItemStatus Status { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private KitchenTicketItem() { ProductNameAr = string.Empty; }

    internal KitchenTicketItem(Guid ticketId, Guid orderItemId, string nameAr, string? nameEn,
        decimal quantity, string? modifiersSummary)
    {
        KitchenTicketId = ticketId;
        OrderItemId = orderItemId;
        ProductNameAr = nameAr;
        ProductNameEn = nameEn;
        Quantity = quantity;
        ModifiersSummary = modifiersSummary;
        Status = KitchenItemStatus.Pending;
    }

    public void MarkReady()
    {
        Status = KitchenItemStatus.Ready;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
