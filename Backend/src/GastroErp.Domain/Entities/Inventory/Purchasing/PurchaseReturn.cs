using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Purchasing;

/// <summary>
/// مرتجع مشتريات — مرتبط بمستند أصلي (استلام / فاتورة / فاتورة مباشرة).
/// لا يُنشأ ببنود يدوية؛ البنود تُحمَّل من المصدر.
/// </summary>
public sealed class PurchaseReturn : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid? BranchId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string ReturnNumber { get; private set; }
    public DateTimeOffset ReturnDate { get; private set; }
    public PurchaseReturnType ReturnType { get; private set; }
    public PurchasingDocumentStatus Status { get; private set; }
    public Guid? GoodsReceiptId { get; private set; }
    public Guid? PurchaseInvoiceId { get; private set; }
    public Guid? ReturnReasonId { get; private set; }
    public string? ReasonNotes { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; }
    public string Currency { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public Guid? JournalEntryId { get; private set; }
    public Guid? CreditNoteJournalEntryId { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public Guid? PostedBy { get; private set; }
    public Guid? ReversalJournalEntryId { get; private set; }

    /// <summary>Legacy flag kept for compatibility with older callers.</summary>
    public bool IsCompleted => Status is PurchasingDocumentStatus.Posted or PurchasingDocumentStatus.Reversed;

    private readonly List<PurchaseReturnLine> _lines = [];
    public IReadOnlyCollection<PurchaseReturnLine> Lines => _lines.AsReadOnly();

    private PurchaseReturn()
    {
        ReturnNumber = string.Empty;
        Currency = "SAR";
    }

    public static PurchaseReturn CreateFromGoodsReceipt(
        Guid tenantId,
        Guid supplierId,
        Guid warehouseId,
        string returnNumber,
        Guid goodsReceiptId,
        string currency = "SAR",
        Guid? branchId = null,
        DateTimeOffset? returnDate = null,
        Guid? returnReasonId = null,
        string? reasonNotes = null,
        string? referenceNumber = null,
        string? notes = null)
    {
        ValidateCore(tenantId, supplierId, warehouseId, returnNumber);
        return new PurchaseReturn
        {
            TenantId = tenantId,
            BranchId = branchId,
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            ReturnNumber = returnNumber.Trim(),
            ReturnDate = returnDate ?? DateTimeOffset.UtcNow,
            ReturnType = PurchaseReturnType.BeforeInvoice,
            Status = PurchasingDocumentStatus.Draft,
            GoodsReceiptId = goodsReceiptId,
            PurchaseInvoiceId = null,
            ReturnReasonId = returnReasonId,
            ReasonNotes = TrimOrNull(reasonNotes),
            ReferenceNumber = TrimOrNull(referenceNumber),
            Notes = notes,
            Currency = currency.ToUpperInvariant()
        };
    }

    public static PurchaseReturn CreateFromInvoice(
        Guid tenantId,
        Guid supplierId,
        Guid warehouseId,
        string returnNumber,
        Guid purchaseInvoiceId,
        PurchaseReturnType returnType,
        string currency = "SAR",
        Guid? branchId = null,
        DateTimeOffset? returnDate = null,
        Guid? returnReasonId = null,
        string? reasonNotes = null,
        string? referenceNumber = null,
        string? notes = null)
    {
        ValidateCore(tenantId, supplierId, warehouseId, returnNumber);
        if (returnType is not (PurchaseReturnType.AfterInvoice or PurchaseReturnType.Direct))
            throw new BusinessException(ErrorCodes.InvalidStatusTransition, "Invoice-based returns must be AfterInvoice or Direct.");

        return new PurchaseReturn
        {
            TenantId = tenantId,
            BranchId = branchId,
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            ReturnNumber = returnNumber.Trim(),
            ReturnDate = returnDate ?? DateTimeOffset.UtcNow,
            ReturnType = returnType,
            Status = PurchasingDocumentStatus.Draft,
            GoodsReceiptId = null,
            PurchaseInvoiceId = purchaseInvoiceId,
            ReturnReasonId = returnReasonId,
            ReasonNotes = TrimOrNull(reasonNotes),
            ReferenceNumber = TrimOrNull(referenceNumber),
            Notes = notes,
            Currency = currency.ToUpperInvariant()
        };
    }

    /// <summary>Legacy constructor — maps to Direct draft without source (prefer factory methods).</summary>
    public PurchaseReturn(Guid tenantId, Guid supplierId, Guid warehouseId, string returnNumber,
                          Guid? goodsReceiptId = null, string? reason = null)
    {
        ValidateCore(tenantId, supplierId, warehouseId, returnNumber);
        TenantId = tenantId;
        SupplierId = supplierId;
        WarehouseId = warehouseId;
        ReturnNumber = returnNumber.Trim();
        ReturnDate = DateTimeOffset.UtcNow;
        GoodsReceiptId = goodsReceiptId;
        ReturnType = goodsReceiptId.HasValue ? PurchaseReturnType.BeforeInvoice : PurchaseReturnType.Direct;
        Status = PurchasingDocumentStatus.Draft;
        ReasonNotes = reason;
        Currency = "SAR";
    }

    public void UpdateHeader(
        DateTimeOffset returnDate,
        Guid? returnReasonId,
        string? reasonNotes,
        string? referenceNumber,
        string? notes)
    {
        EnsureEditable();
        ReturnDate = returnDate;
        ReturnReasonId = returnReasonId;
        ReasonNotes = TrimOrNull(reasonNotes);
        ReferenceNumber = TrimOrNull(referenceNumber);
        Notes = notes;
    }

    public void ClearLines()
    {
        EnsureEditable();
        _lines.Clear();
        RecalculateTotals();
    }

    public void AddLine(
        Guid inventoryItemId,
        Guid unitId,
        decimal originalQuantity,
        decimal previouslyReturnedQuantity,
        decimal returnQuantity,
        decimal unitCost,
        decimal discountAmount = 0,
        decimal taxPercent = 0,
        decimal taxAmount = 0,
        Guid? goodsReceiptLineId = null,
        Guid? purchaseInvoiceLineId = null,
        string? batchNumber = null,
        DateTimeOffset? expiryDate = null,
        string? lineReason = null,
        string? notes = null,
        decimal? productTemperature = null,
        bool destroyItem = false)
    {
        EnsureEditable();
        _lines.Add(new PurchaseReturnLine(
            TenantId, Id, inventoryItemId, unitId,
            originalQuantity, previouslyReturnedQuantity, returnQuantity, unitCost,
            discountAmount, taxPercent, taxAmount,
            goodsReceiptLineId, purchaseInvoiceLineId,
            batchNumber, expiryDate, lineReason, notes, productTemperature, destroyItem));
        RecalculateTotals();
    }

    public void Approve()
    {
        if (Status != PurchasingDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField, "Cannot approve a return with no lines.");
        if (!ReturnReasonId.HasValue && string.IsNullOrWhiteSpace(ReasonNotes))
            throw new BusinessException(ErrorCodes.RequiredField, "Return reason is required.");
        Status = PurchasingDocumentStatus.Approved;
    }

    public void MarkPosted(Guid journalEntryId, Guid postedBy, Guid? creditNoteJournalId = null)
    {
        if (Status is not (PurchasingDocumentStatus.Draft or PurchasingDocumentStatus.Approved))
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField);

        Status = PurchasingDocumentStatus.Posted;
        JournalEntryId = journalEntryId;
        CreditNoteJournalEntryId = creditNoteJournalId;
        PostedAt = DateTimeOffset.UtcNow;
        PostedBy = postedBy;
    }

    public void Complete() => MarkPosted(Guid.Empty, Guid.Empty);

    public void MarkReversed(Guid? reversalJournalId = null)
    {
        if (Status != PurchasingDocumentStatus.Posted)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = PurchasingDocumentStatus.Reversed;
        ReversalJournalEntryId = reversalJournalId;
    }

    public void Cancel()
    {
        if (Status is PurchasingDocumentStatus.Posted or PurchasingDocumentStatus.Reversed)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = PurchasingDocumentStatus.Cancelled;
    }

    private void RecalculateTotals()
    {
        SubTotal = _lines.Sum(l => l.LineSubTotal);
        TaxAmount = _lines.Sum(l => l.TaxAmount);
        TotalAmount = SubTotal + TaxAmount;
    }

    private void EnsureEditable()
    {
        if (Status != PurchasingDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
    }

    private static void ValidateCore(Guid tenantId, Guid supplierId, Guid warehouseId, string returnNumber)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
        if (supplierId == Guid.Empty) throw new ArgumentException(nameof(supplierId));
        if (warehouseId == Guid.Empty) throw new ArgumentException(nameof(warehouseId));
        if (string.IsNullOrWhiteSpace(returnNumber)) throw new BusinessException(ErrorCodes.RequiredField);
    }

    private static string? TrimOrNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class PurchaseReturnLine : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid PurchaseReturnId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid? GoodsReceiptLineId { get; private set; }
    public Guid? PurchaseInvoiceLineId { get; private set; }

    public decimal OriginalQuantity { get; private set; }
    public decimal PreviouslyReturnedQuantity { get; private set; }
    public decimal ReturnQuantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal TaxAmount { get; private set; }

    public string? BatchNumber { get; private set; }
    public DateTimeOffset? ExpiryDate { get; private set; }
    public string? LineReason { get; private set; }
    public string? Notes { get; private set; }
    public decimal? ProductTemperature { get; private set; }
    public bool DestroyItem { get; private set; }

    public decimal AvailableToReturn => Math.Max(0, OriginalQuantity - PreviouslyReturnedQuantity);
    public decimal LineSubTotal => Math.Max(0, (ReturnQuantity * UnitCost) - DiscountAmount);
    public decimal LineTotal => LineSubTotal + TaxAmount;

    private PurchaseReturnLine() { }

    internal PurchaseReturnLine(
        Guid tenantId,
        Guid purchaseReturnId,
        Guid inventoryItemId,
        Guid unitId,
        decimal originalQuantity,
        decimal previouslyReturnedQuantity,
        decimal returnQuantity,
        decimal unitCost,
        decimal discountAmount,
        decimal taxPercent,
        decimal taxAmount,
        Guid? goodsReceiptLineId,
        Guid? purchaseInvoiceLineId,
        string? batchNumber,
        DateTimeOffset? expiryDate,
        string? lineReason,
        string? notes,
        decimal? productTemperature,
        bool destroyItem)
    {
        if (returnQuantity <= 0)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "Return quantity must be greater than zero.");
        if (returnQuantity < 0)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "Return quantity cannot be negative.");
        if (unitCost < 0)
            throw new BusinessException(ErrorCodes.InvalidAmount);

        var available = Math.Max(0, originalQuantity - previouslyReturnedQuantity);
        if (originalQuantity > 0 && returnQuantity > available + 0.0001m)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "Cannot return more than available quantity.");

        TenantId = tenantId;
        PurchaseReturnId = purchaseReturnId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        OriginalQuantity = originalQuantity;
        PreviouslyReturnedQuantity = previouslyReturnedQuantity;
        ReturnQuantity = returnQuantity;
        UnitCost = unitCost;
        DiscountAmount = Math.Max(0, discountAmount);
        TaxPercent = Math.Max(0, taxPercent);
        TaxAmount = Math.Max(0, taxAmount);
        GoodsReceiptLineId = goodsReceiptLineId;
        PurchaseInvoiceLineId = purchaseInvoiceLineId;
        BatchNumber = string.IsNullOrWhiteSpace(batchNumber) ? null : batchNumber.Trim();
        ExpiryDate = expiryDate;
        LineReason = string.IsNullOrWhiteSpace(lineReason) ? null : lineReason.Trim();
        Notes = notes;
        ProductTemperature = productTemperature;
        DestroyItem = destroyItem;
    }
}

/// <summary>جدول ترميز أسباب إرجاع المشتريات.</summary>
public sealed class PurchaseReturnReason : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private PurchaseReturnReason()
    {
        Code = string.Empty;
        NameAr = string.Empty;
    }

    public PurchaseReturnReason(Guid tenantId, string code, string nameAr, string? nameEn = null, int sortOrder = 0)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(code)) throw new BusinessException(ErrorCodes.RequiredField);
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        Code = code.Trim().ToUpperInvariant();
        NameAr = nameAr.Trim();
        NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim();
        SortOrder = sortOrder;
        IsActive = true;
    }

    public void Update(string nameAr, string? nameEn, int sortOrder, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr.Trim();
        NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim();
        SortOrder = sortOrder;
        IsActive = isActive;
    }
}
