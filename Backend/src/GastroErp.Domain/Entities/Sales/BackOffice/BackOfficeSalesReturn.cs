using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Sales.BackOffice;

/// <summary>
/// مرتجع مبيعات إداري (Back Office) — يرتبط دومًا بفاتورة مبيعات إدارية.
/// الدورة: مسودة → اعتماد → ترحيل (إضافة مخزون + قيد عكسي) → عكس / إلغاء.
/// </summary>
public sealed class BackOfficeSalesReturn : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string ReturnNumber { get; private set; }
    public BackOfficeSalesDocumentStatus Status { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? WarehouseId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public DateOnly ReturnDate { get; private set; }
    public string? Notes { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public Guid? JournalEntryId { get; private set; }
    public Guid? ReversalJournalEntryId { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public Guid? PostedBy { get; private set; }

    private readonly List<BackOfficeSalesReturnLine> _lines = [];
    public IReadOnlyCollection<BackOfficeSalesReturnLine> Lines => _lines.AsReadOnly();

    private BackOfficeSalesReturn()
    {
        ReturnNumber = string.Empty;
        Status = BackOfficeSalesDocumentStatus.Draft;
    }

    public static BackOfficeSalesReturn CreateDraft(
        Guid tenantId,
        string returnNumber,
        Guid customerId,
        Guid invoiceId,
        DateOnly returnDate,
        Guid? companyId = null,
        Guid? branchId = null,
        Guid? warehouseId = null,
        string? notes = null,
        decimal discountAmount = 0)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(returnNumber)) throw new BusinessException(ErrorCodes.RequiredField);
        if (customerId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);
        if (invoiceId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField, "Invoice is required.");

        return new BackOfficeSalesReturn
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            ReturnNumber = returnNumber.Trim(),
            Status = BackOfficeSalesDocumentStatus.Draft,
            CustomerId = customerId,
            WarehouseId = warehouseId,
            InvoiceId = invoiceId,
            ReturnDate = returnDate,
            Notes = notes,
            DiscountAmount = Math.Max(0, discountAmount)
        };
    }

    public void AddLine(
        Guid invoiceLineId,
        string description,
        decimal quantity,
        decimal unitPrice,
        Guid? inventoryItemId = null,
        Guid? unitId = null,
        BackOfficeSalesLineNature lineNature = BackOfficeSalesLineNature.Inventory,
        decimal taxPercent = 0,
        decimal taxAmount = 0,
        decimal unitCost = 0)
    {
        EnsureDraft();
        if (invoiceLineId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField, "Invoice line is required.");
        if (quantity <= 0) throw new BusinessException(ErrorCodes.InvalidQuantity);
        if (unitPrice < 0) throw new BusinessException(ErrorCodes.InvalidAmount);

        _lines.Add(new BackOfficeSalesReturnLine(
            Id, invoiceLineId, inventoryItemId, unitId, lineNature, description, quantity, unitPrice,
            taxPercent, taxAmount, unitCost));
        RecalculateTotals();
    }

    public void ClearLines()
    {
        EnsureDraft();
        _lines.Clear();
        RecalculateTotals();
    }

    public void UpdateHeader(
        DateOnly returnDate,
        Guid? warehouseId = null,
        Guid? branchId = null,
        string? notes = null,
        decimal? discountAmount = null)
    {
        EnsureDraft();
        ReturnDate = returnDate;
        if (warehouseId.HasValue) WarehouseId = warehouseId;
        if (branchId.HasValue) BranchId = branchId;
        if (notes is not null) Notes = notes;
        if (discountAmount.HasValue) DiscountAmount = Math.Max(0, discountAmount.Value);

        RecalculateTotals();
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != BackOfficeSalesDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField, "Return must have lines.");

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

    public void MarkPosted(Guid journalEntryId, Guid postedBy)
    {
        if (Status != BackOfficeSalesDocumentStatus.Approved)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition, "Return must be approved before posting.");
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField);

        Status = BackOfficeSalesDocumentStatus.Posted;
        JournalEntryId = journalEntryId;
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

    private void RecalculateTotals()
    {
        SubTotal = _lines.Sum(l => l.LineNet);
        TaxAmount = _lines.Sum(l => l.TaxAmount);
        TotalAmount = Math.Max(0, SubTotal - DiscountAmount + TaxAmount);
    }

    private void EnsureDraft()
    {
        if (Status != BackOfficeSalesDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
    }
}

public sealed class BackOfficeSalesReturnLine : AuditableBaseEntity
{
    public Guid ReturnId { get; private set; }
    public Guid InvoiceLineId { get; private set; }
    public Guid? InventoryItemId { get; private set; }
    public Guid? UnitId { get; private set; }
    public BackOfficeSalesLineNature LineNature { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal UnitCost { get; private set; }

    public decimal LineNet => Quantity * UnitPrice;
    public decimal LineTotal => LineNet + TaxAmount;

    private BackOfficeSalesReturnLine() => Description = string.Empty;

    internal BackOfficeSalesReturnLine(
        Guid returnId,
        Guid invoiceLineId,
        Guid? inventoryItemId,
        Guid? unitId,
        BackOfficeSalesLineNature lineNature,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxPercent,
        decimal taxAmount,
        decimal unitCost)
    {
        ReturnId = returnId;
        InvoiceLineId = invoiceLineId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        LineNature = lineNature;
        Description = string.IsNullOrWhiteSpace(description) ? "—" : description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        UnitCost = Math.Max(0, unitCost);
        TaxPercent = Math.Max(0, taxPercent);
        TaxAmount = taxAmount > 0
            ? Math.Max(0, taxAmount)
            : Math.Round(Quantity * UnitPrice * TaxPercent / 100m, 4);
    }
}
