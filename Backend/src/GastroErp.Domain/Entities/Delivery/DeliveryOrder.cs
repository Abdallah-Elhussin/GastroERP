using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Delivery;

namespace GastroErp.Domain.Entities.Delivery;

/// <summary>
/// DeliveryOrder — طلب توصيل (Aggregate Root)
/// يرتبط بـ SalesOrder فقط — بدون تكرار بنود الطلب.
/// </summary>
public sealed class DeliveryOrder : AuditableBaseEntity, ITenantEntity, ICompanyEntity, IBranchEntity
{
    private static readonly Dictionary<DeliveryStatus, HashSet<DeliveryStatus>> AllowedTransitions = new()
    {
        [DeliveryStatus.Pending] = [DeliveryStatus.Assigned, DeliveryStatus.Cancelled],
        [DeliveryStatus.Assigned] = [DeliveryStatus.PickedUp, DeliveryStatus.Cancelled, DeliveryStatus.Failed],
        [DeliveryStatus.PickedUp] = [DeliveryStatus.InTransit, DeliveryStatus.Failed],
        [DeliveryStatus.InTransit] = [DeliveryStatus.Delivered, DeliveryStatus.Failed],
        [DeliveryStatus.Delivered] = [],
        [DeliveryStatus.Failed] = [DeliveryStatus.Pending],
        [DeliveryStatus.Cancelled] = []
    };

