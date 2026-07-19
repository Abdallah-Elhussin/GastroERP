using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>
/// إشعار مالي موحد (مدين/دائن). يرحَّل عبر <see cref="PostingSource.DebitNote"/> أو <see cref="PostingSource.CreditNote"/>.
/// </summary>
public sealed class FinancialNote : AuditableBaseEntity, ITenantEntity
{
    private readonly List<FinancialNoteLine> _lines = [];

    public Guid TenantId { get; private set; }
    public int Number { get; private set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public FinancialNoteKind NoteKind { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid BranchId { get; private set; }
    public DateOnly NoteDate { get; private set; }
    public Guid FiscalPeriodId { get; private set; }
    public NotificationPartyType PartyType { get; private set; }
    public Guid? PartyId { get; private set; }
    public string? PartyName { get; private set; }
    public Guid MainAccountId { get; private set; }
    public string Currency { get; private set; } = "SAR";
    public decimal ExchangeRate { get; private set; } = 1m;
    public FinancialNoteReferenceType ReferenceType { get; private set; } = FinancialNoteReferenceType.None;
    public Guid? ReferenceDocumentId { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Description { get; private set; }
    public string? Notes { get; private set; }
    public FinancialNoteStatus Status { get; private set; } = FinancialNoteStatus.Draft;
    public Guid? JournalEntryId { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public Guid? PostedBy { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? CancelledBy { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    public IReadOnlyCollection<FinancialNoteLine> Lines => _lines.AsReadOnly();

    private FinancialNote() { }

    public static FinancialNote Create(
        Guid tenantId,
        int number,
        string documentNumber,
        FinancialNoteKind noteKind,
        Guid companyId,
        Guid branchId,
        DateOnly noteDate,
        Guid fiscalPeriodId,
        NotificationPartyType partyType,
        Guid mainAccountId,
        string currency = "SAR",
        decimal exchangeRate = 1m,
        Guid? partyId = null,
        string? partyName = null,
        FinancialNoteReferenceType referenceType = FinancialNoteReferenceType.None,
        Guid? referenceDocumentId = null,
        string? referenceNumber = null,
        string? description = null,
        string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (companyId == Guid.Empty || branchId == Guid.Empty || fiscalPeriodId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);
        if (number < 1) throw new ArgumentOutOfRangeException(nameof(number));
        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new BusinessException(ErrorCodes.RequiredField, "Document number is required.");

        var note = new FinancialNote
        {
            TenantId = tenantId,
            Number = number,
            DocumentNumber = documentNumber.Trim(),
            Status = FinancialNoteStatus.Draft
        };
        note.ApplyHeader(
            noteKind, noteDate, fiscalPeriodId, partyType, mainAccountId, currency, exchangeRate,
            partyId, partyName, referenceType, referenceDocumentId, referenceNumber, description, notes);
        note.CompanyId = companyId;
        note.BranchId = branchId;
        return note;
    }

    public void Update(
        Guid companyId,
        Guid branchId,
        FinancialNoteKind noteKind,
        DateOnly noteDate,
        Guid fiscalPeriodId,
        NotificationPartyType partyType,
        Guid mainAccountId,
        string currency,
        decimal exchangeRate,
        Guid? partyId,
        string? partyName,
        FinancialNoteReferenceType referenceType,
        Guid? referenceDocumentId,
        string? referenceNumber,
        string? description,
        string? notes)
    {
        EnsureEditable();
        if (companyId == Guid.Empty || branchId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);
        CompanyId = companyId;
        BranchId = branchId;
        ApplyHeader(
            noteKind, noteDate, fiscalPeriodId, partyType, mainAccountId, currency, exchangeRate,
            partyId, partyName, referenceType, referenceDocumentId, referenceNumber, description, notes);
    }

    public FinancialNoteLine AddLine(
        Guid notificationReasonId,
        Guid offsetAccountId,
        decimal amount,
        Guid? costCenterId = null,
        Guid? analyticalAccountId = null,
        string? currency = null,
        decimal? exchangeRate = null,
        string? description = null)
    {
        EnsureEditable();
        if (notificationReasonId == Guid.Empty)
            throw new BusinessException(ErrorCodes.FinancialNoteReasonInvalid, "Notification reason is required.");
        if (offsetAccountId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField, "Offset account is required.");
        if (amount <= 0)
            throw new BusinessException(ErrorCodes.FinancialNoteAmountInvalid, "Line amount must be greater than zero.");

        var lineCurrency = string.IsNullOrWhiteSpace(currency) ? Currency : currency.Trim().ToUpperInvariant();
        var lineRate = exchangeRate is > 0 ? exchangeRate.Value : ExchangeRate;

        var line = FinancialNoteLine.Create(
            Id, _lines.Count + 1, notificationReasonId, offsetAccountId, amount,
            costCenterId, analyticalAccountId, lineCurrency, lineRate, description);
        _lines.Add(line);
        return line;
    }

    public void ReplaceLines(
        IEnumerable<(
            Guid NotificationReasonId,
            Guid OffsetAccountId,
            decimal Amount,
            Guid? CostCenterId,
            Guid? AnalyticalAccountId,
            string? Currency,
            decimal? ExchangeRate,
            string? Description)> lines)
    {
        EnsureEditable();
        _lines.Clear();
        foreach (var l in lines)
            AddLine(l.NotificationReasonId, l.OffsetAccountId, l.Amount, l.CostCenterId,
                l.AnalyticalAccountId, l.Currency, l.ExchangeRate, l.Description);
    }

    public void Submit()
    {
        if (Status != FinancialNoteStatus.Draft)
            throw new BusinessException(ErrorCodes.FinancialNoteNotEditable, "Only draft notes can be submitted.");
        EnsureHasLines();
        Status = FinancialNoteStatus.Submitted;
    }

    public void Approve(Guid userId)
    {
        if (Status is not (FinancialNoteStatus.Draft or FinancialNoteStatus.Submitted))
            throw new BusinessException(ErrorCodes.FinancialNoteNotEditable, "Only draft/submitted notes can be approved.");
        EnsureHasLines();
        Status = FinancialNoteStatus.Approved;
        ApprovedBy = userId;
        ApprovedAt = DateTimeOffset.UtcNow;
    }

    public void MarkPosted(Guid journalEntryId, Guid postedBy)
    {
        if (Status is not (FinancialNoteStatus.Approved or FinancialNoteStatus.Draft))
            throw new BusinessException(ErrorCodes.FinancialNoteNotEditable, "Only approved or draft notes can be posted.");
        EnsureHasLines();
        if (TotalAmount <= 0)
            throw new BusinessException(ErrorCodes.FinancialNoteAmountInvalid, "Note total must be greater than zero.");
        if (journalEntryId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField, "Journal entry is required.");

        JournalEntryId = journalEntryId;
        PostedBy = postedBy;
        PostedAt = DateTimeOffset.UtcNow;
        Status = FinancialNoteStatus.Posted;
    }

    public void MarkReversed()
    {
        if (Status != FinancialNoteStatus.Posted)
            throw new BusinessException(ErrorCodes.FinancialNoteNotPosted, "Only posted notes can be reversed.");
        Status = FinancialNoteStatus.Reversed;
    }

    public void Cancel(Guid userId)
    {
        if (Status is FinancialNoteStatus.Posted or FinancialNoteStatus.Reversed or FinancialNoteStatus.Cancelled)
            throw new BusinessException(ErrorCodes.FinancialNoteNotEditable, "Posted/reversed notes cannot be cancelled.");
        Status = FinancialNoteStatus.Cancelled;
        CancelledBy = userId;
        CancelledAt = DateTimeOffset.UtcNow;
    }

    public void EnsureCanDelete()
    {
        if (Status != FinancialNoteStatus.Draft)
            throw new BusinessException(ErrorCodes.FinancialNoteNotEditable, "Only draft notes can be deleted.");
    }

    public decimal TotalAmount => _lines.Sum(l => l.Amount);
    public decimal TotalAmountInBase => _lines.Sum(l => l.AmountInBase);

    public PostingSource PostingSource =>
        NoteKind == FinancialNoteKind.Debit ? PostingSource.DebitNote : PostingSource.CreditNote;

    private void ApplyHeader(
        FinancialNoteKind noteKind,
        DateOnly noteDate,
        Guid fiscalPeriodId,
        NotificationPartyType partyType,
        Guid mainAccountId,
        string currency,
        decimal exchangeRate,
        Guid? partyId,
        string? partyName,
        FinancialNoteReferenceType referenceType,
        Guid? referenceDocumentId,
        string? referenceNumber,
        string? description,
        string? notes)
    {
        if (!Enum.IsDefined(noteKind))
            throw new BusinessException(ErrorCodes.RequiredField, "Note kind is required.");
        if (fiscalPeriodId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField, "Fiscal period is required.");
        if (mainAccountId == Guid.Empty)
            throw new BusinessException(ErrorCodes.FinancialNoteMainAccountRequired, "Main account is required.");
        if (exchangeRate <= 0)
            throw new BusinessException(ErrorCodes.ReceiptVoucherExchangeRateInvalid, "Exchange rate must be greater than zero.");
        if (!Enum.IsDefined(partyType))
            throw new BusinessException(ErrorCodes.RequiredField, "Party type is required.");

        if (partyType is NotificationPartyType.Customer or NotificationPartyType.Supplier)
        {
            // Party id optional for now; party name recommended for display
        }

        if (string.IsNullOrWhiteSpace(partyName) && partyId is null)
            throw new BusinessException(ErrorCodes.RequiredField, "Party name is required.");

        NoteKind = noteKind;
        NoteDate = noteDate;
        FiscalPeriodId = fiscalPeriodId;
        PartyType = partyType;
        PartyId = partyId == Guid.Empty ? null : partyId;
        PartyName = string.IsNullOrWhiteSpace(partyName) ? null : partyName.Trim();
        MainAccountId = mainAccountId;
        Currency = string.IsNullOrWhiteSpace(currency) ? "SAR" : currency.Trim().ToUpperInvariant();
        ExchangeRate = Math.Round(exchangeRate, 6, MidpointRounding.AwayFromZero);
        ReferenceType = Enum.IsDefined(referenceType) ? referenceType : FinancialNoteReferenceType.None;
        ReferenceDocumentId = referenceDocumentId == Guid.Empty ? null : referenceDocumentId;
        ReferenceNumber = string.IsNullOrWhiteSpace(referenceNumber) ? null : referenceNumber.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    private void EnsureEditable()
    {
        if (Status is not (FinancialNoteStatus.Draft or FinancialNoteStatus.Submitted))
            throw new BusinessException(ErrorCodes.FinancialNoteNotEditable,
                "Only draft/submitted notes can be modified.");
    }

    private void EnsureHasLines()
    {
        if (!_lines.Any())
            throw new BusinessException(ErrorCodes.JournalHasNoLines, "Financial note must have lines.");
    }
}

public sealed class FinancialNoteLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid FinancialNoteId { get; private set; }
    public int LineNumber { get; private set; }
    public Guid NotificationReasonId { get; private set; }
    public Guid OffsetAccountId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public Guid? AnalyticalAccountId { get; private set; }
    public string Currency { get; private set; } = "SAR";
    public decimal ExchangeRate { get; private set; } = 1m;
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }

    public decimal AmountInBase => Math.Round(Amount * ExchangeRate, 2, MidpointRounding.AwayFromZero);

    private FinancialNoteLine() { }

    internal static FinancialNoteLine Create(
        Guid noteId,
        int lineNumber,
        Guid notificationReasonId,
        Guid offsetAccountId,
        decimal amount,
        Guid? costCenterId,
        Guid? analyticalAccountId,
        string currency,
        decimal exchangeRate,
        string? description)
    {
        if (exchangeRate <= 0)
            throw new BusinessException(ErrorCodes.ReceiptVoucherExchangeRateInvalid, "Exchange rate must be greater than zero.");

        return new FinancialNoteLine
        {
            FinancialNoteId = noteId,
            LineNumber = lineNumber,
            NotificationReasonId = notificationReasonId,
            OffsetAccountId = offsetAccountId,
            CostCenterId = costCenterId == Guid.Empty ? null : costCenterId,
            AnalyticalAccountId = analyticalAccountId == Guid.Empty ? null : analyticalAccountId,
            Currency = string.IsNullOrWhiteSpace(currency) ? "SAR" : currency.Trim().ToUpperInvariant(),
            ExchangeRate = Math.Round(exchangeRate, 6, MidpointRounding.AwayFromZero),
            Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        };
    }
}
