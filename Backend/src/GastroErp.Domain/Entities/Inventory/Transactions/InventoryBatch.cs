using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Transactions;

/// <summary>
/// تتبع التشغيلات والدفعات (Aggregate Root)
/// </summary>
public sealed class InventoryBatch : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    
    /// <summary>رقم التشغيلة المستلم من المورد أو المنتج داخلياً</summary>
    public string BatchNumber { get; private set; }
    
    /// <summary>رقم القطعة (Lot Number) لتتبع أعمق إن لزم</summary>
    public string? LotNumber { get; private set; }

    public DateTimeOffset? ManufacturingDate { get; private set; }
    public DateTimeOffset? ExpirationDate { get; private set; }
    
    public BatchStatus Status { get; private set; }

    private InventoryBatch() { BatchNumber = string.Empty; }

    public InventoryBatch(Guid tenantId, Guid inventoryItemId, string batchNumber, string? lotNumber = null,
                          DateTimeOffset? manufacturingDate = null, DateTimeOffset? expirationDate = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (inventoryItemId == Guid.Empty) throw new ArgumentException("InventoryItemId cannot be empty.", nameof(inventoryItemId));
        if (string.IsNullOrWhiteSpace(batchNumber)) throw new ArgumentException("BatchNumber cannot be empty.", nameof(batchNumber));
        if (expirationDate.HasValue && manufacturingDate.HasValue && expirationDate <= manufacturingDate)
            throw new ArgumentException("Expiration date must be after manufacturing date.", nameof(expirationDate));

        TenantId = tenantId;
        InventoryItemId = inventoryItemId;
        BatchNumber = batchNumber;
        LotNumber = lotNumber;
        ManufacturingDate = manufacturingDate;
        ExpirationDate = expirationDate;
        Status = BatchStatus.Active;
    }

    public void MarkAsDepleted() => Status = BatchStatus.Depleted;
    public void MarkAsExpired() => Status = BatchStatus.Expired;
    public void MarkAsQuarantine() => Status = BatchStatus.Quarantine;
    public void MarkAsRecalled() => Status = BatchStatus.Recalled;
}
