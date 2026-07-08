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
    
    /// <summary>رقم المستند المرجعي (مثلاً رقم فاتورة الشراء أو رقم التحويل)</summary>
    public string ReferenceDocumentNumber { get; private set; }
    
    /// <summary>معرف المستند المرجعي الأصلي (GoodsReceiptId, WasteRecordId, etc.)</summary>
    public Guid ReferenceDocumentId { get; private set; }
    
    public DateTimeOffset TransactionDate { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<StockMovement> _movements = [];
    public IReadOnlyCollection<StockMovement> Movements => _movements.AsReadOnly();

    private InventoryTransaction() { ReferenceDocumentNumber = string.Empty; }

    public InventoryTransaction(Guid tenantId, TransactionType transactionType, string referenceDocumentNumber, Guid referenceDocumentId, string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (referenceDocumentId == Guid.Empty) throw new ArgumentException("ReferenceDocumentId cannot be empty.", nameof(referenceDocumentId));

        TenantId = tenantId;
        TransactionType = transactionType;
        ReferenceDocumentNumber = referenceDocumentNumber;
        ReferenceDocumentId = referenceDocumentId;
        TransactionDate = DateTimeOffset.UtcNow;
        Notes = notes;
    }

    /// <summary>إضافة حركة مخزنية (Append-Only Ledger)</summary>
    public void AddMovement(Guid inventoryItemId, Guid warehouseId, Guid? warehouseBinId, decimal quantityChange, decimal unitCost, Guid? batchId = null)
    {
        if (quantityChange == 0) throw new ArgumentException("Quantity change cannot be zero.", nameof(quantityChange));

        _movements.Add(new StockMovement(TenantId, Id, inventoryItemId, warehouseId, warehouseBinId, quantityChange, unitCost, batchId));
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// سجل حركة المخزون غير القابل للتعديل (Append-Only Ledger)
/// </summary>
public sealed class StockMovement
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    public Guid InventoryTransactionId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid? WarehouseBinId { get; private set; }
    
    /// <summary>التغير في الكمية (موجب للاستلام، سالب للصرف)</summary>
    public decimal QuantityChange { get; private set; }
    
    /// <summary>تكلفة الوحدة وقت الحركة بناءً على طريقة التقييم (FIFO, WA, etc.)</summary>
    public decimal UnitCost { get; private set; }
    
    /// <summary>إجمالي تكلفة الحركة (QuantityChange * UnitCost)</summary>
    public decimal TotalCost => QuantityChange * UnitCost;

    public Guid? InventoryBatchId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private StockMovement() { }

    internal StockMovement(Guid tenantId, Guid transactionId, Guid inventoryItemId, Guid warehouseId, Guid? warehouseBinId, decimal quantityChange, decimal unitCost, Guid? batchId)
    {
        TenantId = tenantId;
        InventoryTransactionId = transactionId;
        InventoryItemId = inventoryItemId;
        WarehouseId = warehouseId;
        WarehouseBinId = warehouseBinId;
        QuantityChange = quantityChange;
        UnitCost = unitCost;
        InventoryBatchId = batchId;
    }
}
