using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>
/// Financial opening balances document (أرصدة افتتاحية محاسبية).
/// Distinct from inventory <c>OpeningBalance</c>. Posts an Opening Journal Entry on confirm.
/// </summary>
public sealed class FinancialOpeningBalance : AuditableBaseEntity, ITenantEntity
{
    private readonly List<FinancialOpeningBalanceLine> _lines = [];

    public Guid TenantId { get; private set; }
    public int Number { get; private set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public Guid CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public DateOnly OpeningDate { get; private set; }
    public Guid FiscalPeriodId { get; private set; }
    public string? Description { get; private set; }
    public FinancialOpeningBalanceStatus Status { get; private set; } = FinancialOpeningBalanceStatus.Draft;
    public Guid? EquityAccountId { get; private set; }
    public Guid? JournalEntryId { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public Guid? PostedBy { get; private set; }

    public IReadOnlyCollection<FinancialOpeningBalanceLine> Lines => _lines.AsReadOnly();

    private FinancialOpeningBalance() { }

    public static FinancialOpeningBalance Create(
        Guid tenantId,
        int number,
        string documentNumber,
        Guid companyId,
        DateOnly openingDate,
        Guid fiscalPeriodId,
        Guid? branchId = null,
        string? description = null,
        Guid? equityAccountId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (companyId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField, "Company is required.");
        if (fiscalPeriodId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField, "Fiscal period is required.");
        if (number < 1) throw new ArgumentOutOfRangeException(nameof(number));
        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new BusinessException(ErrorCodes.RequiredField, "Document number is required.");

        return new FinancialOpeningBalance
        {
            TenantId = tenantId,
            Number = number,
            DocumentNumber = documentNumber.Trim(),
            CompanyId = companyId,
            BranchId = branchId,
            OpeningDate = openingDate,
            FiscalPeriodId = fiscalPeriodId,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            EquityAccountId = equityAccountId,
            Status = FinancialOpeningBalanceStatus.Draft
        };
    }

    public void Update(
        DateOnly openingDate,
        Guid fiscalPeriodId,
        Guid? branchId,
        string? description,
        Guid? equityAccountId)
    {
        EnsureDraft();
        OpeningDate = openingDate;
        FiscalPeriodId = fiscalPeriodId;
        BranchId = branchId;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        EquityAccountId = equityAccountId;
    }

    public FinancialOpeningBalanceLine AddLine(
        Guid chartOfAccountId,
        decimal debit,
        decimal credit,
        string currency = "SAR",
        Guid? costCenterId = null,
        string? description = null)
    {
        EnsureDraft();
        if (chartOfAccountId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField, "Account is required.");
        if (debit < 0 || credit < 0)
            throw new BusinessException(ErrorCodes.InvalidJournalAmount);
        if (debit > 0 && credit > 0)
            throw new BusinessException(ErrorCodes.JournalLineBothSides);
        if (debit == 0 && credit == 0)
            throw new BusinessException(ErrorCodes.InvalidJournalAmount);

        var line = FinancialOpeningBalanceLine.Create(
            Id, _lines.Count + 1, chartOfAccountId, debit, credit, currency, costCenterId, description);
        _lines.Add(line);
        return line;
    }

    public void ReplaceLines(IEnumerable<(Guid AccountId, decimal Debit, decimal Credit, string Currency, Guid? CostCenterId, string? Description)> lines)
    {
        EnsureDraft();
        _lines.Clear();
        foreach (var (accountId, debit, credit, currency, costCenterId, description) in lines)
            AddLine(accountId, debit, credit, currency, costCenterId, description);
    }

    public void MarkPosted(Guid journalEntryId, Guid postedBy)
    {
        EnsureDraft();
        if (!_lines.Any())
            throw new BusinessException(ErrorCodes.JournalHasNoLines, "Opening balance must have lines.");
        if (journalEntryId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField, "Journal entry is required.");

        JournalEntryId = journalEntryId;
        PostedBy = postedBy;
        PostedAt = DateTimeOffset.UtcNow;
        Status = FinancialOpeningBalanceStatus.Posted;
    }

    public void MarkReversed()
    {
        if (Status != FinancialOpeningBalanceStatus.Posted)
            throw new BusinessException(ErrorCodes.OpeningBalanceNotPosted, "Only posted opening balances can be reversed.");
        Status = FinancialOpeningBalanceStatus.Reversed;
    }

    public void EnsureCanDelete()
    {
        if (Status != FinancialOpeningBalanceStatus.Draft)
            throw new BusinessException(ErrorCodes.OpeningBalanceNotEditable,
                "Only draft opening balances can be deleted.");
    }

    public decimal TotalDebit => _lines.Sum(l => l.Debit);
    public decimal TotalCredit => _lines.Sum(l => l.Credit);
    public decimal NetDifference => TotalDebit - TotalCredit;

    private void EnsureDraft()
    {
        if (Status != FinancialOpeningBalanceStatus.Draft)
            throw new BusinessException(ErrorCodes.OpeningBalanceNotEditable,
                "Posted opening balances cannot be modified.");
    }
}

public sealed class FinancialOpeningBalanceLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid FinancialOpeningBalanceId { get; private set; }
    public int LineNumber { get; private set; }
    public Guid ChartOfAccountId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public string Currency { get; private set; } = "SAR";
    public string? Description { get; private set; }

    private FinancialOpeningBalanceLine() { }

    internal static FinancialOpeningBalanceLine Create(
        Guid documentId,
        int lineNumber,
        Guid chartOfAccountId,
        decimal debit,
        decimal credit,
        string currency,
        Guid? costCenterId,
        string? description)
    {
        return new FinancialOpeningBalanceLine
        {
            FinancialOpeningBalanceId = documentId,
            LineNumber = lineNumber,
            ChartOfAccountId = chartOfAccountId,
            Debit = Math.Round(debit, 2, MidpointRounding.AwayFromZero),
            Credit = Math.Round(credit, 2, MidpointRounding.AwayFromZero),
            Currency = string.IsNullOrWhiteSpace(currency) ? "SAR" : currency.Trim().ToUpperInvariant(),
            CostCenterId = costCenterId,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        };
    }
}