    public Guid TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public string DeliveryNumber { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public DeliveryPriority Priority { get; private set; }
    public DeliveryPaymentMode PaymentMode { get; private set; }
    public DeliveryProviderType ProviderType { get; private set; }
    public string? ExternalProviderCode { get; private set; }
    public string? ExternalOrderReference { get; private set; }

    public string CustomerName { get; private set; }
    public string CustomerPhone { get; private set; }
    public string DeliveryAddress { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? City { get; private set; }
    public string? DeliveryNotes { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }

    public Guid? DeliveryZoneId { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public int EstimatedMinutes { get; private set; }
    public bool IsReadyForPickup { get; private set; }

    public Guid? CurrentDriverId { get; private set; }
    public DateTimeOffset? ScheduledAt { get; private set; }
    public DateTimeOffset? AssignedAt { get; private set; }
    public DateTimeOffset? PickedUpAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public DateTimeOffset? FailedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private readonly List<DeliveryAssignment> _assignments = [];
    public IReadOnlyCollection<DeliveryAssignment> Assignments => _assignments.AsReadOnly();

    private readonly List<DeliveryTrackingEvent> _trackingEvents = [];
    public IReadOnlyCollection<DeliveryTrackingEvent> TrackingEvents => _trackingEvents.AsReadOnly();

    private DeliveryOrder()
    {
        DeliveryNumber = string.Empty;
        CustomerName = string.Empty;
        CustomerPhone = string.Empty;
        DeliveryAddress = string.Empty;
    }

    public static DeliveryOrder Create(
        Guid tenantId, Guid companyId, Guid branchId, Guid salesOrderId, string deliveryNumber,
        string customerName, string customerPhone, string deliveryAddress,
        DeliveryPaymentMode paymentMode, DeliveryPriority priority = DeliveryPriority.Normal,
        DeliveryProviderType providerType = DeliveryProviderType.Internal,
        decimal deliveryFee = 0, int estimatedMinutes = 30,
        Guid? deliveryZoneId = null, string? addressLine2 = null, string? city = null,
        string? deliveryNotes = null, decimal? latitude = null, decimal? longitude = null,
        DateTimeOffset? scheduledAt = null, string? externalProviderCode = null,
        string? externalOrderReference = null)
    {
        if (salesOrderId == Guid.Empty) throw new ArgumentException("SalesOrderId cannot be empty.", nameof(salesOrderId));
        if (string.IsNullOrWhiteSpace(customerName)) throw new BusinessException(ErrorCodes.RequiredField);
        if (string.IsNullOrWhiteSpace(customerPhone)) throw new BusinessException(ErrorCodes.RequiredField);
        if (string.IsNullOrWhiteSpace(deliveryAddress)) throw new BusinessException(ErrorCodes.DeliveryAddressRequired);

        var order = new DeliveryOrder
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            SalesOrderId = salesOrderId,
            DeliveryNumber = deliveryNumber,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            DeliveryAddress = deliveryAddress,
            AddressLine2 = addressLine2,
            City = city,
            DeliveryNotes = deliveryNotes,
            Latitude = latitude,
            Longitude = longitude,
            PaymentMode = paymentMode,
            Priority = priority,
            ProviderType = providerType,
            ExternalProviderCode = externalProviderCode,
            ExternalOrderReference = externalOrderReference,
            DeliveryZoneId = deliveryZoneId,
            DeliveryFee = deliveryFee,
            EstimatedMinutes = estimatedMinutes,
            ScheduledAt = scheduledAt,
            Status = DeliveryStatus.Pending
        };

        order.RecordTracking(DeliveryStatus.Pending, "Delivery order created.");
        order.RaiseDomainEvent(new DeliveryOrderCreatedEvent(order.Id, salesOrderId, tenantId, branchId, providerType));
        return order;
    }

    public void MarkReadyForPickup()
    {
        if (IsReadyForPickup) return;
        IsReadyForPickup = true;
        RecordTracking(Status, "Order ready for pickup from kitchen.");
        RaiseDomainEvent(new DeliveryReadyForPickupEvent(Id, SalesOrderId));
    }

    public void AssignDriver(Guid driverId)
    {
        EnsureTransition(DeliveryStatus.Assigned);
        CurrentDriverId = driverId;
        AssignedAt = DateTimeOffset.UtcNow;
        Status = DeliveryStatus.Assigned;
        _assignments.Add(new DeliveryAssignment(Id, driverId));
        RecordTracking(DeliveryStatus.Assigned, $"Driver assigned: {driverId}");
        RaiseDomainEvent(new DeliveryAssignedEvent(Id, SalesOrderId, driverId));
    }

    public void PickUp(Guid driverId, decimal? latitude = null, decimal? longitude = null)
    {
        if (CurrentDriverId != driverId)
            throw new BusinessException(ErrorCodes.DeliveryDriverMismatch);
        EnsureTransition(DeliveryStatus.PickedUp);

        PickedUpAt = DateTimeOffset.UtcNow;
        Status = DeliveryStatus.PickedUp;
        if (latitude.HasValue) Latitude = latitude;
        if (longitude.HasValue) Longitude = longitude;

        var assignment = _assignments.LastOrDefault(a => a.DriverId == driverId);
        assignment?.MarkPickedUp();

        RecordTracking(DeliveryStatus.PickedUp, "Order picked up by driver.");
        RaiseDomainEvent(new DeliveryPickedUpEvent(Id, SalesOrderId, driverId));
    }

    public void StartTransit()
    {
        EnsureTransition(DeliveryStatus.InTransit);
        Status = DeliveryStatus.InTransit;
        RecordTracking(DeliveryStatus.InTransit, "Order in transit.");
    }

    public void Complete(decimal? latitude = null, decimal? longitude = null)
    {
        EnsureTransition(DeliveryStatus.Delivered);
        DeliveredAt = DateTimeOffset.UtcNow;
        Status = DeliveryStatus.Delivered;
        if (latitude.HasValue) Latitude = latitude;
        if (longitude.HasValue) Longitude = longitude;

        var assignment = _assignments.LastOrDefault(a => a.DriverId == CurrentDriverId);
        assignment?.MarkDelivered();

        RecordTracking(DeliveryStatus.Delivered, "Order delivered.");
        RaiseDomainEvent(new DeliveryCompletedEvent(Id, SalesOrderId, DeliveryFee));
    }

    public void Fail(string reason)
    {
        if (Status is DeliveryStatus.Delivered or DeliveryStatus.Cancelled)
            throw new BusinessException(ErrorCodes.DeliveryInvalidStatusTransition);

        FailedAt = DateTimeOffset.UtcNow;
        FailureReason = reason;
        Status = DeliveryStatus.Failed;
        RecordTracking(DeliveryStatus.Failed, reason);
        RaiseDomainEvent(new DeliveryFailedEvent(Id, SalesOrderId, reason));
    }

    public void Cancel(string reason)
    {
        if (Status is DeliveryStatus.Delivered)
            throw new BusinessException(ErrorCodes.DeliveryCannotBeCancelled);
        Status = DeliveryStatus.Cancelled;
        RecordTracking(DeliveryStatus.Cancelled, reason);
    }

    public void SetExternalReference(string providerCode, string externalReference)
    {
        ExternalProviderCode = providerCode;
        ExternalOrderReference = externalReference;
        ProviderType = DeliveryProviderType.External;
    }

    private void EnsureTransition(DeliveryStatus target)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(target))
            throw new BusinessException(ErrorCodes.DeliveryInvalidStatusTransition);
    }

    private void RecordTracking(DeliveryStatus status, string notes) =>
        _trackingEvents.Add(new DeliveryTrackingEvent(Id, status, notes));
}

public sealed class DeliveryAssignment : AuditableBaseEntity
{
    public Guid DeliveryOrderId { get; private set; }
    public Guid DriverId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
    public DateTimeOffset? PickedUpAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }

    private DeliveryAssignment() { }

    internal DeliveryAssignment(Guid deliveryOrderId, Guid driverId)
    {
        DeliveryOrderId = deliveryOrderId;
        DriverId = driverId;
        AssignedAt = DateTimeOffset.UtcNow;
    }

    internal void MarkPickedUp() => PickedUpAt = DateTimeOffset.UtcNow;
    internal void MarkDelivered() => DeliveredAt = DateTimeOffset.UtcNow;
}

public sealed class DeliveryTrackingEvent : AuditableBaseEntity
{
    public Guid DeliveryOrderId { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public string Notes { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private DeliveryTrackingEvent() { Notes = string.Empty; }

    internal DeliveryTrackingEvent(Guid deliveryOrderId, DeliveryStatus status, string notes)
    {
        DeliveryOrderId = deliveryOrderId;
        Status = status;
        Notes = notes;
        OccurredAt = DateTimeOffset.UtcNow;
    }
}
