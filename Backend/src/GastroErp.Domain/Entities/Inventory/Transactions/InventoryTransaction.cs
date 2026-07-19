using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Transactions;

/// <summary>
/// الطبقة الوسيطة للمعاملات المخزنية (Aggregate Root)
/// </summary>
public sealed class InventoryTransaction : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public string ReferenceDocumentNumber { get; private set; }
    public Guid ReferenceDocumentId { get; private set; }
    public DateTimeOffset TransactionDate { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<StockMovement> _movements = [];
    public IReadOnlyCollection<StockMovement> Movements => _movements.AsReadOnly();

    private InventoryTransaction() { ReferenceDocumentNumber = string.Empty; }

    public InventoryTransaction(
        Guid tenantId,
        TransactionType transactionType,
        string referenceDocumentNumber,
        Guid referenceDocumentId,
        string? notes = null,
        DateTimeOffset? transactionDate = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (referenceDocumentId == Guid.Empty) throw new ArgumentException("ReferenceDocumentId cannot be empty.", nameof(referenceDocumentId));

        TenantId = tenantId;
        TransactionType = transactionType;
        ReferenceDocumentNumber = referenceDocumentNumber;
        ReferenceDocumentId = referenceDocumentId;
        TransactionDate = transactionDate ?? DateTimeOffset.UtcNow;
        Notes = notes;
    }

    /// <summary>
    /// Append-only movement. Quantity is always positive; direction comes from MovementType only.
    /// </summary>
    public StockMovement AddMovement(
        Guid inventoryItemId,
        Guid warehouseId,
        Guid? warehouseBinId,
        decimal quantity,
        InventoryMovementType movementType,
        decimal unitCost,
        Guid? batchId = null,
        bool adjIncreasesOnHand = true)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        var movement = new StockMovement(
            TenantId,
            Id,
            inventoryItemId,
            warehouseId,
            warehouseBinId,
            quantity,
            movementType,
            unitCost,
            batchId,
            adjIncreasesOnHand);
        _movements.Add(movement);
        RaiseDomainEvent(new Domain.Events.Inventory.StockMovementRecordedEvent(
            movement.Id,
            Id,
            inventoryItemId,
            warehouseId,
            movement.QuantityChange,
            TenantId));
        return movement;
    }
}

/// <summary>
/// Append-only ledger line. External contract: positive Quantity + MovementType.
/// QuantityChange is derived internally for read-model aggregation only.
/// </summary>
public sealed class StockMovement
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    public Guid InventoryTransactionId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid? WarehouseBinId { get; private set; }

    /// <summary>Always positive.</summary>
    public decimal Quantity { get; private set; }

    public InventoryMovementType MovementType { get; private set; }

    /// <summary>ADJ/REV only: whether the movement increases on-hand.</summary>
    public bool AdjIncreasesOnHand { get; private set; }

    /// <summary>
    /// Signed delta persisted for SQL aggregation. Never set by handlers — derived from MovementType.
    /// </summary>
    public decimal QuantityChange { get; private set; }

    public decimal UnitCost { get; private set; }
    public decimal TotalCost => Quantity * UnitCost;

    public Guid? InventoryBatchId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private StockMovement() { }

    internal StockMovement(
        Guid tenantId,
        Guid transactionId,
        Guid inventoryItemId,
        Guid warehouseId,
        Guid? warehouseBinId,
        decimal quantity,
        InventoryMovementType movementType,
        decimal unitCost,
        Guid? batchId,
        bool adjIncreasesOnHand)
    {
        TenantId = tenantId;
        InventoryTransactionId = transactionId;
        InventoryItemId = inventoryItemId;
        WarehouseId = warehouseId;
        WarehouseBinId = warehouseBinId;
        Quantity = quantity;
        MovementType = movementType;
        UnitCost = unitCost;
        InventoryBatchId = batchId;
        AdjIncreasesOnHand = adjIncreasesOnHand;
        QuantityChange = ComputeSignedDelta(quantity, movementType, adjIncreasesOnHand);
    }

    private static decimal ComputeSignedDelta(
        decimal quantity,
        InventoryMovementType movementType,
        bool adjIncreasesOnHand) =>
        movementType switch
        {
            InventoryMovementType.IN => quantity,
            InventoryMovementType.TRI => quantity,
            InventoryMovementType.OUT => -quantity,
            InventoryMovementType.TRO => -quantity,
            InventoryMovementType.ADJ => adjIncreasesOnHand ? quantity : -quantity,
            InventoryMovementType.REV => adjIncreasesOnHand ? quantity : -quantity,
            _ => throw new ArgumentOutOfRangeException(nameof(movementType))
        };
}
