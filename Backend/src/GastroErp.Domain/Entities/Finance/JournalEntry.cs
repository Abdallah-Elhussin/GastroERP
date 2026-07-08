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
    }

    public static JournalEntry CreateDraft(
        Guid tenantId, string entryNumber, DateOnly postingDate, Guid fiscalPeriodId,
        string description, PostingSource source, Guid? companyId = null, Guid? branchId = null,
        string? reference = null, Guid? sourceDocumentId = null, Guid? reversalOfJournalId = null)
    {
        if (string.IsNullOrWhiteSpace(entryNumber)) throw new BusinessException(ErrorCodes.JournalNumberRequired);
        if (string.IsNullOrWhiteSpace(description)) throw new BusinessException(ErrorCodes.RequiredField);

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
            SourceModule = source,
            SourceDocumentId = sourceDocumentId,
            ReversalOfJournalId = reversalOfJournalId,
            Status = JournalStatus.Draft
        };
    }

    public JournalEntryLine AddLine(
        Guid accountId, decimal debit, decimal credit, string currency,
        int lineNumber, Guid? costCenterId = null, string? lineDescription = null)
    {
        EnsureModifiable();
        if (debit < 0 || credit < 0) throw new BusinessException(ErrorCodes.InvalidJournalAmount);
        if (debit > 0 && credit > 0) throw new BusinessException(ErrorCodes.JournalLineBothSides);
        if (debit == 0 && credit == 0) throw new BusinessException(ErrorCodes.InvalidJournalAmount);

        var line = new JournalEntryLine(Id, accountId, debit, credit, currency, lineNumber, costCenterId, lineDescription);
        _lines.Add(line);
        return line;
    }

    public void Post(Guid postedBy)
    {
        if (Status != JournalStatus.Draft)
            throw new BusinessException(ErrorCodes.JournalNotDraft);
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
    public int LineNumber { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public string Currency { get; private set; }
    public string? Description { get; private set; }

    private JournalEntryLine() { Currency = "SAR"; }

    internal JournalEntryLine(
        Guid journalEntryId, Guid accountId, decimal debit, decimal credit,
        string currency, int lineNumber, Guid? costCenterId, string? description)
    {
        JournalEntryId = journalEntryId;
        ChartOfAccountId = accountId;
        CostCenterId = costCenterId;
        Debit = debit;
        Credit = credit;
        Currency = currency.ToUpperInvariant();
        LineNumber = lineNumber;
        Description = description;
    }
}
