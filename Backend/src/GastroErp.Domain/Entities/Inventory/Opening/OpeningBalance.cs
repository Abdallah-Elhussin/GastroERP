using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Opening;

/// <summary>رصيد افتتاحي للمخزون — Aggregate Root</summary>
public sealed class OpeningBalance : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }

    /// <summary>مخزن افتراضي للأسطر (اختياري إن وُجد مخزن على كل سطر).</summary>
    public Guid? WarehouseId { get; private set; }

    public string DocumentNumber { get; private set; }
    public DateTimeOffset DocumentDate { get; private set; }
    public DateTimeOffset? ApprovalDate { get; private set; }
    public string? Notes { get; private set; }

    public OpeningBalanceStatus Status { get; private set; }
    public OpeningBalanceEntryMethod EntryMethod { get; private set; }
    public OpeningBalanceDisplayMethod DisplayMethod { get; private set; }
    public InventoryCostingMethod CostingMethod { get; private set; }
    public WeightedAverageScope WeightedAverageScope { get; private set; }

    public bool UseExpiryDate { get; private set; }
    public bool UseBatchNumbers { get; private set; }
    public bool UseSerialNumbers { get; private set; }

    public Guid? ContraAccountId { get; private set; }
    public Guid? CostCenterId { get; private set; }

    public bool IsApproved => Status is OpeningBalanceStatus.Approved or OpeningBalanceStatus.Posted;
    public bool IsPosted => Status == OpeningBalanceStatus.Posted;

    private readonly List<OpeningBalanceLine> _lines = [];
    public IReadOnlyCollection<OpeningBalanceLine> Lines => _lines.AsReadOnly();

    private OpeningBalance() { DocumentNumber = string.Empty; }

    public OpeningBalance(
        Guid tenantId,
        string documentNumber,
        DateTimeOffset? documentDate = null,
        Guid? warehouseId = null,
        string? notes = null,
        OpeningBalanceEntryMethod entryMethod = OpeningBalanceEntryMethod.Manual,
        OpeningBalanceDisplayMethod displayMethod = OpeningBalanceDisplayMethod.ByItem,
        InventoryCostingMethod costingMethod = InventoryCostingMethod.WeightedAverage,
        WeightedAverageScope weightedAverageScope = WeightedAverageScope.Warehouse,
        bool useExpiryDate = false,
        bool useBatchNumbers = false,
        bool useSerialNumbers = false,
        Guid? contraAccountId = null,
        Guid? costCenterId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(documentNumber)) throw new ArgumentException("DocumentNumber is required.", nameof(documentNumber));

        TenantId = tenantId;
        WarehouseId = warehouseId;
        DocumentNumber = documentNumber.Trim();
        DocumentDate = documentDate ?? DateTimeOffset.UtcNow;
        Notes = notes?.Trim();
        EntryMethod = entryMethod;
        DisplayMethod = displayMethod;
        CostingMethod = costingMethod;
        WeightedAverageScope = weightedAverageScope;
        UseExpiryDate = useExpiryDate;
        UseBatchNumbers = useBatchNumbers;
        UseSerialNumbers = useSerialNumbers;
        ContraAccountId = contraAccountId;
        CostCenterId = costCenterId;
        Status = OpeningBalanceStatus.Draft;
    }

    public void UpdateHeader(
        DateTimeOffset documentDate,
        Guid? warehouseId,
        string? notes,
        OpeningBalanceEntryMethod entryMethod,
        OpeningBalanceDisplayMethod displayMethod,
        InventoryCostingMethod costingMethod,
        WeightedAverageScope weightedAverageScope,
        bool useExpiryDate,
        bool useBatchNumbers,
        bool useSerialNumbers,
        Guid? contraAccountId,
        Guid? costCenterId)
    {
        EnsureDraft();
        DocumentDate = documentDate;
        WarehouseId = warehouseId;
        Notes = notes?.Trim();
        EntryMethod = entryMethod;
        DisplayMethod = displayMethod;
        CostingMethod = costingMethod == InventoryCostingMethod.WeightedAverage
            ? costingMethod
            : InventoryCostingMethod.WeightedAverage;
        WeightedAverageScope = weightedAverageScope;
        UseExpiryDate = useExpiryDate;
        UseBatchNumbers = useBatchNumbers;
        UseSerialNumbers = useSerialNumbers;
        ContraAccountId = contraAccountId;
        CostCenterId = costCenterId;
    }

    public void SetDocumentNumber(string documentNumber)
    {
        EnsureDraft();
        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new ArgumentException("DocumentNumber is required.", nameof(documentNumber));
        DocumentNumber = documentNumber.Trim();
    }

    public OpeningBalanceLine AddLine(
        Guid inventoryItemId,
        Guid unitId,
        decimal quantity,
        decimal unitCost,
        Guid? warehouseId = null,
        string? batchNumber = null,
        DateTimeOffset? expiryDate = null,
        string? serialNumber = null)
    {
        EnsureDraft();
        if (inventoryItemId == Guid.Empty) throw new ArgumentException("InventoryItemId is required.", nameof(inventoryItemId));
        if (unitId == Guid.Empty) throw new ArgumentException("UnitId is required.", nameof(unitId));
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (unitCost < 0) throw new ArgumentOutOfRangeException(nameof(unitCost));

        var lineWarehouse = warehouseId ?? WarehouseId;
        if (lineWarehouse is null || lineWarehouse == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);

        if (UseBatchNumbers && string.IsNullOrWhiteSpace(batchNumber))
            throw new BusinessException(ErrorCodes.RequiredField);
        if (UseExpiryDate && !expiryDate.HasValue)
            throw new BusinessException(ErrorCodes.RequiredField);
        if (UseSerialNumbers && string.IsNullOrWhiteSpace(serialNumber))
            throw new BusinessException(ErrorCodes.RequiredField);

        var line = new OpeningBalanceLine(
            TenantId, Id, inventoryItemId, unitId, quantity, unitCost,
            lineWarehouse.Value, batchNumber, expiryDate, serialNumber);
        _lines.Add(line);
        return line;
    }

    public void UpdateLine(
        Guid lineId,
        Guid inventoryItemId,
        Guid unitId,
        decimal quantity,
        decimal unitCost,
        Guid warehouseId,
        string? batchNumber,
        DateTimeOffset? expiryDate,
        string? serialNumber)
    {
        EnsureDraft();
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        line.Update(inventoryItemId, unitId, quantity, unitCost, warehouseId, batchNumber, expiryDate, serialNumber);
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureDraft();
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _lines.Remove(line);
    }

    public void ClearLines()
    {
        EnsureDraft();
        _lines.Clear();
    }

    public void ReplaceLines(IEnumerable<(Guid ItemId, Guid UnitId, decimal Qty, decimal Cost, Guid WarehouseId, string? Batch, DateTimeOffset? Expiry, string? Serial)> lines)
    {
        EnsureDraft();
        _lines.Clear();
        foreach (var l in lines)
            AddLine(l.ItemId, l.UnitId, l.Qty, l.Cost, l.WarehouseId, l.Batch, l.Expiry, l.Serial);
    }

    public void Approve()
    {
        if (Status != OpeningBalanceStatus.Draft)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField);
        Status = OpeningBalanceStatus.Approved;
        ApprovalDate = DateTimeOffset.UtcNow;
    }

    public void Unapprove()
    {
        if (Status != OpeningBalanceStatus.Approved)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = OpeningBalanceStatus.Draft;
        ApprovalDate = null;
    }

    public void MarkPosted()
    {
        if (Status != OpeningBalanceStatus.Approved)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (!ContraAccountId.HasValue || ContraAccountId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);
        Status = OpeningBalanceStatus.Posted;
    }

    private void EnsureDraft()
    {
        if (Status != OpeningBalanceStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
    }
}

