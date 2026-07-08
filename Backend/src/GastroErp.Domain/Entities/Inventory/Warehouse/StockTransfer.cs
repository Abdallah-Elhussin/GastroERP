using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Inventory.Warehouse;
/// <summary>
/// النقل المخزني بين المستودعات (Aggregate Root)
/// </summary>
public sealed class StockTransfer : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid SourceWarehouseId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public string TransferNumber { get; private set; }
    public DateTimeOffset TransferDate { get; private set; }
    public StockTransferStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<StockTransferLine> _lines = [];
    public IReadOnlyCollection<StockTransferLine> Lines => _lines.AsReadOnly();

    private StockTransfer() { TransferNumber = string.Empty; }

    public StockTransfer(Guid tenantId, Guid sourceWarehouseId, Guid destinationWarehouseId, string transferNumber, string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (sourceWarehouseId == Guid.Empty) throw new ArgumentException("SourceWarehouseId cannot be empty.", nameof(sourceWarehouseId));
        if (destinationWarehouseId == Guid.Empty) throw new ArgumentException("DestinationWarehouseId cannot be empty.", nameof(destinationWarehouseId));
        if (sourceWarehouseId == destinationWarehouseId) throw new ArgumentException("Source and Destination cannot be the same.");

        TenantId = tenantId;
        SourceWarehouseId = sourceWarehouseId;
        DestinationWarehouseId = destinationWarehouseId;
        TransferNumber = transferNumber;
        TransferDate = DateTimeOffset.UtcNow;
        Notes = notes;
        Status = StockTransferStatus.Draft;
    }

    public void AddLine(Guid inventoryItemId, Guid unitId, decimal quantity, string? batchNumber = null)
    {
        if (Status != StockTransferStatus.Draft) throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
        _lines.Add(new StockTransferLine(TenantId, Id, inventoryItemId, unitId, quantity, batchNumber));
    }

    public void SubmitForApproval()
    {
        if (Status != StockTransferStatus.Draft) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (_lines.Count == 0) throw new BusinessException(ErrorCodes.RequiredField);
        RaiseDomainEvent(new Domain.Events.Inventory.StockTransferSubmittedEvent(Id, TenantId));
    }

    public void MarkAsInTransit()
    {
        if (Status != StockTransferStatus.Draft) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = StockTransferStatus.InTransit;
    }

    public void Complete()
    {
        if (Status != StockTransferStatus.InTransit && Status != StockTransferStatus.Draft) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = StockTransferStatus.Completed;
    }

    public void Cancel() => Status = StockTransferStatus.Cancelled;
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class StockTransferLine : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid StockTransferId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid UnitId { get; private set; }
    public decimal Quantity { get; private set; }
    public string? BatchNumber { get; private set; }

    private StockTransferLine() { }

    internal StockTransferLine(Guid tenantId, Guid transferId, Guid inventoryItemId, Guid unitId, decimal quantity, string? batchNumber)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        TenantId = tenantId;
        StockTransferId = transferId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        Quantity = quantity;
        BatchNumber = batchNumber;
    }
}
