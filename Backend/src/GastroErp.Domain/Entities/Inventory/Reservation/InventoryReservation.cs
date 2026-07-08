using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Inventory;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Inventory.Reservation;

/// <summary>
/// حجز المخزون (Aggregate Root)
/// يحجز كمية من المخزون مسبقاً قبل عملية البيع أو الصرف الفعلي
/// (Available = OnHand - Reserved)
/// </summary>
public sealed class InventoryReservation : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    
    /// <summary>الكمية المحجوزة</summary>
    public decimal ReservedQuantity { get; private set; }
    
    /// <summary>المستند المرجعي للحجز (مثال: رقم طلب البيع، أمر إنتاج)</summary>
    public string SourceDocument { get; private set; }
    
    public ReservationStatus Status { get; private set; }
    
    /// <summary>متى ينتهي الحجز تلقائياً إذا لم يتم تنفيذه (صرفه)</summary>
    public DateTimeOffset? ExpirationDate { get; private set; }

    private InventoryReservation() { SourceDocument = string.Empty; }

    public InventoryReservation(Guid tenantId, Guid warehouseId, Guid inventoryItemId, decimal reservedQuantity, string sourceDocument, DateTimeOffset? expirationDate = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (warehouseId == Guid.Empty) throw new ArgumentException("WarehouseId cannot be empty.", nameof(warehouseId));
        if (inventoryItemId == Guid.Empty) throw new ArgumentException("InventoryItemId cannot be empty.", nameof(inventoryItemId));
        if (reservedQuantity <= 0) throw new ArgumentException("Reserved quantity must be greater than zero.", nameof(reservedQuantity));
        if (string.IsNullOrWhiteSpace(sourceDocument)) throw new ArgumentException("SourceDocument cannot be empty.", nameof(sourceDocument));

        TenantId = tenantId;
        WarehouseId = warehouseId;
        InventoryItemId = inventoryItemId;
        ReservedQuantity = reservedQuantity;
        SourceDocument = sourceDocument;
        ExpirationDate = expirationDate;
        Status = ReservationStatus.Active;

        RaiseDomainEvent(new StockReservedEvent(Id, InventoryItemId, WarehouseId, ReservedQuantity, TenantId));
    }

    public void MarkAsFulfilled()
    {
        if (Status != ReservationStatus.Active) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = ReservationStatus.Fulfilled;
    }

    public void Cancel()
    {
        if (Status != ReservationStatus.Active) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = ReservationStatus.Cancelled;
    }

    public void Expire()
    {
        if (Status != ReservationStatus.Active) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = ReservationStatus.Expired;
    }
}
