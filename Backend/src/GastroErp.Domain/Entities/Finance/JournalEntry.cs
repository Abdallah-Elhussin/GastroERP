using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Finance;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>JournalEntry — قيد يومية (Aggregate Root)</summary>
public sealed class JournalEntry : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string EntryNumber { get; private set; }
    public DateOnly PostingDate { get; private set; }
    public Guid FiscalPeriodId { get; private set; }
    public string Description { get; private set; }
    public string? Reference { get; private set; }
    public JournalVoucherType VoucherType { get; private set; }
    public PostingSource SourceModule { get; private set; }
    public Guid? SourceDocumentId { get; private set; }
    public JournalStatus Status { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public Guid? PostedBy { get; private set; }
    public Guid? ReversalOfJournalId { get; private set; }
    public Guid? ReversedByJournalId { get; private set; }

    private readonly List<JournalEntryLine> _lines = [];
    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

    private JournalEntry()
    {
        EntryNumber = string.Empty;
        Description = string.Empty;
        VoucherType = JournalVoucherType.Ordinary;
    }

    public static JournalEntry CreateDraft(
        Guid tenantId, string entryNumber, DateOnly postingDate, Guid fiscalPeriodId,
        string description, PostingSource source, Guid? companyId = null, Guid? branchId = null,
        string? reference = null, Guid? sourceDocumentId = null, Guid? reversalOfJournalId = null,
        JournalVoucherType voucherType = JournalVoucherType.Ordinary)
    {
        if (string.IsNullOrWhiteSpace(entryNumber)) throw new BusinessException(ErrorCodes.JournalNumberRequired);
        if (string.IsNullOrWhiteSpace(description)) throw new BusinessException(ErrorCodes.RequiredField);

        if (source == PostingSource.Manual
            && voucherType is JournalVoucherType.Opening or JournalVoucherType.Reversal
            && reversalOfJournalId is null)
            throw new BusinessException(ErrorCodes.JournalInvalidVoucherType);

        return new JournalEntry
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            EntryNumber = entryNumber,
            PostingDate = postingDate,
            FiscalPeriodId = fiscalPeriodId,
            Description = description,
            Reference = reference,
            VoucherType = voucherType,
            SourceModule = source,
            SourceDocumentId = sourceDocumentId,
            ReversalOfJournalId = reversalOfJournalId,
            Status = JournalStatus.Draft
        };
    }

    public JournalEntryLine AddLine(
        Guid accountId, decimal debit, decimal credit, string currency,
        int lineNumber, Guid? costCenterId = null, string? lineDescription = null,
        decimal exchangeRate = 1m, Guid? analyticalAccountId = null)
    {
        EnsureModifiable();
        if (debit < 0 || credit < 0) throw new BusinessException(ErrorCodes.InvalidJournalAmount);
        if (debit > 0 && credit > 0) throw new BusinessException(ErrorCodes.JournalLineBothSides);
        if (debit == 0 && credit == 0) throw new BusinessException(ErrorCodes.InvalidJournalAmount);
        if (exchangeRate <= 0) throw new BusinessException(ErrorCodes.InvalidJournalAmount);

        var line = new JournalEntryLine(
            Id, accountId, debit, credit, currency, lineNumber, costCenterId, lineDescription,
            exchangeRate, analyticalAccountId);
        _lines.Add(line);
        return line;
    }

    public void UpdateDraft(
        DateOnly postingDate,
        Guid fiscalPeriodId,
        string description,
        Guid? companyId,
        Guid? branchId,
        string? reference,
        JournalVoucherType? voucherType = null)
    {
        EnsureModifiable();
        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessException(ErrorCodes.RequiredField);

        if (voucherType.HasValue)
        {
            if (voucherType is JournalVoucherType.Opening or JournalVoucherType.Reversal)
                throw new BusinessException(ErrorCodes.JournalInvalidVoucherType);
            VoucherType = voucherType.Value;
        }

        PostingDate = postingDate;
        FiscalPeriodId = fiscalPeriodId;
        Description = description.Trim();
        CompanyId = companyId;
        BranchId = branchId;
        Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim();
    }

    public void ClearLines()
    {
        EnsureModifiable();
        _lines.Clear();
    }

    public void EnsureCanDelete()
    {
        if (Status != JournalStatus.Draft)
            throw new BusinessException(ErrorCodes.JournalNotEditable, "Only draft journals can be deleted.");
        if (SourceModule != PostingSource.Manual)
            throw new BusinessException(ErrorCodes.JournalNotEditable, "Only manual draft journals can be deleted.");
    }

    /// <summary>Draft → Approved. Manual vouchers require ≥2 balanced lines.</summary>
    public void Approve()
    {
        if (Status != JournalStatus.Draft)
            throw new BusinessException(ErrorCodes.JournalNotDraft);
        if (!_lines.Any())
            throw new BusinessException(ErrorCodes.JournalHasNoLines);
        if (SourceModule == PostingSource.Manual && _lines.Count < 2)
            throw new BusinessException(ErrorCodes.JournalMinTwoLines);

        var totalDebit = _lines.Sum(l => l.Debit);
        var totalCredit = _lines.Sum(l => l.Credit);
        if (totalDebit != totalCredit)
            throw new BusinessException(ErrorCodes.JournalNotBalanced);

        Status = JournalStatus.Approved;
    }

    public void Post(Guid postedBy)
    {
        // System / reversal drafts can be posted without a separate Approve API call.
        if (Status == JournalStatus.Draft
            && (SourceModule != PostingSource.Manual || ReversalOfJournalId.HasValue))
        {
            Approve();
        }

        if (Status != JournalStatus.Approved)
            throw new BusinessException(ErrorCodes.JournalNotApproved);
        if (!_lines.Any())
            throw new BusinessException(ErrorCodes.JournalHasNoLines);

        var totalDebit = _lines.Sum(l => l.Debit);
        var totalCredit = _lines.Sum(l => l.Credit);
        if (totalDebit != totalCredit)
            throw new BusinessException(ErrorCodes.JournalNotBalanced);

        Status = JournalStatus.Posted;
        PostedAt = DateTimeOffset.UtcNow;
        PostedBy = postedBy;
        RaiseDomainEvent(new JournalPostedEvent(Id, TenantId, EntryNumber, SourceModule));
    }

    public void MarkReversed(Guid reversalJournalId)
    {
        if (Status != JournalStatus.Posted)
            throw new BusinessException(ErrorCodes.JournalNotPosted);
        ReversedByJournalId = reversalJournalId;
        Status = JournalStatus.Reversed;
        RaiseDomainEvent(new JournalReversedEvent(Id, reversalJournalId, TenantId));
    }

    private void EnsureModifiable()
    {
        if (Status != JournalStatus.Draft)
            throw new BusinessException(ErrorCodes.JournalNotEditable);
    }
}

public sealed class JournalEntryLine : AuditableBaseEntity
{
    public Guid JournalEntryId { get; private set; }
    public Guid ChartOfAccountId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public Guid? AnalyticalAccountId { get; private set; }
    public int LineNumber { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public string Currency { get; private set; }
    public decimal ExchangeRate { get; private set; }
    public string? Description { get; private set; }

    private JournalEntryLine()
    {
        Currency = "SAR";
        ExchangeRate = 1m;
    }

    internal JournalEntryLine(
        Guid journalEntryId, Guid accountId, decimal debit, decimal credit,
        string currency, int lineNumber, Guid? costCenterId, string? description,
        decimal exchangeRate = 1m, Guid? analyticalAccountId = null)
    {
        JournalEntryId = journalEntryId;
        ChartOfAccountId = accountId;
        CostCenterId = costCenterId;
        AnalyticalAccountId = analyticalAccountId == Guid.Empty ? null : analyticalAccountId;
        Debit = debit;
        Credit = credit;
        Currency = currency.ToUpperInvariant();
        ExchangeRate = exchangeRate <= 0 ? 1m : exchangeRate;
        LineNumber = lineNumber;
        Description = description;
    }
}
