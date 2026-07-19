using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Sales.BackOffice;

/// <summary>
/// إذن تسليم بضاعة إداري (Back Office) — يمثل التسليم الفعلي للعميل مقابل أمر بيع.
/// الدورة: مسودة → اعتماد → ترحيل (خصم مخزون) → عكس / إلغاء.
/// </summary>
public sealed class BackOfficeSalesDeliveryNote : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string DeliveryNumber { get; private set; }
    public BackOfficeSalesDocumentStatus Status { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid OrderId { get; private set; }
    public DateOnly DeliveryDate { get; private set; }
    public string? Notes { get; private set; }
    public Guid? JournalEntryId { get; private set; }
    public Guid? ReversalJournalEntryId { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public Guid? PostedBy { get; private set; }

    public decimal TotalCost => _lines.Sum(l => l.LineCost);

    private readonly List<BackOfficeSalesDeliveryNoteLine> _lines = [];
    public IReadOnlyCollection<BackOfficeSalesDeliveryNoteLine> Lines => _lines.AsReadOnly();

    private BackOfficeSalesDeliveryNote()
    {
        DeliveryNumber = string.Empty;
        Status = BackOfficeSalesDocumentStatus.Draft;
    }

    public static BackOfficeSalesDeliveryNote CreateDraft(
        Guid tenantId,
        string deliveryNumber,
        Guid customerId,
        Guid warehouseId,
        Guid orderId,
        DateOnly deliveryDate,
        Guid? companyId = null,
        Guid? branchId = null,
        string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(deliveryNumber)) throw new BusinessException(ErrorCodes.RequiredField);
        if (customerId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);
        if (warehouseId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField, "Warehouse is required.");
        if (orderId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField, "Sales order is required.");

        return new BackOfficeSalesDeliveryNote
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            DeliveryNumber = deliveryNumber.Trim(),
            Status = BackOfficeSalesDocumentStatus.Draft,
            CustomerId = customerId,
            WarehouseId = warehouseId,
            OrderId = orderId,
            DeliveryDate = deliveryDate,
            Notes = notes
        };
    }

    public void AddLine(
        Guid orderLineId,
        string description,
        decimal quantity,
        Guid? inventoryItemId = null,
        Guid? unitId = null,
        decimal unitCost = 0)
    {
        EnsureDraft();
        if (orderLineId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField, "Order line is required.");
        if (quantity <= 0) throw new BusinessException(ErrorCodes.InvalidQuantity);

        _lines.Add(new BackOfficeSalesDeliveryNoteLine(
            Id, orderLineId, inventoryItemId, unitId, description, quantity, unitCost));
    }

    public void ClearLines()
    {
        EnsureDraft();
        _lines.Clear();
    }

    public void UpdateHeader(
        DateOnly deliveryDate,
        Guid? warehouseId = null,
        Guid? branchId = null,
        string? notes = null)
    {
        EnsureDraft();
        DeliveryDate = deliveryDate;
        if (warehouseId.HasValue)
        {
            if (warehouseId.Value == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField, "Warehouse is required.");
            WarehouseId = warehouseId.Value;
        }
        if (branchId.HasValue) BranchId = branchId;
        if (notes is not null) Notes = notes;
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != BackOfficeSalesDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField, "Delivery note must have lines.");

        Status = BackOfficeSalesDocumentStatus.Approved;
        ApprovedAt = DateTimeOffset.UtcNow;
        ApprovedBy = approvedBy;
    }

    public void Unapprove()
    {
        if (Status != BackOfficeSalesDocumentStatus.Approved)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = BackOfficeSalesDocumentStatus.Draft;
        ApprovedAt = null;
        ApprovedBy = null;
    }

    public void MarkPosted(Guid? journalEntryId, Guid postedBy)
    {
        if (Status != BackOfficeSalesDocumentStatus.Approved)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition, "Delivery note must be approved before posting.");
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField);

        Status = BackOfficeSalesDocumentStatus.Posted;
        JournalEntryId = journalEntryId == Guid.Empty ? null : journalEntryId;
        PostedAt = DateTimeOffset.UtcNow;
        PostedBy = postedBy;
    }

    public void MarkReversed(Guid? reversalJournalId = null)
    {
        if (Status != BackOfficeSalesDocumentStatus.Posted)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = BackOfficeSalesDocumentStatus.Reversed;
        ReversalJournalEntryId = reversalJournalId;
    }

    public void Cancel()
    {
        if (Status is BackOfficeSalesDocumentStatus.Posted or BackOfficeSalesDocumentStatus.Reversed)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = BackOfficeSalesDocumentStatus.Cancelled;
    }

    private void EnsureDraft()
    {
        if (Status != BackOfficeSalesDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
    }
}

public sealed class BackOfficeSalesDeliveryNoteLine : AuditableBaseEntity
{
    public Guid DeliveryNoteId { get; private set; }
    public Guid OrderLineId { get; private set; }
    public Guid? InventoryItemId { get; private set; }
    public Guid? UnitId { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }

    public decimal LineCost => Quantity * UnitCost;

    private BackOfficeSalesDeliveryNoteLine() => Description = string.Empty;

    internal BackOfficeSalesDeliveryNoteLine(
        Guid deliveryNoteId,
        Guid orderLineId,
        Guid? inventoryItemId,
        Guid? unitId,
        string description,
        decimal quantity,
        decimal unitCost)
    {
        DeliveryNoteId = deliveryNoteId;
        OrderLineId = orderLineId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        Description = string.IsNullOrWhiteSpace(description) ? "—" : description.Trim();
        Quantity = quantity;
        UnitCost = Math.Max(0, unitCost);
    }
}
