using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Issuing;

/// <summary>صرف مخزني (Goods Issue) — Aggregate Root. Draft → Approved → Posted | Cancelled.</summary>
public sealed class GoodsIssue : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid? WarehouseId { get; private set; }
    public Guid? IssueDestinationId { get; private set; }
    public string IssueNumber { get; private set; }
    public DateTimeOffset IssueDate { get; private set; }
    public DateTimeOffset? ApprovalDate { get; private set; }
    public string Currency { get; private set; }
    public string? Notes { get; private set; }
    public GoodsIssueStatus Status { get; private set; }

    public bool IsConfirmed => Status is GoodsIssueStatus.Approved or GoodsIssueStatus.Posted;
    public bool IsCompleted => Status == GoodsIssueStatus.Posted;

    private readonly List<GoodsIssueLine> _lines = [];
    public IReadOnlyCollection<GoodsIssueLine> Lines => _lines.AsReadOnly();

    public decimal TotalAmount => _lines.Sum(l => l.TotalCost);

    private GoodsIssue()
    {
        IssueNumber = string.Empty;
        Currency = "SAR";
    }

    public GoodsIssue(
        Guid tenantId,
        string issueNumber,
        DateTimeOffset? issueDate = null,
        Guid? warehouseId = null,
        Guid? issueDestinationId = null,
        string currency = "SAR",
        string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(issueNumber)) throw new ArgumentException("IssueNumber is required.", nameof(issueNumber));

        TenantId = tenantId;
        IssueNumber = issueNumber.Trim();
        IssueDate = issueDate ?? DateTimeOffset.UtcNow;
        WarehouseId = warehouseId;
        IssueDestinationId = issueDestinationId;
        Currency = string.IsNullOrWhiteSpace(currency) ? "SAR" : currency.Trim().ToUpperInvariant();
        Notes = notes?.Trim();
        Status = GoodsIssueStatus.Draft;
    }

    public void UpdateHeader(
        DateTimeOffset issueDate,
        Guid? warehouseId,
        Guid? issueDestinationId,
        string currency,
        string? notes)
    {
        EnsureDraft();
        IssueDate = issueDate;
        WarehouseId = warehouseId;
        IssueDestinationId = issueDestinationId;
        Currency = string.IsNullOrWhiteSpace(currency) ? Currency : currency.Trim().ToUpperInvariant();
        Notes = notes?.Trim();
    }

    public void SetIssueNumber(string issueNumber)
    {
        EnsureDraft();
        if (string.IsNullOrWhiteSpace(issueNumber))
            throw new ArgumentException("IssueNumber is required.", nameof(issueNumber));
        IssueNumber = issueNumber.Trim();
    }

    public GoodsIssueLine AddLine(
        Guid inventoryItemId,
        Guid unitId,
        decimal quantity,
        decimal unitCost,
        Guid? warehouseId = null,
        Guid? costCenterId = null,
        string? notes = null)
    {
        EnsureDraft();
        if (inventoryItemId == Guid.Empty) throw new ArgumentException("InventoryItemId is required.", nameof(inventoryItemId));
        if (unitId == Guid.Empty) throw new ArgumentException("UnitId is required.", nameof(unitId));
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (unitCost < 0) throw new ArgumentOutOfRangeException(nameof(unitCost));

        var lineWh = warehouseId ?? WarehouseId;
        if (lineWh is null || lineWh == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);

        var line = new GoodsIssueLine(
            TenantId, Id, inventoryItemId, unitId, quantity, unitCost, lineWh.Value, costCenterId, notes);
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
        if (Status != GoodsIssueStatus.Draft)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField);
        if (!IssueDestinationId.HasValue)
            throw new BusinessException(ErrorCodes.RequiredField);
        Status = GoodsIssueStatus.Approved;
        ApprovalDate = DateTimeOffset.UtcNow;
    }

    public void Unapprove()
    {
        if (Status != GoodsIssueStatus.Approved)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = GoodsIssueStatus.Draft;
        ApprovalDate = null;
    }

    public void MarkPosted()
    {
        if (Status != GoodsIssueStatus.Approved)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = GoodsIssueStatus.Posted;
    }

    public void Cancel()
    {
        if (Status is GoodsIssueStatus.Posted or GoodsIssueStatus.Cancelled)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = GoodsIssueStatus.Cancelled;
    }

    /// <summary>توافق مع المسار القديم Confirm → Complete.</summary>
    public void Confirm() => Approve();

    public void MarkCompleted() => MarkPosted();

    private void EnsureDraft()
    {
        if (Status != GoodsIssueStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
    }
}

public sealed class GoodsIssueLine : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid GoodsIssueId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid UnitId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost => Quantity * UnitCost;
    public Guid? CostCenterId { get; private set; }
    public string? Notes { get; private set; }

    private GoodsIssueLine() { }

    internal GoodsIssueLine(
        Guid tenantId,
        Guid goodsIssueId,
        Guid inventoryItemId,
        Guid unitId,
        decimal quantity,
        decimal unitCost,
        Guid warehouseId,
        Guid? costCenterId,
        string? notes)
    {
        TenantId = tenantId;
        GoodsIssueId = goodsIssueId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        Quantity = quantity;
        UnitCost = unitCost;
        WarehouseId = warehouseId;
        CostCenterId = costCenterId;
        Notes = notes?.Trim();
    }
}
