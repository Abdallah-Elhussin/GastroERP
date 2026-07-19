using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Warehouse;

/// <summary>التحويل المخزني بين المستودعات (Aggregate Root).</summary>
public sealed class StockTransfer : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid SourceWarehouseId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public string TransferNumber { get; private set; }
    public DateTimeOffset TransferDate { get; private set; }
    public StockTransferType TransferType { get; private set; }
    public StockTransferStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<StockTransferLine> _lines = [];
    public IReadOnlyCollection<StockTransferLine> Lines => _lines.AsReadOnly();

    public decimal TotalAmount => _lines.Sum(l => l.LineTotal);

    private StockTransfer()
    {
        TransferNumber = string.Empty;
    }

    public StockTransfer(
        Guid tenantId,
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        string transferNumber,
        DateTimeOffset? transferDate = null,
        StockTransferType transferType = StockTransferType.Outbound,
        string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (sourceWarehouseId == Guid.Empty) throw new ArgumentException("SourceWarehouseId cannot be empty.", nameof(sourceWarehouseId));
        if (destinationWarehouseId == Guid.Empty) throw new ArgumentException("DestinationWarehouseId cannot be empty.", nameof(destinationWarehouseId));
        if (sourceWarehouseId == destinationWarehouseId)
            throw new ArgumentException("Source and Destination cannot be the same.");
        if (string.IsNullOrWhiteSpace(transferNumber))
            throw new ArgumentException("TransferNumber is required.", nameof(transferNumber));

        TenantId = tenantId;
        SourceWarehouseId = sourceWarehouseId;
        DestinationWarehouseId = destinationWarehouseId;
        TransferNumber = transferNumber.Trim();
        TransferDate = transferDate ?? DateTimeOffset.UtcNow;
        TransferType = transferType;
        Notes = notes?.Trim();
        Status = StockTransferStatus.Draft;
    }

    public void UpdateHeader(
        DateTimeOffset transferDate,
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        StockTransferType transferType,
        string? notes)
    {
        EnsureDraft();
        if (sourceWarehouseId == Guid.Empty) throw new ArgumentException("SourceWarehouseId cannot be empty.", nameof(sourceWarehouseId));
        if (destinationWarehouseId == Guid.Empty) throw new ArgumentException("DestinationWarehouseId cannot be empty.", nameof(destinationWarehouseId));
        if (sourceWarehouseId == destinationWarehouseId)
            throw new ArgumentException("Source and Destination cannot be the same.");

        TransferDate = transferDate;
        SourceWarehouseId = sourceWarehouseId;
        DestinationWarehouseId = destinationWarehouseId;
        TransferType = transferType;
        Notes = notes?.Trim();
    }

    public void SetTransferNumber(string transferNumber)
    {
        EnsureDraft();
        if (string.IsNullOrWhiteSpace(transferNumber))
            throw new ArgumentException("TransferNumber is required.", nameof(transferNumber));
        TransferNumber = transferNumber.Trim();
    }

    public StockTransferLine AddLine(
        Guid inventoryItemId,
        Guid unitId,
        decimal quantity,
        decimal unitCost = 0,
        string? batchNumber = null)
    {
        EnsureDraft();
        if (inventoryItemId == Guid.Empty) throw new ArgumentException("InventoryItemId is required.", nameof(inventoryItemId));
        if (unitId == Guid.Empty) throw new ArgumentException("UnitId is required.", nameof(unitId));
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (unitCost < 0) throw new ArgumentOutOfRangeException(nameof(unitCost));

        var line = new StockTransferLine(TenantId, Id, inventoryItemId, unitId, quantity, unitCost, batchNumber);
        _lines.Add(line);
        return line;
    }

    public void ClearLines()
    {
        EnsureDraft();
        _lines.Clear();
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureDraft();
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _lines.Remove(line);
    }

    public void Approve()
    {
        if (Status != StockTransferStatus.Draft)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField);
        Status = StockTransferStatus.Approved;
        RaiseDomainEvent(new Domain.Events.Inventory.StockTransferSubmittedEvent(Id, TenantId));
    }

    public void Unapprove()
    {
        if (Status != StockTransferStatus.Approved)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = StockTransferStatus.Draft;
    }

    /// <summary>ترحيل — خروج من المصدر (InTransit).</summary>
    public void MarkAsInTransit()
    {
        if (Status is not (StockTransferStatus.Approved or StockTransferStatus.Draft))
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField);
        Status = StockTransferStatus.InTransit;
    }

    /// <summary>استلام — دخول إلى الوجهة (Completed).</summary>
    public void Complete()
    {
        if (Status != StockTransferStatus.InTransit)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        foreach (var line in _lines)
            line.MarkFullyReceived();
        Status = StockTransferStatus.Completed;
    }

    public void Cancel()
    {
        if (Status is StockTransferStatus.Completed or StockTransferStatus.Cancelled)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = StockTransferStatus.Cancelled;
    }

    /// <summary>توافق مع مسار الاعتماد القديم.</summary>
    public void SubmitForApproval() => Approve();

    private void EnsureDraft()
    {
        if (Status != StockTransferStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
    }
}

public sealed class StockTransferLine : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid StockTransferId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid UnitId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal LineTotal => Quantity * UnitCost;
    public decimal ReceivedQuantity { get; private set; }
    public string? BatchNumber { get; private set; }

    private StockTransferLine() { }

    internal StockTransferLine(
        Guid tenantId,
        Guid transferId,
        Guid inventoryItemId,
        Guid unitId,
        decimal quantity,
        decimal unitCost,
        string? batchNumber)
    {
        TenantId = tenantId;
        StockTransferId = transferId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        Quantity = quantity;
        UnitCost = unitCost;
        ReceivedQuantity = 0;
        BatchNumber = batchNumber?.Trim();
    }

    internal void MarkFullyReceived() => ReceivedQuantity = Quantity;
}
