using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Sales.BackOffice;

/// <summary>أمر بيع إداري — مستقل عن POS SalesOrder.</summary>
public sealed class BackOfficeSalesOrder : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string OrderNumber { get; private set; }
    public BackOfficeSalesDocumentStatus Status { get; private set; }
    public BackOfficeSalesFulfillmentStatus FulfillmentStatus { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? WarehouseId { get; private set; }
    public Guid? SalesPersonId { get; private set; }
    public Guid? QuotationId { get; private set; }
    public DateOnly OrderDate { get; private set; }
    public DateOnly? ExpectedDeliveryDate { get; private set; }
    public string Currency { get; private set; }
    public decimal ExchangeRate { get; private set; }
    public string? Notes { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }

    private readonly List<BackOfficeSalesOrderLine> _lines = [];
    public IReadOnlyCollection<BackOfficeSalesOrderLine> Lines => _lines.AsReadOnly();

    private BackOfficeSalesOrder()
    {
        OrderNumber = string.Empty;
        Currency = "SAR";
        ExchangeRate = 1m;
        Status = BackOfficeSalesDocumentStatus.Draft;
        FulfillmentStatus = BackOfficeSalesFulfillmentStatus.Open;
    }

    public static BackOfficeSalesOrder CreateDraft(
        Guid tenantId,
        string orderNumber,
        Guid customerId,
        DateOnly orderDate,
        string currency = "SAR",
        Guid? companyId = null,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? salesPersonId = null,
        Guid? quotationId = null,
        DateOnly? expectedDeliveryDate = null,
        decimal exchangeRate = 1m,
        string? notes = null,
        decimal discountAmount = 0)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(orderNumber)) throw new BusinessException(ErrorCodes.RequiredField);
        if (customerId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);
        if (exchangeRate <= 0) throw new BusinessException(ErrorCodes.InvalidAmount);

        return new BackOfficeSalesOrder
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            OrderNumber = orderNumber.Trim(),
            CustomerId = customerId,
            WarehouseId = warehouseId,
            SalesPersonId = salesPersonId,
            QuotationId = quotationId,
            OrderDate = orderDate,
            ExpectedDeliveryDate = expectedDeliveryDate,
            Currency = currency.ToUpperInvariant(),
            ExchangeRate = exchangeRate,
            Notes = notes,
            DiscountAmount = Math.Max(0, discountAmount)
        };
    }

    public void AddLine(
        string description,
        decimal quantity,
        decimal unitPrice,
        Guid? inventoryItemId = null,
        Guid? unitId = null,
        BackOfficeSalesLineNature lineNature = BackOfficeSalesLineNature.Inventory,
        decimal taxPercent = 0,
        decimal discountAmount = 0,
        decimal unitCost = 0)
    {
        EnsureDraft();
        if (quantity <= 0) throw new BusinessException(ErrorCodes.InvalidQuantity);
        if (unitPrice < 0) throw new BusinessException(ErrorCodes.InvalidAmount);
        _lines.Add(new BackOfficeSalesOrderLine(
            Id, description, quantity, unitPrice, inventoryItemId, unitId, lineNature,
            taxPercent, discountAmount, unitCost));
        RecalculateTotals();
    }

    public void ClearLines()
    {
        EnsureDraft();
        _lines.Clear();
        RecalculateTotals();
    }

    public void UpdateHeader(
        DateOnly orderDate,
        Guid? warehouseId = null,
        Guid? salesPersonId = null,
        Guid? branchId = null,
        DateOnly? expectedDeliveryDate = null,
        string? notes = null,
        decimal? discountAmount = null)
    {
        EnsureDraft();
        OrderDate = orderDate;
        if (warehouseId.HasValue) WarehouseId = warehouseId;
        if (salesPersonId.HasValue) SalesPersonId = salesPersonId;
        if (branchId.HasValue) BranchId = branchId;
        ExpectedDeliveryDate = expectedDeliveryDate;
        if (notes is not null) Notes = notes;
        if (discountAmount.HasValue) DiscountAmount = Math.Max(0, discountAmount.Value);
        RecalculateTotals();
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != BackOfficeSalesDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField, "Order must have lines.");
        Status = BackOfficeSalesDocumentStatus.Approved;
        ApprovedAt = DateTimeOffset.UtcNow;
        ApprovedBy = approvedBy;
    }

    public void Unapprove()
    {
        if (Status != BackOfficeSalesDocumentStatus.Approved)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        if (FulfillmentStatus != BackOfficeSalesFulfillmentStatus.Open)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition, "Cannot unapprove after delivery/invoicing.");
        Status = BackOfficeSalesDocumentStatus.Draft;
        ApprovedAt = null;
        ApprovedBy = null;
    }

    public void Cancel()
    {
        if (Status is BackOfficeSalesDocumentStatus.Posted or BackOfficeSalesDocumentStatus.Reversed)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        if (_lines.Any(l => l.DeliveredQuantity > 0 || l.InvoicedQuantity > 0))
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition, "Cannot cancel after delivery/invoicing.");
        Status = BackOfficeSalesDocumentStatus.Cancelled;
    }

    public void Close()
    {
        if (Status != BackOfficeSalesDocumentStatus.Approved)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = BackOfficeSalesDocumentStatus.Posted;
        FulfillmentStatus = BackOfficeSalesFulfillmentStatus.Closed;
    }

    public void RegisterDelivery(Guid orderLineId, decimal qty)
    {
        EnsureApprovedOrPosted();
        var line = _lines.FirstOrDefault(l => l.Id == orderLineId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        line.AddDeliveredQuantity(qty);
        RefreshFulfillment();
    }

    public void RegisterInvoice(Guid orderLineId, decimal qty)
    {
        EnsureApprovedOrPosted();
        var line = _lines.FirstOrDefault(l => l.Id == orderLineId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        line.AddInvoicedQuantity(qty);
        RefreshFulfillment();
    }

    /// <summary>يعكس تسليم بضاعة سابق (يُستخدم عند إلغاء إذن تسليم مُرَحَّل).</summary>
    public void RegisterDeliveryReversal(Guid orderLineId, decimal qty)
    {
        EnsureApprovedOrPosted();
        var line = _lines.FirstOrDefault(l => l.Id == orderLineId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        line.SubtractDeliveredQuantity(qty);
        RefreshFulfillment();
    }

    /// <summary>يعكس فوترة سابقة (يُستخدم عند عكس فاتورة مرتبطة بأمر).</summary>
    public void RegisterInvoiceReversal(Guid orderLineId, decimal qty)
    {
        EnsureApprovedOrPosted();
        var line = _lines.FirstOrDefault(l => l.Id == orderLineId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        line.SubtractInvoicedQuantity(qty);
        RefreshFulfillment();
    }

    private void RefreshFulfillment()
    {
        var anyDelivered = _lines.Any(l => l.DeliveredQuantity > 0);
        var allDelivered = _lines.All(l => l.RemainingToDeliver <= 0.0001m);
        var anyInvoiced = _lines.Any(l => l.InvoicedQuantity > 0);
        var allInvoiced = _lines.All(l => l.RemainingToInvoice <= 0.0001m);

        if (allInvoiced)
            FulfillmentStatus = BackOfficeSalesFulfillmentStatus.FullyInvoiced;
        else if (anyInvoiced)
            FulfillmentStatus = BackOfficeSalesFulfillmentStatus.PartiallyInvoiced;
        else if (allDelivered)
            FulfillmentStatus = BackOfficeSalesFulfillmentStatus.FullyDelivered;
        else if (anyDelivered)
            FulfillmentStatus = BackOfficeSalesFulfillmentStatus.PartiallyDelivered;
        else
            FulfillmentStatus = BackOfficeSalesFulfillmentStatus.Open;
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

    private void EnsureApprovedOrPosted()
    {
        if (Status is not (BackOfficeSalesDocumentStatus.Approved or BackOfficeSalesDocumentStatus.Posted))
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
    }
}

public sealed class BackOfficeSalesOrderLine : AuditableBaseEntity
{
    public Guid BackOfficeSalesOrderId { get; private set; }
    public Guid? InventoryItemId { get; private set; }
    public Guid? UnitId { get; private set; }
    public BackOfficeSalesLineNature LineNature { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DeliveredQuantity { get; private set; }
    public decimal InvoicedQuantity { get; private set; }

    public decimal LineGross => Quantity * UnitPrice;
    public decimal LineNet => Math.Max(0, LineGross - DiscountAmount);
    public decimal RemainingToDeliver => Math.Max(0, Quantity - DeliveredQuantity);
    public decimal RemainingToInvoice => Math.Max(0, Quantity - InvoicedQuantity);

    private BackOfficeSalesOrderLine() => Description = string.Empty;

    internal BackOfficeSalesOrderLine(
        Guid orderId, string description, decimal quantity, decimal unitPrice,
        Guid? inventoryItemId, Guid? unitId, BackOfficeSalesLineNature lineNature,
        decimal taxPercent, decimal discountAmount, decimal unitCost)
    {
        BackOfficeSalesOrderId = orderId;
        Description = string.IsNullOrWhiteSpace(description) ? "—" : description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        LineNature = lineNature;
        UnitCost = Math.Max(0, unitCost);
        DiscountAmount = Math.Max(0, discountAmount);
        TaxPercent = Math.Max(0, taxPercent);
        TaxAmount = Math.Round(Math.Max(0, Quantity * UnitPrice - DiscountAmount) * TaxPercent / 100m, 4);
    }

    internal void AddDeliveredQuantity(decimal qty)
    {
        if (qty <= 0) throw new BusinessException(ErrorCodes.InvalidQuantity);
        if (DeliveredQuantity + qty > Quantity + 0.0001m)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "Cannot deliver more than ordered quantity.");
        DeliveredQuantity += qty;
    }

    internal void AddInvoicedQuantity(decimal qty)
    {
        if (qty <= 0) throw new BusinessException(ErrorCodes.InvalidQuantity);
        if (InvoicedQuantity + qty > Quantity + 0.0001m)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "Cannot invoice more than ordered quantity.");
        InvoicedQuantity += qty;
    }

    internal void SubtractDeliveredQuantity(decimal qty)
    {
        if (qty <= 0) throw new BusinessException(ErrorCodes.InvalidQuantity);
        DeliveredQuantity = Math.Max(0, DeliveredQuantity - qty);
    }

    internal void SubtractInvoicedQuantity(decimal qty)
    {
        if (qty <= 0) throw new BusinessException(ErrorCodes.InvalidQuantity);
        InvoicedQuantity = Math.Max(0, InvoicedQuantity - qty);
    }
}
