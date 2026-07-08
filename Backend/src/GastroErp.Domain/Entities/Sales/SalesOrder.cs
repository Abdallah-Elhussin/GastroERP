using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Sales;

namespace GastroErp.Domain.Entities.Sales;

/// <summary>
/// SalesOrder — طلب البيع (Aggregate Root)
/// </summary>
public sealed class SalesOrder : AuditableBaseEntity, ITenantEntity, ICompanyEntity, IBranchEntity
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.Draft] = [OrderStatus.Pending, OrderStatus.Cancelled],
        [OrderStatus.Pending] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Preparing, OrderStatus.Cancelled],
        [OrderStatus.Preparing] = [OrderStatus.Ready, OrderStatus.Cancelled],
        [OrderStatus.Ready] = [OrderStatus.Served, OrderStatus.Cancelled],
        [OrderStatus.Served] = [OrderStatus.Completed],
        [OrderStatus.Completed] = [OrderStatus.Served, OrderStatus.Archived],
        [OrderStatus.Cancelled] = [],
        [OrderStatus.Archived] = []
    };

    public Guid TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid BranchId { get; private set; }
    public string OrderNumber { get; private set; }
    public SalesChannel SalesChannel { get; private set; }
    public OrderType OrderType { get; private set; }
    public OrderStatus Status { get; private set; }
    public Guid? TableId { get; private set; }
    public Guid CashierId { get; private set; }
    public Guid? WaiterId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid DeviceId { get; private set; }
    public int? GuestCount { get; private set; }
    public string? Notes { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal ServiceChargeTotal { get; private set; }
    public decimal GrandTotal { get; private set; }
    public string Currency { get; private set; }
    public Guid? PriceLevelId { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public SyncStatus SyncStatus { get; private set; }
    public DateTimeOffset LocalCreatedAt { get; private set; }
    public decimal PaidAmount { get; private set; }
    public OrderPaymentStatus PaymentStatus { get; private set; }
    public Guid? CashierShiftId { get; private set; }
    public Guid? DeliveryOrderId { get; private set; }

    public decimal RemainingBalance => Math.Max(0, GrandTotal - PaidAmount);

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<OrderDiscount> _discounts = [];
    public IReadOnlyCollection<OrderDiscount> Discounts => _discounts.AsReadOnly();

    private readonly List<OrderTax> _taxes = [];
    public IReadOnlyCollection<OrderTax> Taxes => _taxes.AsReadOnly();

    private readonly List<OrderStatusHistory> _statusHistory = [];
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    private SalesOrder()
    {
        OrderNumber = string.Empty;
        Currency = "SAR";
    }

    public static SalesOrder Create(
        Guid tenantId, Guid companyId, Guid branchId, Guid deviceId, Guid cashierId,
        OrderType orderType, SalesChannel salesChannel, string orderNumber,
        string currency = "SAR", Guid? tableId = null, int? guestCount = null,
        Guid? waiterId = null, Guid? priceLevelId = null, string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (companyId == Guid.Empty) throw new ArgumentException("CompanyId cannot be empty.", nameof(companyId));
        if (branchId == Guid.Empty) throw new ArgumentException("BranchId cannot be empty.", nameof(branchId));
        if (deviceId == Guid.Empty) throw new ArgumentException("DeviceId cannot be empty.", nameof(deviceId));
        if (cashierId == Guid.Empty) throw new ArgumentException("CashierId cannot be empty.", nameof(cashierId));
        if (string.IsNullOrWhiteSpace(orderNumber)) throw new ArgumentException("OrderNumber cannot be empty.", nameof(orderNumber));
        if (orderType == OrderType.DineIn && tableId is null)
            throw new BusinessException(ErrorCodes.TableRequired);

        var order = new SalesOrder
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            DeviceId = deviceId,
            CashierId = cashierId,
            WaiterId = waiterId,
            OrderType = orderType,
            SalesChannel = salesChannel,
            OrderNumber = orderNumber,
            Currency = currency.ToUpperInvariant(),
            TableId = tableId,
            GuestCount = guestCount,
            PriceLevelId = priceLevelId,
            Notes = notes,
            Status = OrderStatus.Draft,
            SyncStatus = SyncStatus.Local,
            LocalCreatedAt = DateTimeOffset.UtcNow,
            PaymentStatus = OrderPaymentStatus.Unpaid,
            PaidAmount = 0
        };

        order.RaiseDomainEvent(new OrderCreatedEvent(order.Id, branchId, tenantId, orderType, salesChannel));
        return order;
    }

    public OrderItem AddItem(
        Guid productId, string productNameAr, string? productNameEn, string? sku,
        decimal quantity, decimal unitPrice, string currency, string? notes = null,
        Guid? comboMealId = null)
    {
        EnsureModifiable();

        if (productId == Guid.Empty) throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0) throw new ArgumentException("UnitPrice cannot be negative.", nameof(unitPrice));

        var lineNumber = _items.Count + 1;
        var item = new OrderItem(Id, productId, comboMealId, productNameAr, productNameEn, sku,
            quantity, unitPrice, currency, lineNumber, notes);
        _items.Add(item);
        RecalculateTotals();
        return item;
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureModifiable();
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new BusinessException(ErrorCodes.SalesItemNotFound);
        if (item.IsVoided) throw new BusinessException(ErrorCodes.ItemAlreadyVoided);
        _items.Remove(item);
        RecalculateTotals();
    }

    public void VoidItem(Guid itemId, string reason, Guid voidedBy)
    {
        EnsureModifiable();
        if (string.IsNullOrWhiteSpace(reason)) throw new BusinessException(ErrorCodes.VoidReasonRequired);

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new BusinessException(ErrorCodes.SalesItemNotFound);
        item.Void(reason);
        RecalculateTotals();
        RaiseDomainEvent(new OrderItemVoidedEvent(Id, itemId, reason));
    }

    public void ApplyDiscount(DiscountType discountType, decimal value, Guid appliedBy, string? description = null)
    {
        EnsureModifiable();
        if (value <= 0) throw new ArgumentException("Discount value must be greater than zero.", nameof(value));
        if (discountType == DiscountType.Percentage && value > 100)
            throw new ArgumentException("Percentage discount cannot exceed 100.", nameof(value));

        var amount = discountType == DiscountType.Percentage
            ? Math.Round(SubTotal * value / 100m, 4)
            : value;

        _discounts.Add(new OrderDiscount(Id, discountType, amount, Currency, appliedBy, description));
        RecalculateTotals();
    }

    public void ApplyTax(string taxNameAr, string? taxNameEn, decimal rate, decimal taxableAmount, bool isInclusive)
    {
        EnsureModifiable();
        if (rate < 0) throw new ArgumentException("Tax rate cannot be negative.", nameof(rate));

        var taxAmount = Math.Round(taxableAmount * rate / 100m, 4);
        _taxes.Add(new OrderTax(Id, taxNameAr, taxNameEn, rate, taxableAmount, taxAmount, Currency, isInclusive));
        RecalculateTotals();
    }

    public void ApplyDeliveryFee(decimal fee)
    {
        EnsureModifiable();
        if (fee < 0) throw new ArgumentException("Delivery fee cannot be negative.", nameof(fee));
        ServiceChargeTotal = fee;
        RecalculateTotals();
    }

    public void LinkDeliveryOrder(Guid deliveryOrderId)
    {
        if (OrderType != OrderType.Delivery)
            throw new BusinessException(ErrorCodes.DeliveryOrderTypeRequired);
        DeliveryOrderId = deliveryOrderId;
    }

    public void SyncForDeliveryAssigned(Guid changedBy, Guid deviceId)
    {
        if (Status == OrderStatus.Confirmed)
            StartPreparing(changedBy, deviceId);
    }

    public void SyncForDeliveryPickup(Guid changedBy, Guid deviceId)
    {
        if (Status is OrderStatus.Ready or OrderStatus.Preparing or OrderStatus.Confirmed)
            MarkServed(changedBy, deviceId);
    }

    public void SyncForDeliveryComplete(Guid changedBy, Guid deviceId)
    {
        if (Status != OrderStatus.Completed)
            Complete(changedBy, deviceId);
    }

    public void Submit(Guid submittedBy, Guid deviceId)
    {
        TransitionTo(OrderStatus.Pending, submittedBy, deviceId);
        RaiseDomainEvent(new OrderSubmittedEvent(Id, _items.Count(i => !i.IsVoided), GrandTotal));
    }

    public void Confirm(Guid confirmedBy, Guid deviceId)
    {
        if (!_items.Any(i => !i.IsVoided))
            throw new BusinessException(ErrorCodes.OrderHasNoItems);

        TransitionTo(OrderStatus.Confirmed, confirmedBy, deviceId);
        ConfirmedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new OrderConfirmedEvent(Id, BranchId, TenantId));
    }

    public void StartPreparing(Guid changedBy, Guid deviceId) =>
        TransitionTo(OrderStatus.Preparing, changedBy, deviceId);

    public void MarkReady(Guid changedBy, Guid deviceId) =>
        TransitionTo(OrderStatus.Ready, changedBy, deviceId);

    public void MarkServed(Guid changedBy, Guid deviceId) =>
        TransitionTo(OrderStatus.Served, changedBy, deviceId);

    public void Complete(Guid completedBy, Guid deviceId)
    {
        TransitionTo(OrderStatus.Completed, completedBy, deviceId);
        CompletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new OrderCompletedEvent(Id, GrandTotal, Currency, CompletedAt.Value));
    }

    public void Cancel(string reason, Guid cancelledBy, Guid deviceId)
    {
        if (Status == OrderStatus.Completed)
            throw new BusinessException(ErrorCodes.OrderCannotBeCancelled);
        if (Status == OrderStatus.Cancelled)
            return;

        var previous = Status;
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;
        CancellationReason = reason;
        RecordStatusChange(previous, OrderStatus.Cancelled, cancelledBy, deviceId, reason);
        RaiseDomainEvent(new OrderCancelledEvent(Id, reason, cancelledBy));
    }

    public void Reopen(string reason, Guid reopenedBy, Guid deviceId)
    {
        if (Status != OrderStatus.Completed)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        if (CompletedAt.HasValue && DateTimeOffset.UtcNow - CompletedAt.Value > TimeSpan.FromHours(24))
            throw new BusinessException(ErrorCodes.ReopenWindowExpired);

        TransitionTo(OrderStatus.Served, reopenedBy, deviceId, reason);
        CompletedAt = null;
        RaiseDomainEvent(new OrderReopenedEvent(Id, reopenedBy, reason));
    }

    public void Archive(Guid archivedBy, Guid deviceId) =>
        TransitionTo(OrderStatus.Archived, archivedBy, deviceId);

    public void EnsureCanAcceptPayment()
    {
        if (Status == OrderStatus.Cancelled)
            throw new BusinessException(ErrorCodes.OrderCannotAcceptPayment);
        if (Status == OrderStatus.Draft)
            throw new BusinessException(ErrorCodes.OrderCannotAcceptPayment);
        if (PaymentStatus == OrderPaymentStatus.Paid)
            throw new BusinessException(ErrorCodes.PaymentExceedsBalance);
    }

    public void RecordPayment(decimal amount, Guid cashierShiftId)
    {
        EnsureCanAcceptPayment();
        if (amount <= 0) throw new BusinessException(ErrorCodes.InvalidPaymentAmount);
        if (amount > RemainingBalance)
            throw new BusinessException(ErrorCodes.PaymentExceedsBalance);

        PaidAmount += amount;
        CashierShiftId = cashierShiftId;
        UpdatePaymentStatus();
    }

    public void ReversePayment(decimal amount)
    {
        if (amount <= 0) throw new BusinessException(ErrorCodes.InvalidPaymentAmount);
        if (amount > PaidAmount) throw new BusinessException(ErrorCodes.RefundExceedsPaid);

        PaidAmount -= amount;
        UpdatePaymentStatus();
    }

    private void UpdatePaymentStatus()
    {
        PaymentStatus = PaidAmount switch
        {
            0 => OrderPaymentStatus.Unpaid,
            _ when PaidAmount >= GrandTotal => OrderPaymentStatus.Paid,
            _ => OrderPaymentStatus.PartiallyPaid
        };
    }

    public bool CanTransitionTo(OrderStatus target) =>
        AllowedTransitions.TryGetValue(Status, out var allowed) && allowed.Contains(target);

    private void TransitionTo(OrderStatus target, Guid changedBy, Guid deviceId, string? reason = null)
    {
        if (!CanTransitionTo(target))
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);

        var previous = Status;
        Status = target;
        RecordStatusChange(previous, target, changedBy, deviceId, reason);
        RaiseDomainEvent(new OrderStatusChangedEvent(Id, previous, target, changedBy));
    }

    private void RecordStatusChange(OrderStatus from, OrderStatus to, Guid changedBy, Guid deviceId, string? reason) =>
        _statusHistory.Add(new OrderStatusHistory(Id, from, to, changedBy, deviceId, reason));

    private void EnsureModifiable()
    {
        if (Status is OrderStatus.Completed or OrderStatus.Cancelled or OrderStatus.Archived)
            throw new BusinessException(ErrorCodes.OrderAlreadyClosed);
    }

    private void RecalculateTotals()
    {
        var activeItems = _items.Where(i => !i.IsVoided).ToList();
        SubTotal = activeItems.Sum(i => i.LineTotal);
        DiscountTotal = _discounts.Sum(d => d.Amount);
        TaxTotal = _taxes.Sum(t => t.TaxAmount);
        GrandTotal = Math.Max(0, SubTotal - DiscountTotal + TaxTotal + ServiceChargeTotal);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class OrderItem : AuditableBaseEntity
{
    public Guid SalesOrderId { get; private set; }
    public int LineNumber { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ComboMealId { get; private set; }
    public string ProductNameAr { get; private set; }
    public string? ProductNameEn { get; private set; }
    public string? Sku { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineDiscount { get; private set; }
    public decimal LineTax { get; private set; }
    public decimal LineTotal { get; private set; }
    public string Currency { get; private set; }
    public string? Notes { get; private set; }
    public KitchenItemStatus KitchenStatus { get; private set; }
    public bool IsVoided { get; private set; }
    public string? VoidReason { get; private set; }

    private readonly List<OrderLineModifier> _modifiers = [];
    public IReadOnlyCollection<OrderLineModifier> Modifiers => _modifiers.AsReadOnly();

    private OrderItem()
    {
        ProductNameAr = string.Empty;
        Currency = "SAR";
    }

    internal OrderItem(Guid salesOrderId, Guid productId, Guid? comboMealId,
        string productNameAr, string? productNameEn, string? sku,
        decimal quantity, decimal unitPrice, string currency, int lineNumber, string? notes)
    {
        SalesOrderId = salesOrderId;
        ProductId = productId;
        ComboMealId = comboMealId;
        ProductNameAr = productNameAr;
        ProductNameEn = productNameEn;
        Sku = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Currency = currency.ToUpperInvariant();
        LineNumber = lineNumber;
        Notes = notes;
        KitchenStatus = KitchenItemStatus.Pending;
        RecalculateLineTotal();
    }

    public void AddModifier(Guid modifierId, string nameAr, string? nameEn, decimal extraPrice, int quantity = 1)
    {
        if (IsVoided) throw new BusinessException(ErrorCodes.ItemAlreadyVoided);
        _modifiers.Add(new OrderLineModifier(Id, modifierId, nameAr, nameEn, extraPrice, quantity));
        RecalculateLineTotal();
    }

    public void Void(string reason)
    {
        if (IsVoided) throw new BusinessException(ErrorCodes.ItemAlreadyVoided);
        IsVoided = true;
        VoidReason = reason;
        KitchenStatus = KitchenItemStatus.Voided;
        LineTotal = 0;
    }

    public void UpdateKitchenStatus(KitchenItemStatus status) => KitchenStatus = status;

    private void RecalculateLineTotal()
    {
        var modifierTotal = _modifiers.Sum(m => m.ExtraPrice * m.Quantity);
        LineTotal = Math.Round((Quantity * UnitPrice) + modifierTotal - LineDiscount + LineTax, 4);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class OrderLineModifier : AuditableBaseEntity
{
    public Guid OrderItemId { get; private set; }
    public Guid ModifierId { get; private set; }
    public string ModifierNameAr { get; private set; }
    public string? ModifierNameEn { get; private set; }
    public decimal ExtraPrice { get; private set; }
    public int Quantity { get; private set; }

    private OrderLineModifier() { ModifierNameAr = string.Empty; }

    internal OrderLineModifier(Guid orderItemId, Guid modifierId, string nameAr, string? nameEn, decimal extraPrice, int quantity)
    {
        OrderItemId = orderItemId;
        ModifierId = modifierId;
        ModifierNameAr = nameAr;
        ModifierNameEn = nameEn;
        ExtraPrice = extraPrice;
        Quantity = quantity > 0 ? quantity : 1;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class OrderDiscount : AuditableBaseEntity
{
    public Guid SalesOrderId { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public string? Description { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public Guid AppliedBy { get; private set; }

    private OrderDiscount() { Currency = "SAR"; }

    internal OrderDiscount(Guid salesOrderId, DiscountType discountType, decimal amount, string currency, Guid appliedBy, string? description)
    {
        SalesOrderId = salesOrderId;
        DiscountType = discountType;
        Amount = amount;
        Currency = currency;
        AppliedBy = appliedBy;
        Description = description;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class OrderTax : AuditableBaseEntity
{
    public Guid SalesOrderId { get; private set; }
    public string TaxNameAr { get; private set; }
    public string? TaxNameEn { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxableAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public string Currency { get; private set; }
    public bool IsInclusive { get; private set; }

    private OrderTax() { TaxNameAr = string.Empty; Currency = "SAR"; }

    internal OrderTax(Guid salesOrderId, string taxNameAr, string? taxNameEn, decimal rate,
        decimal taxableAmount, decimal taxAmount, string currency, bool isInclusive)
    {
        SalesOrderId = salesOrderId;
        TaxNameAr = taxNameAr;
        TaxNameEn = taxNameEn;
        TaxRate = rate;
        TaxableAmount = taxableAmount;
        TaxAmount = taxAmount;
        Currency = currency;
        IsInclusive = isInclusive;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class OrderStatusHistory : BaseEntity
{
    public Guid SalesOrderId { get; private set; }
    public OrderStatus FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public Guid ChangedBy { get; private set; }
    public string? Reason { get; private set; }
    public Guid DeviceId { get; private set; }

    private OrderStatusHistory() { }

    internal OrderStatusHistory(Guid salesOrderId, OrderStatus from, OrderStatus to,
        Guid changedBy, Guid deviceId, string? reason)
    {
        SalesOrderId = salesOrderId;
        FromStatus = from;
        ToStatus = to;
        ChangedBy = changedBy;
        DeviceId = deviceId;
        Reason = reason;
        ChangedAt = DateTimeOffset.UtcNow;
    }
}
