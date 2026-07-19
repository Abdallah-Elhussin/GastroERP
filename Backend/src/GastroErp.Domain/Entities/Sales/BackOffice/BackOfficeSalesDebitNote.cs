using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Sales.BackOffice;

/// <summary>
/// إشعار مدين مبيعات إداري (Back Office) — يزيد مديونية العميل عن فاتورة أصلية أو بشكل مستقل.
/// الدورة: مسودة → اعتماد → ترحيل (قيد محاسبي) → عكس / إلغاء.
/// </summary>
public sealed class BackOfficeSalesDebitNote : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string DebitNoteNumber { get; private set; }
    public BackOfficeSalesDocumentStatus Status { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public DateOnly DebitDate { get; private set; }
    public string Currency { get; private set; }
    public string? Notes { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public Guid? JournalEntryId { get; private set; }
    public Guid? ReversalJournalEntryId { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public Guid? PostedBy { get; private set; }

    private readonly List<BackOfficeSalesDebitNoteLine> _lines = [];
    public IReadOnlyCollection<BackOfficeSalesDebitNoteLine> Lines => _lines.AsReadOnly();

    private BackOfficeSalesDebitNote()
    {
        DebitNoteNumber = string.Empty;
        Currency = "SAR";
        Status = BackOfficeSalesDocumentStatus.Draft;
    }

    public static BackOfficeSalesDebitNote CreateDraft(
        Guid tenantId,
        string debitNoteNumber,
        Guid customerId,
        DateOnly debitDate,
        string currency = "SAR",
        Guid? companyId = null,
        Guid? branchId = null,
        Guid? invoiceId = null,
        string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(debitNoteNumber)) throw new BusinessException(ErrorCodes.RequiredField);
        if (customerId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);

        return new BackOfficeSalesDebitNote
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            DebitNoteNumber = debitNoteNumber.Trim(),
            Status = BackOfficeSalesDocumentStatus.Draft,
            CustomerId = customerId,
            InvoiceId = invoiceId,
            DebitDate = debitDate,
            Currency = currency.ToUpperInvariant(),
            Notes = notes
        };
    }

    public void AddLine(
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxPercent = 0,
        decimal taxAmount = 0)
    {
        EnsureDraft();
        if (quantity <= 0) throw new BusinessException(ErrorCodes.InvalidQuantity);
        if (unitPrice < 0) throw new BusinessException(ErrorCodes.InvalidAmount);

        _lines.Add(new BackOfficeSalesDebitNoteLine(
            Id, description, quantity, unitPrice, taxPercent, taxAmount));
        RecalculateTotals();
    }

    public void ClearLines()
    {
        EnsureDraft();
        _lines.Clear();
        RecalculateTotals();
    }

    public void UpdateHeader(
        DateOnly debitDate,
        Guid? invoiceId = null,
        Guid? branchId = null,
        string? notes = null)
    {
        EnsureDraft();
        DebitDate = debitDate;
        if (invoiceId.HasValue) InvoiceId = invoiceId;
        if (branchId.HasValue) BranchId = branchId;
        if (notes is not null) Notes = notes;

        RecalculateTotals();
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != BackOfficeSalesDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField, "Debit note must have lines.");

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
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition, "Debit note must be approved before posting.");
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
        TotalAmount = Math.Max(0, SubTotal + TaxAmount);
    }

    private void EnsureDraft()
    {
        if (Status != BackOfficeSalesDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
    }
}

public sealed class BackOfficeSalesDebitNoteLine : AuditableBaseEntity
{
    public Guid DebitNoteId { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal TaxAmount { get; private set; }

    public decimal LineNet => Quantity * UnitPrice;
    public decimal LineTotal => LineNet + TaxAmount;

    private BackOfficeSalesDebitNoteLine() => Description = string.Empty;

    internal BackOfficeSalesDebitNoteLine(
        Guid debitNoteId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxPercent,
        decimal taxAmount)
    {
        DebitNoteId = debitNoteId;
        Description = string.IsNullOrWhiteSpace(description) ? "—" : description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxPercent = Math.Max(0, taxPercent);
        TaxAmount = taxAmount > 0
            ? Math.Max(0, taxAmount)
            : Math.Round(Quantity * UnitPrice * TaxPercent / 100m, 4);
    }
}