public sealed class OpeningBalanceLine : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid OpeningBalanceId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid UnitId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public string? BatchNumber { get; private set; }
    public DateTimeOffset? ExpiryDate { get; private set; }
    public string? SerialNumber { get; private set; }

    private OpeningBalanceLine() { }

    internal OpeningBalanceLine(
        Guid tenantId,
        Guid openingBalanceId,
        Guid inventoryItemId,
        Guid unitId,
        decimal quantity,
        decimal unitCost,
        Guid warehouseId,
        string? batchNumber,
        DateTimeOffset? expiryDate,
        string? serialNumber)
    {
        TenantId = tenantId;
        OpeningBalanceId = openingBalanceId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        Quantity = quantity;
        UnitCost = unitCost;
        WarehouseId = warehouseId;
        BatchNumber = batchNumber?.Trim();
        ExpiryDate = expiryDate;
        SerialNumber = serialNumber?.Trim();
    }

    internal void Update(
        Guid inventoryItemId,
        Guid unitId,
        decimal quantity,
        decimal unitCost,
        Guid warehouseId,
        string? batchNumber,
        DateTimeOffset? expiryDate,
        string? serialNumber)
    {
        if (inventoryItemId == Guid.Empty) throw new ArgumentException("InventoryItemId is required.", nameof(inventoryItemId));
        if (unitId == Guid.Empty) throw new ArgumentException("UnitId is required.", nameof(unitId));
        if (warehouseId == Guid.Empty) throw new ArgumentException("WarehouseId is required.", nameof(warehouseId));
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (unitCost < 0) throw new ArgumentOutOfRangeException(nameof(unitCost));

        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        Quantity = quantity;
        UnitCost = unitCost;
        WarehouseId = warehouseId;
        BatchNumber = batchNumber?.Trim();
        ExpiryDate = expiryDate;
        SerialNumber = serialNumber?.Trim();
    }
}
