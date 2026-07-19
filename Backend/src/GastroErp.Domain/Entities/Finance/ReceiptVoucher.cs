using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>
/// سند قبض مالي. عند الترحيل يُنشأ قيد يومية بمصدر <see cref="PostingSource.Receipt"/>.
/// مدين: الصندوق/البنك — دائن: بنود السند.
/// </summary>
public sealed class ReceiptVoucher : AuditableBaseEntity, ITenantEntity
{
    private readonly List<ReceiptVoucherLine> _lines = [];

    public Guid TenantId { get; private set; }
    public int Number { get; private set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public Guid CompanyId { get; private set; }
    public Guid BranchId { get; private set; }
    public DateOnly VoucherDate { get; private set; }
    public Guid FiscalPeriodId { get; private set; }
    public ReceiptMethod ReceiptMethod { get; private set; }
    public Guid? CashBoxId { get; private set; }
    public Guid? BankId { get; private set; }
    public ReceiptPartyType PartyType { get; private set; }
    public Guid? PartyId { get; private set; }
    public string? PartyName { get; private set; }
    public string Currency { get; private set; } = "SAR";
    public decimal ExchangeRate { get; private set; } = 1m;
    public Guid? CostCenterId { get; private set; }
    public string? Reference { get; private set; }
    public string? ChequeNumber { get; private set; }
    public DateOnly? ChequeDate { get; private set; }
    public string? Description { get; private set; }
    public string? Notes { get; private set; }
    public ReceiptVoucherStatus Status { get; private set; } = ReceiptVoucherStatus.Draft;
    public Guid? JournalEntryId { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public Guid? PostedBy { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? CancelledBy { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    public IReadOnlyCollection<ReceiptVoucherLine> Lines => _lines.AsReadOnly();

    private ReceiptVoucher() { }

    public static ReceiptVoucher Create(
        Guid tenantId,
        int number,
        string documentNumber,
        Guid companyId,
        Guid branchId,
        DateOnly voucherDate,
        Guid fiscalPeriodId,
        ReceiptMethod receiptMethod,
        ReceiptPartyType partyType,
        Guid? cashBoxId = null,
        Guid? bankId = null,
        Guid? partyId = null,
        string? partyName = null,
        string currency = "SAR",
        decimal exchangeRate = 1m,
        Guid? costCenterId = null,
        string? reference = null,
        string? description = null,
        string? notes = null,
        string? chequeNumber = null,
        DateOnly? chequeDate = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (companyId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField, "Company is required.");
        if (branchId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField, "Branch is required.");
        if (fiscalPeriodId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField, "Fiscal period is required.");
        if (number < 1) throw new ArgumentOutOfRangeException(nameof(number));
        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new BusinessException(ErrorCodes.RequiredField, "Document number is required.");

        var voucher = new ReceiptVoucher
        {
            TenantId = tenantId,
            Number = number,
            DocumentNumber = documentNumber.Trim(),
            CompanyId = companyId,
            BranchId = branchId,
            Status = ReceiptVoucherStatus.Draft
        };
        voucher.ApplyHeader(
            voucherDate, fiscalPeriodId, receiptMethod, partyType,
            cashBoxId, bankId, partyId, partyName, currency, exchangeRate,
            costCenterId, reference, description, notes, chequeNumber, chequeDate);
        return voucher;
    }

    public void Update(
        DateOnly voucherDate,
        Guid fiscalPeriodId,
        ReceiptMethod receiptMethod,
        ReceiptPartyType partyType,
        Guid? cashBoxId,
        Guid? bankId,
        Guid? partyId,
        string? partyName,
        string currency,
        decimal exchangeRate,
        Guid? costCenterId,
        string? reference,
        string? description,
        string? notes,
        string? chequeNumber,
        DateOnly? chequeDate)
    {
        EnsureEditable();
        ApplyHeader(
            voucherDate, fiscalPeriodId, receiptMethod, partyType,
            cashBoxId, bankId, partyId, partyName, currency, exchangeRate,
            costCenterId, reference, description, notes, chequeNumber, chequeDate);
    }

    public ReceiptVoucherLine AddLine(
        Guid chartOfAccountId,
        decimal amount,
        Guid? costCenterId = null,
        string? description = null,
        string? currency = null,
        decimal? exchangeRate = null,
        Guid? analyticalAccountId = null)
    {
        EnsureEditable();
        if (chartOfAccountId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField, "Account is required.");
        if (amount <= 0)
            throw new BusinessException(ErrorCodes.ReceiptVoucherAmountInvalid, "Line amount must be greater than zero.");

        var lineCurrency = string.IsNullOrWhiteSpace(currency) ? Currency : currency.Trim().ToUpperInvariant();
        var lineRate = exchangeRate is > 0 ? exchangeRate.Value : ExchangeRate;

        var line = ReceiptVoucherLine.Create(
            Id, _lines.Count + 1, chartOfAccountId, amount, costCenterId, description,
            lineCurrency, lineRate, analyticalAccountId);
        _lines.Add(line);
        return line;
    }

    public void ReplaceLines(
        IEnumerable<(
            Guid AccountId,
            decimal Amount,
            Guid? CostCenterId,
            string? Description,
            string? Currency,
            decimal? ExchangeRate,
            Guid? AnalyticalAccountId)> lines)
    {
        EnsureEditable();
        _lines.Clear();
        foreach (var (accountId, amount, costCenterId, description, currency, exchangeRate, analyticalAccountId) in lines)
            AddLine(accountId, amount, costCenterId, description, currency, exchangeRate, analyticalAccountId);
    }

    public void Submit()
    {
        if (Status != ReceiptVoucherStatus.Draft)
            throw new BusinessException(ErrorCodes.ReceiptVoucherNotEditable, "Only draft vouchers can be submitted.");
        EnsureHasLines();
        Status = ReceiptVoucherStatus.Submitted;
    }

    public void Approve(Guid userId)
    {
        if (Status is not (ReceiptVoucherStatus.Draft or ReceiptVoucherStatus.Submitted))
            throw new BusinessException(ErrorCodes.ReceiptVoucherNotEditable, "Only draft/submitted vouchers can be approved.");
        EnsureHasLines();
        Status = ReceiptVoucherStatus.Approved;
        ApprovedBy = userId;
        ApprovedAt = DateTimeOffset.UtcNow;
    }

    public void MarkPosted(Guid journalEntryId, Guid postedBy)
    {
        if (Status != ReceiptVoucherStatus.Approved && Status != ReceiptVoucherStatus.Draft)
            throw new BusinessException(ErrorCodes.ReceiptVoucherNotEditable,
                "Only approved (or draft) receipt vouchers can be posted.");
        EnsureHasLines();
        if (TotalAmount <= 0)
            throw new BusinessException(ErrorCodes.ReceiptVoucherAmountInvalid, "Receipt total must be greater than zero.");
        if (journalEntryId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField, "Journal entry is required.");

        JournalEntryId = journalEntryId;
        PostedBy = postedBy;
        PostedAt = DateTimeOffset.UtcNow;
        Status = ReceiptVoucherStatus.Posted;
    }

    public void MarkReversed()
    {
        if (Status != ReceiptVoucherStatus.Posted)
            throw new BusinessException(ErrorCodes.ReceiptVoucherNotPosted, "Only posted receipt vouchers can be reversed.");
        Status = ReceiptVoucherStatus.Reversed;
    }

    public void Cancel(Guid userId)
    {
        if (Status is ReceiptVoucherStatus.Posted or ReceiptVoucherStatus.Reversed or ReceiptVoucherStatus.Cancelled)
            throw new BusinessException(ErrorCodes.ReceiptVoucherNotEditable, "Posted/reversed vouchers cannot be cancelled.");
        Status = ReceiptVoucherStatus.Cancelled;
        CancelledBy = userId;
        CancelledAt = DateTimeOffset.UtcNow;
    }

    public void EnsureCanDelete()
    {
        if (Status != ReceiptVoucherStatus.Draft)
            throw new BusinessException(ErrorCodes.ReceiptVoucherNotEditable,
                "Only draft receipt vouchers can be deleted.");
    }

    public decimal TotalAmount => _lines.Sum(l => l.Amount);
    public decimal TotalAmountInBase => _lines.Sum(l => l.AmountInBase);

    private void ApplyHeader(
        DateOnly voucherDate,
        Guid fiscalPeriodId,
        ReceiptMethod receiptMethod,
        ReceiptPartyType partyType,
        Guid? cashBoxId,
        Guid? bankId,
        Guid? partyId,
        string? partyName,
        string currency,
        decimal exchangeRate,
        Guid? costCenterId,
        string? reference,
        string? description,
        string? notes,
        string? chequeNumber,
        DateOnly? chequeDate)
    {
        if (fiscalPeriodId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField, "Fiscal period is required.");
        if (exchangeRate <= 0)
            throw new BusinessException(ErrorCodes.ReceiptVoucherExchangeRateInvalid, "Exchange rate must be greater than zero.");
        if (!Enum.IsDefined(receiptMethod))
            throw new BusinessException(ErrorCodes.ReceiptVoucherMethodInvalid, "Invalid receipt method.");

        switch (receiptMethod)
        {
            case ReceiptMethod.Cash:
                if (cashBoxId is null || cashBoxId == Guid.Empty)
                    throw new BusinessException(ErrorCodes.ReceiptVoucherCashBoxRequired, "Cash box is required for cash receipts.");
                bankId = null;
                break;
            case ReceiptMethod.BankTransfer:
            case ReceiptMethod.Cheque:
            case ReceiptMethod.CreditCard:
            case ReceiptMethod.DebitCard:
                if (bankId is null || bankId == Guid.Empty)
                    throw new BusinessException(ErrorCodes.ReceiptVoucherBankRequired, "Bank is required for this receipt method.");
                cashBoxId = null;
                break;
            case ReceiptMethod.Wallet:
            case ReceiptMethod.Other:
                var hasCash = cashBoxId is not null && cashBoxId != Guid.Empty;
                var hasBank = bankId is not null && bankId != Guid.Empty;
                if (!hasCash && !hasBank)
                    throw new BusinessException(ErrorCodes.ReceiptVoucherDestinationInvalid,
                        "Cash box or bank is required.");
                if (hasCash && hasBank)
                    bankId = null;
                break;
            default:
                throw new BusinessException(ErrorCodes.ReceiptVoucherMethodInvalid, "Invalid receipt method.");
        }

        if (receiptMethod == ReceiptMethod.Cheque)
        {
            if (string.IsNullOrWhiteSpace(chequeNumber))
                throw new BusinessException(ErrorCodes.RequiredField, "Cheque number is required.");
        }
        else
        {
            chequeNumber = null;
            chequeDate = null;
        }

        switch (partyType)
        {
            case ReceiptPartyType.Customer:
            case ReceiptPartyType.Supplier:
                if (partyId is null || partyId == Guid.Empty)
                    throw new BusinessException(ErrorCodes.ReceiptVoucherPartyRequired, "Party is required.");
                partyName = null;
                break;
            case ReceiptPartyType.General:
                if (string.IsNullOrWhiteSpace(partyName))
                    throw new BusinessException(ErrorCodes.ReceiptVoucherPartyRequired, "Party name is required for general receipts.");
                partyId = null;
                partyName = partyName.Trim();
                break;
            default:
                throw new BusinessException(ErrorCodes.ReceiptVoucherPartyInvalid, "Invalid party type.");
        }

        VoucherDate = voucherDate;
        FiscalPeriodId = fiscalPeriodId;
        ReceiptMethod = receiptMethod;
        CashBoxId = cashBoxId == Guid.Empty ? null : cashBoxId;
        BankId = bankId == Guid.Empty ? null : bankId;
        PartyType = partyType;
        PartyId = partyId;
        PartyName = partyName;
        Currency = string.IsNullOrWhiteSpace(currency) ? "SAR" : currency.Trim().ToUpperInvariant();
        ExchangeRate = Math.Round(exchangeRate, 6, MidpointRounding.AwayFromZero);
        CostCenterId = costCenterId == Guid.Empty ? null : costCenterId;
        Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        ChequeNumber = string.IsNullOrWhiteSpace(chequeNumber) ? null : chequeNumber.Trim();
        ChequeDate = chequeDate;
    }

    private void EnsureEditable()
    {
        if (Status is not (ReceiptVoucherStatus.Draft or ReceiptVoucherStatus.Submitted))
            throw new BusinessException(ErrorCodes.ReceiptVoucherNotEditable,
                "Only draft/submitted receipt vouchers can be modified.");
    }

    private void EnsureHasLines()
    {
        if (!_lines.Any())
            throw new BusinessException(ErrorCodes.JournalHasNoLines, "Receipt voucher must have lines.");
    }
}

public sealed class ReceiptVoucherLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ReceiptVoucherId { get; private set; }
    public int LineNumber { get; private set; }
    public Guid ChartOfAccountId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public Guid? AnalyticalAccountId { get; private set; }
    public string Currency { get; private set; } = "SAR";
    public decimal ExchangeRate { get; private set; } = 1m;
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }

    public decimal AmountInBase => Math.Round(Amount * ExchangeRate, 2, MidpointRounding.AwayFromZero);

    private ReceiptVoucherLine() { }

    internal static ReceiptVoucherLine Create(
        Guid voucherId,
        int lineNumber,
        Guid chartOfAccountId,
        decimal amount,
        Guid? costCenterId,
        string? description,
        string currency,
        decimal exchangeRate,
        Guid? analyticalAccountId)
    {
        if (exchangeRate <= 0)
            throw new BusinessException(ErrorCodes.ReceiptVoucherExchangeRateInvalid, "Exchange rate must be greater than zero.");

        return new ReceiptVoucherLine
        {
            ReceiptVoucherId = voucherId,
            LineNumber = lineNumber,
            ChartOfAccountId = chartOfAccountId,
            Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero),
            CostCenterId = costCenterId == Guid.Empty ? null : costCenterId,
            AnalyticalAccountId = analyticalAccountId == Guid.Empty ? null : analyticalAccountId,
            Currency = string.IsNullOrWhiteSpace(currency) ? "SAR" : currency.Trim().ToUpperInvariant(),
            ExchangeRate = Math.Round(exchangeRate, 6, MidpointRounding.AwayFromZero),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        };
    }
}
