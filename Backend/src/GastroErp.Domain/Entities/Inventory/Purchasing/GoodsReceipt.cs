using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Inventory;

namespace GastroErp.Domain.Entities.Inventory.Purchasing;

/// <summary>
/// سند الفحص والاستلام (GRN) — استلام فعلي من المورد بعد أمر الشراء (أو استلام مباشر استثنائي).
/// الترحيل: مخزون (الكميات المقبولة فقط) + قيد مدين مخزون / دائن GRNI.
/// </summary>
public sealed class GoodsReceipt : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid? BranchId { get; private set; }
    public Guid? PurchaseOrderId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string ReceiptNumber { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? SupplierInvoiceNumber { get; private set; }
    public DateTimeOffset ReceiptDate { get; private set; }
    public GoodsReceiptStatus Status { get; private set; }
    public GoodsReceiptSource Source { get; private set; }
    public string Currency { get; private set; }
    public decimal ExchangeRate { get; private set; }
    public string? ReceiptMethod { get; private set; }
    public string? ReceivedByName { get; private set; }
    public string? SupplierRepName { get; private set; }
    public string? VehicleNumber { get; private set; }
    public string? WaybillNumber { get; private set; }
    public string? Notes { get; private set; }

    // Inspection
    public InspectionResult InspectionResult { get; private set; }
    public string? InspectedBy { get; private set; }
    public DateTimeOffset? InspectionDate { get; private set; }
    public string? QualityNotes { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? QualityCertificateRef { get; private set; }
    public string? ExpiryCertificateRef { get; private set; }

    public Guid? JournalEntryId { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public Guid? PostedBy { get; private set; }
    public Guid? ReversalJournalEntryId { get; private set; }

    private readonly List<GoodsReceiptLine> _lines = [];
    public IReadOnlyCollection<GoodsReceiptLine> Lines => _lines.AsReadOnly();

    private GoodsReceipt()
    {
        ReceiptNumber = string.Empty;
        Currency = "SAR";
        ExchangeRate = 1m;
    }

    public static GoodsReceipt CreateFromPurchaseOrder(
        Guid tenantId,
        Guid purchaseOrderId,
        Guid supplierId,
        Guid warehouseId,
        string receiptNumber,
        string currency = "SAR",
        Guid? branchId = null,
        DateTimeOffset? receiptDate = null,
        string? notes = null)
    {
        ValidateCore(tenantId, supplierId, warehouseId, receiptNumber);
        return new GoodsReceipt
        {
            TenantId = tenantId,
            BranchId = branchId,
            PurchaseOrderId = purchaseOrderId,
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            ReceiptNumber = receiptNumber.Trim(),
            ReceiptDate = receiptDate ?? DateTimeOffset.UtcNow,
            Status = GoodsReceiptStatus.Draft,
            Source = GoodsReceiptSource.FromPurchaseOrder,
            Currency = currency.ToUpperInvariant(),
            ExchangeRate = 1m,
            Notes = notes,
            InspectionResult = InspectionResult.Accepted
        };
    }

    public static GoodsReceipt CreateDirect(
        Guid tenantId,
        Guid supplierId,
        Guid warehouseId,
        string receiptNumber,
        string currency = "SAR",
        Guid? branchId = null,
        DateTimeOffset? receiptDate = null,
        string? notes = null)
    {
        ValidateCore(tenantId, supplierId, warehouseId, receiptNumber);
        return new GoodsReceipt
        {
            TenantId = tenantId,
            BranchId = branchId,
            PurchaseOrderId = null,
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            ReceiptNumber = receiptNumber.Trim(),
            ReceiptDate = receiptDate ?? DateTimeOffset.UtcNow,
            Status = GoodsReceiptStatus.Draft,
            Source = GoodsReceiptSource.Direct,
            Currency = currency.ToUpperInvariant(),
            ExchangeRate = 1m,
            Notes = notes,
            InspectionResult = InspectionResult.Accepted
        };
    }

    /// <summary>Legacy constructor kept for older callers.</summary>
    public GoodsReceipt(Guid tenantId, Guid supplierId, Guid warehouseId, string receiptNumber,
                        Guid? purchaseOrderId = null, string? supplierInvoiceNumber = null, string? notes = null)
    {
        ValidateCore(tenantId, supplierId, warehouseId, receiptNumber);
        TenantId = tenantId;
        SupplierId = supplierId;
        WarehouseId = warehouseId;
        ReceiptNumber = receiptNumber.Trim();
        PurchaseOrderId = purchaseOrderId;
        SupplierInvoiceNumber = supplierInvoiceNumber;
        ReceiptDate = DateTimeOffset.UtcNow;
        Notes = notes;
        Status = GoodsReceiptStatus.Draft;
        Source = purchaseOrderId.HasValue ? GoodsReceiptSource.FromPurchaseOrder : GoodsReceiptSource.Direct;
        Currency = "SAR";
        ExchangeRate = 1m;
        InspectionResult = InspectionResult.Accepted;
    }

    public void UpdateHeader(
        DateTimeOffset receiptDate,
        Guid warehouseId,
        string? referenceNumber,
        string? notes,
        string? receiptMethod,
        string? receivedByName,
        string? supplierRepName,
        string? vehicleNumber,
        string? waybillNumber,
        string currency,
        decimal exchangeRate,
        Guid? branchId = null)
    {
        EnsureEditable();
        if (warehouseId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);
        if (exchangeRate <= 0) throw new BusinessException(ErrorCodes.InvalidAmount);

        ReceiptDate = receiptDate;
        WarehouseId = warehouseId;
        ReferenceNumber = TrimOrNull(referenceNumber);
        Notes = notes;
        ReceiptMethod = TrimOrNull(receiptMethod);
        ReceivedByName = TrimOrNull(receivedByName);
        SupplierRepName = TrimOrNull(supplierRepName);
        VehicleNumber = TrimOrNull(vehicleNumber);
        WaybillNumber = TrimOrNull(waybillNumber);
        Currency = string.IsNullOrWhiteSpace(currency) ? "SAR" : currency.Trim().ToUpperInvariant();
        ExchangeRate = exchangeRate;
        BranchId = branchId;
    }

    public void SetInspection(
        InspectionResult result,
        string? inspectedBy,
        DateTimeOffset? inspectionDate,
        string? qualityNotes,
        string? rejectionReason,
        string? qualityCertificateRef,
        string? expiryCertificateRef)
    {
        EnsureEditable();
        if (result == InspectionResult.Rejected && string.IsNullOrWhiteSpace(rejectionReason))
            throw new BusinessException(ErrorCodes.RequiredField, "Rejection reason is required when inspection is rejected.");

        InspectionResult = result;
        InspectedBy = TrimOrNull(inspectedBy);
        InspectionDate = inspectionDate;
        QualityNotes = qualityNotes;
        RejectionReason = TrimOrNull(rejectionReason);
        QualityCertificateRef = TrimOrNull(qualityCertificateRef);
        ExpiryCertificateRef = TrimOrNull(expiryCertificateRef);

        if (result == InspectionResult.Rejected)
        {
            foreach (var line in _lines)
                line.SetAcceptedRejected(0, line.ReceivedQuantity);
        }
    }

    public void ClearLines()
    {
        EnsureEditable();
        _lines.Clear();
    }

    public void AddLine(
        Guid inventoryItemId,
        Guid unitId,
        decimal receivedQuantity,
        decimal unitCost,
        Guid? purchaseOrderLineId = null,
        decimal orderedQuantity = 0,
        decimal previouslyReceivedQuantity = 0,
        decimal acceptedQuantity = -1,
        decimal rejectedQuantity = 0,
        decimal discountAmount = 0,
        decimal taxPercent = 0,
        decimal taxAmount = 0,
        string? batchNumber = null,
        DateTimeOffset? productionDate = null,
        DateTimeOffset? expiryDate = null,
        string? storageLocation = null,
        string? description = null)
    {
        EnsureEditable();
        var accepted = acceptedQuantity < 0 ? receivedQuantity : acceptedQuantity;
        _lines.Add(new GoodsReceiptLine(
            TenantId, Id, inventoryItemId, unitId, receivedQuantity, unitCost,
            batchNumber, productionDate, expiryDate, purchaseOrderLineId,
            orderedQuantity, previouslyReceivedQuantity, accepted, rejectedQuantity,
            discountAmount, taxPercent, taxAmount, storageLocation, description));
    }

    public void Approve()
    {
        if (Status != GoodsReceiptStatus.Draft)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (!_lines.Any())
            throw new BusinessException(ErrorCodes.RequiredField, "Cannot approve a receipt with no lines.");
        Status = GoodsReceiptStatus.Approved;
    }

    public void MarkPosted(Guid journalEntryId, Guid postedBy)
    {
        if (Status is not (GoodsReceiptStatus.Draft or GoodsReceiptStatus.Approved))
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (!_lines.Any())
            throw new BusinessException(ErrorCodes.RequiredField);
        if (InspectionResult == InspectionResult.Rejected)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition, "Rejected inspection cannot be posted to inventory.");
        if (StockableQuantity <= 0)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "No accepted quantity to post.");

        Status = GoodsReceiptStatus.Posted;
        JournalEntryId = journalEntryId;
        PostedAt = DateTimeOffset.UtcNow;
        PostedBy = postedBy;

        if (PurchaseOrderId.HasValue)
            RaiseDomainEvent(new GoodsReceivedEvent(Id, PurchaseOrderId.Value, WarehouseId, TenantId));
    }

    public void Complete() => MarkPosted(Guid.Empty, Guid.Empty);

    public void MarkReversed(Guid? reversalJournalId = null)
    {
        if (Status != GoodsReceiptStatus.Posted)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = GoodsReceiptStatus.Reversed;
        ReversalJournalEntryId = reversalJournalId;
    }

    public void Cancel()
    {
        if (Status is GoodsReceiptStatus.Posted or GoodsReceiptStatus.Reversed)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = GoodsReceiptStatus.Cancelled;
    }

    /// <summary>Quantity that enters inventory (accepted only).</summary>
    public decimal StockableQuantity => _lines.Sum(l => l.AcceptedQuantity);

    public decimal TotalValue => _lines.Sum(l => l.LineSubTotal);
    public decimal TotalTax => _lines.Sum(l => l.TaxAmount);
    public decimal GrandTotal => TotalValue + TotalTax;
    public bool IsInvoiced => _lines.Any() && _lines.All(l => l.RemainingToInvoice <= 0);
    public bool IsPartiallyInvoiced => _lines.Any(l => l.InvoicedQuantity > 0) && !IsInvoiced;

    private void EnsureEditable()
    {
        if (Status != GoodsReceiptStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
    }

    private static void ValidateCore(Guid tenantId, Guid supplierId, Guid warehouseId, string receiptNumber)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
        if (supplierId == Guid.Empty) throw new ArgumentException(nameof(supplierId));
        if (warehouseId == Guid.Empty) throw new ArgumentException(nameof(warehouseId));
        if (string.IsNullOrWhiteSpace(receiptNumber)) throw new BusinessException(ErrorCodes.RequiredField);
    }

    private static string? TrimOrNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class GoodsReceiptLine : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid GoodsReceiptId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid? PurchaseOrderLineId { get; private set; }

    public decimal OrderedQuantity { get; private set; }
    public decimal PreviouslyReceivedQuantity { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public decimal AcceptedQuantity { get; private set; }
    public decimal RejectedQuantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal InvoicedQuantity { get; private set; }
    public decimal ReturnedQuantity { get; private set; }

    public string? BatchNumber { get; private set; }
    public DateTimeOffset? ProductionDate { get; private set; }
    public DateTimeOffset? ExpiryDate { get; private set; }
    public string? StorageLocation { get; private set; }
    public string? Description { get; private set; }

    public decimal RemainingAtCreation => Math.Max(0, OrderedQuantity - PreviouslyReceivedQuantity);
    public decimal RemainingToInvoice => Math.Max(0, AcceptedQuantity - InvoicedQuantity);
    public decimal RemainingToReturn => Math.Max(0, AcceptedQuantity - ReturnedQuantity);
    public decimal LineSubTotal => Math.Max(0, (AcceptedQuantity * UnitCost) - DiscountAmount);

    private GoodsReceiptLine() { }

    internal GoodsReceiptLine(
        Guid tenantId,
        Guid goodsReceiptId,
        Guid inventoryItemId,
        Guid unitId,
        decimal receivedQuantity,
        decimal unitCost,
        string? batchNumber,
        DateTimeOffset? productionDate,
        DateTimeOffset? expiryDate,
        Guid? purchaseOrderLineId,
        decimal orderedQuantity,
        decimal previouslyReceivedQuantity,
        decimal acceptedQuantity,
        decimal rejectedQuantity,
        decimal discountAmount,
        decimal taxPercent,
        decimal taxAmount,
        string? storageLocation,
        string? description)
    {
        if (receivedQuantity < 0)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "Received quantity cannot be negative.");
        if (receivedQuantity == 0)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "Received quantity must be greater than zero.");
        if (unitCost < 0)
            throw new BusinessException(ErrorCodes.InvalidAmount);

        var remaining = Math.Max(0, orderedQuantity - previouslyReceivedQuantity);
        if (orderedQuantity > 0 && receivedQuantity > remaining)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "Cannot receive more than remaining PO quantity.");

        if (acceptedQuantity < 0 || rejectedQuantity < 0)
            throw new BusinessException(ErrorCodes.InvalidQuantity);
        if (acceptedQuantity + rejectedQuantity > receivedQuantity + 0.0001m)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "Accepted + rejected cannot exceed received quantity.");

        TenantId = tenantId;
        GoodsReceiptId = goodsReceiptId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        PurchaseOrderLineId = purchaseOrderLineId;
        OrderedQuantity = orderedQuantity;
        PreviouslyReceivedQuantity = previouslyReceivedQuantity;
        ReceivedQuantity = receivedQuantity;
        AcceptedQuantity = acceptedQuantity;
        RejectedQuantity = rejectedQuantity;
        UnitCost = unitCost;
        DiscountAmount = Math.Max(0, discountAmount);
        TaxPercent = Math.Max(0, taxPercent);
        TaxAmount = Math.Max(0, taxAmount);
        BatchNumber = string.IsNullOrWhiteSpace(batchNumber) ? null : batchNumber.Trim();
        ProductionDate = productionDate;
        ExpiryDate = expiryDate;
        StorageLocation = string.IsNullOrWhiteSpace(storageLocation) ? null : storageLocation.Trim();
        Description = description;
        InvoicedQuantity = 0;
        ReturnedQuantity = 0;
    }

    internal void SetAcceptedRejected(decimal accepted, decimal rejected)
    {
        if (accepted < 0 || rejected < 0)
            throw new BusinessException(ErrorCodes.InvalidQuantity);
        if (accepted + rejected > ReceivedQuantity + 0.0001m)
            throw new BusinessException(ErrorCodes.InvalidQuantity);
        AcceptedQuantity = accepted;
        RejectedQuantity = rejected;
    }

    public void AddInvoicedQuantity(decimal qty)
    {
        if (qty <= 0) throw new ArgumentException("Quantity must be positive.", nameof(qty));
        if (InvoicedQuantity + qty > AcceptedQuantity)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "Cannot invoice more than accepted quantity.");
        InvoicedQuantity += qty;
    }

    public void ReduceInvoicedQuantity(decimal qty)
    {
        if (qty <= 0) throw new ArgumentException("Quantity must be positive.", nameof(qty));
        InvoicedQuantity = Math.Max(0, InvoicedQuantity - qty);
    }

    public void AddReturnedQuantity(decimal qty)
    {
        if (qty == 0) return;
        if (ReturnedQuantity + qty < -0.0001m)
            throw new BusinessException(ErrorCodes.InvalidQuantity);
        if (ReturnedQuantity + qty > AcceptedQuantity + 0.0001m)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "Cannot return more than accepted quantity.");
        ReturnedQuantity = Math.Max(0, ReturnedQuantity + qty);
    }
}
