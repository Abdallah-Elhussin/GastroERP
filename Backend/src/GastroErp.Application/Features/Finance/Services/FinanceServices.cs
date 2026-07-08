using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Finance.Services;

public sealed class JournalNumberGenerator : IJournalNumberGenerator
{
    private readonly IApplicationDbContext _context;
    public JournalNumberGenerator(IApplicationDbContext context) => _context = context;

    public async Task<string> GenerateAsync(Guid tenantId, CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.JournalEntries
            .CountAsync(j => j.TenantId == tenantId && j.CreatedAt.Year == year, ct);
        return $"JE-{year}-{(count + 1):D6}";
    }
}

public sealed class FiscalPeriodService : IFiscalPeriodService
{
    private readonly IApplicationDbContext _context;
    public FiscalPeriodService(IApplicationDbContext context) => _context = context;

    public async Task<FiscalPeriod> GetOrEnsureOpenPeriodAsync(Guid tenantId, DateOnly postingDate, CancellationToken ct = default)
    {
        var period = await _context.FiscalPeriods
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Status == FiscalPeriodStatus.Open
                && p.StartDate <= postingDate && p.EndDate >= postingDate, ct);

        if (period is not null) return period;

        var year = postingDate.Year;
        period = await _context.FiscalPeriods
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.FiscalYear == year, ct);

        if (period is not null) return period;

        period = FiscalPeriod.Create(tenantId, year, $"FY {year}", new DateOnly(year, 1, 1), new DateOnly(year, 12, 31));
        _context.FiscalPeriods.Add(period);
        return period;
    }

    public async Task<Result> ValidatePeriodAcceptsPostingsAsync(Guid fiscalPeriodId, CancellationToken ct = default)
    {
        var period = await _context.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == fiscalPeriodId, ct);
        if (period is null) return Result.Failure(ErrorCodes.FiscalPeriodNotFound, "Fiscal period not found.");
        try { period.EnsureAcceptsPostings(); return Result.Success(); }
        catch (Domain.Common.Exceptions.BusinessException ex)
        { return Result.Failure(ex.ErrorCode, ex.Message); }
    }
}

public sealed class FinancialValidationService : IFinancialValidationService
{
    private readonly IApplicationDbContext _context;
    public FinancialValidationService(IApplicationDbContext context) => _context = context;

    public async Task<Result> ValidateJournalLinesAsync(Guid tenantId, IReadOnlyList<JournalLineDto> lines, CancellationToken ct = default)
    {
        if (lines.Count == 0) return Result.Failure(ErrorCodes.JournalHasNoLines, "Journal must have at least one line.");

        var totalDebit = lines.Sum(l => l.Debit);
        var totalCredit = lines.Sum(l => l.Credit);
        if (totalDebit != totalCredit)
            return Result.Failure(ErrorCodes.JournalNotBalanced, "Debit and credit totals must be equal.");

        var accountIds = lines.Select(l => l.ChartOfAccountId).Distinct().ToList();
        var accounts = await _context.ChartOfAccounts
            .Where(a => accountIds.Contains(a.Id) && a.TenantId == tenantId)
            .ToListAsync(ct);

        if (accounts.Count != accountIds.Count)
            return Result.Failure(ErrorCodes.AccountNotFound, "One or more accounts not found.");

        foreach (var account in accounts)
        {
            try { account.EnsureCanPost(); }
            catch (Domain.Common.Exceptions.BusinessException ex)
            { return Result.Failure(ex.ErrorCode, ex.Message); }
        }

        return Result.Success();
    }

    public async Task<Result> ValidateAccountCanPostAsync(Guid accountId, CancellationToken ct = default)
    {
        var account = await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.Id == accountId, ct);
        if (account is null) return Result.Failure(ErrorCodes.AccountNotFound, "Account not found.");
        try { account.EnsureCanPost(); return Result.Success(); }
        catch (Domain.Common.Exceptions.BusinessException ex)
        { return Result.Failure(ex.ErrorCode, ex.Message); }
    }
}

public sealed class JournalPostingService : IJournalPostingService
{
    private readonly IApplicationDbContext _context;
    private readonly IJournalNumberGenerator _numberGenerator;
    private readonly IFiscalPeriodService _fiscalPeriodService;
    private readonly IFinancialValidationService _validationService;

    public JournalPostingService(
        IApplicationDbContext context, IJournalNumberGenerator numberGenerator,
        IFiscalPeriodService fiscalPeriodService, IFinancialValidationService validationService)
        => (_context, _numberGenerator, _fiscalPeriodService, _validationService)
            = (context, numberGenerator, fiscalPeriodService, validationService);

    public async Task<Result<JournalEntry>> CreateDraftAsync(
        Guid tenantId, CreateJournalDto dto, CancellationToken ct = default)
    {
        if (dto.Lines is null || dto.Lines.Count == 0)
            return Result<JournalEntry>.Failure(ErrorCodes.JournalHasNoLines, "Journal must have lines.");

        var validation = await _validationService.ValidateJournalLinesAsync(tenantId, dto.Lines, ct);
        if (!validation.IsSuccess) return Result<JournalEntry>.Failure(validation.ErrorCode!, validation.ErrorMessage!);

        var period = await _fiscalPeriodService.GetOrEnsureOpenPeriodAsync(tenantId, dto.PostingDate, ct);
        var periodCheck = await _fiscalPeriodService.ValidatePeriodAcceptsPostingsAsync(period.Id, ct);
        if (!periodCheck.IsSuccess) return Result<JournalEntry>.Failure(periodCheck.ErrorCode!, periodCheck.ErrorMessage!);

        var entryNumber = await _numberGenerator.GenerateAsync(tenantId, ct);
        var journal = JournalEntry.CreateDraft(
            tenantId, entryNumber, dto.PostingDate, period.Id, dto.Description,
            dto.SourceModule, dto.CompanyId, dto.BranchId, dto.Reference, dto.SourceDocumentId);

        var lineNum = 1;
        foreach (var line in dto.Lines)
            journal.AddLine(line.ChartOfAccountId, line.Debit, line.Credit, "SAR", lineNum++, line.CostCenterId, line.Description);

        _context.JournalEntries.Add(journal);
        return Result<JournalEntry>.Success(journal);
    }

    public async Task<Result<JournalEntry>> CreateAndPostAsync(
        Guid tenantId, Guid userId, CreateJournalDto dto, CancellationToken ct = default)
    {
        if (dto.Lines is null || dto.Lines.Count == 0)
            return Result<JournalEntry>.Failure(ErrorCodes.JournalHasNoLines, "Journal must have lines.");

        var validation = await _validationService.ValidateJournalLinesAsync(tenantId, dto.Lines, ct);
        if (!validation.IsSuccess) return Result<JournalEntry>.Failure(validation.ErrorCode!, validation.ErrorMessage!);

        var period = await _fiscalPeriodService.GetOrEnsureOpenPeriodAsync(tenantId, dto.PostingDate, ct);
        var periodCheck = await _fiscalPeriodService.ValidatePeriodAcceptsPostingsAsync(period.Id, ct);
        if (!periodCheck.IsSuccess) return Result<JournalEntry>.Failure(periodCheck.ErrorCode!, periodCheck.ErrorMessage!);

        var entryNumber = await _numberGenerator.GenerateAsync(tenantId, ct);
        var journal = JournalEntry.CreateDraft(
            tenantId, entryNumber, dto.PostingDate, period.Id, dto.Description,
            dto.SourceModule, dto.CompanyId, dto.BranchId, dto.Reference, dto.SourceDocumentId);

        var lineNum = 1;
        foreach (var line in dto.Lines)
            journal.AddLine(line.ChartOfAccountId, line.Debit, line.Credit, "SAR", lineNum++, line.CostCenterId, line.Description);

        journal.Post(userId);
        _context.JournalEntries.Add(journal);
        return Result<JournalEntry>.Success(journal);
    }

    public async Task<Result<JournalEntry>> PostExistingAsync(Guid journalId, Guid userId, CancellationToken ct = default)
    {
        var journal = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == journalId, ct);
        if (journal is null) return Result<JournalEntry>.Failure(ErrorCodes.JournalNotFound, "Journal not found.");

        var periodCheck = await _fiscalPeriodService.ValidatePeriodAcceptsPostingsAsync(journal.FiscalPeriodId, ct);
        if (!periodCheck.IsSuccess) return Result<JournalEntry>.Failure(periodCheck.ErrorCode!, periodCheck.ErrorMessage!);

        try { journal.Post(userId); }
        catch (Domain.Common.Exceptions.BusinessException ex)
        { return Result<JournalEntry>.Failure(ex.ErrorCode, ex.Message); }

        _context.JournalEntries.Update(journal);
        return Result<JournalEntry>.Success(journal);
    }

    public async Task<Result<JournalEntry>> ReverseAsync(Guid journalId, Guid userId, CancellationToken ct = default)
    {
        var original = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == journalId, ct);
        if (original is null) return Result<JournalEntry>.Failure(ErrorCodes.JournalNotFound, "Journal not found.");
        if (original.Status != JournalStatus.Posted)
            return Result<JournalEntry>.Failure(ErrorCodes.JournalNotPosted, "Only posted journals can be reversed.");

        var periodCheck = await _fiscalPeriodService.ValidatePeriodAcceptsPostingsAsync(original.FiscalPeriodId, ct);
        if (!periodCheck.IsSuccess) return Result<JournalEntry>.Failure(periodCheck.ErrorCode!, periodCheck.ErrorMessage!);

        var entryNumber = await _numberGenerator.GenerateAsync(original.TenantId, ct);
        var reversal = JournalEntry.CreateDraft(
            original.TenantId, entryNumber, DateOnly.FromDateTime(DateTime.UtcNow),
            original.FiscalPeriodId, $"Reversal of {original.EntryNumber}", PostingSource.Manual,
            original.CompanyId, original.BranchId, original.EntryNumber, original.SourceDocumentId, original.Id);

        var lineNum = 1;
        foreach (var line in original.Lines)
            reversal.AddLine(line.ChartOfAccountId, line.Credit, line.Debit, line.Currency, lineNum++, line.CostCenterId, line.Description);

        reversal.Post(userId);
        original.MarkReversed(reversal.Id);

        _context.JournalEntries.Add(reversal);
        _context.JournalEntries.Update(original);
        return Result<JournalEntry>.Success(reversal);
    }
}

public sealed class AutoPostingService : IAutoPostingService
{
    private readonly IApplicationDbContext _context;
    private readonly IJournalPostingService _postingService;
    private readonly ILogger<AutoPostingService> _logger;

    public AutoPostingService(
        IApplicationDbContext context, IJournalPostingService postingService,
        ILogger<AutoPostingService> logger)
        => (_context, _postingService, _logger) = (context, postingService, logger);

    public async Task<Result> PostSalesOrderCompletedAsync(Guid salesOrderId, Guid userId, CancellationToken ct = default)
    {
        var exists = await _context.AccountingTransactions
            .AnyAsync(t => t.SourceModule == PostingSource.Sales && t.SourceDocumentId == salesOrderId, ct);
        if (exists) return Result.Success();

        var order = await _context.SalesOrders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == salesOrderId, ct);
        if (order is null) return Result.Failure("NotFound", "Order not found.");

        var invoice = await _context.Invoices.AsNoTracking()
            .FirstOrDefaultAsync(i => i.SalesOrderId == salesOrderId && i.Status == InvoiceStatus.Finalized, ct);

        var grandTotal = invoice?.GrandTotal ?? order.GrandTotal;
        var taxTotal = invoice?.TaxTotal ?? order.TaxTotal;
        var revenue = grandTotal - taxTotal;
        if (grandTotal <= 0) return Result.Success();

        var accounts = await EnsureStandardAccountsAsync(order.TenantId, ct);
        if (accounts is null) return Result.Failure(ErrorCodes.StandardAccountsNotConfigured, "Standard accounts not configured.");

        var postingDate = DateOnly.FromDateTime((order.CompletedAt ?? DateTimeOffset.UtcNow).UtcDateTime);
        var dto = new CreateJournalDto(
            postingDate, $"Sales order {order.OrderNumber}", PostingSource.Sales,
            order.BranchId, order.CompanyId, order.OrderNumber, salesOrderId,
            [
                new JournalLineDto(null, accounts.ArId, null, grandTotal, 0, "Accounts Receivable"),
                new JournalLineDto(null, accounts.RevenueId, null, 0, revenue, "Sales Revenue"),
                new JournalLineDto(null, accounts.VatId, null, 0, taxTotal, "VAT Payable")
            ]);

        var result = await _postingService.CreateAndPostAsync(order.TenantId, userId, dto, ct);
        if (!result.IsSuccess) return Result.Failure(result.ErrorCode!, result.ErrorMessage!);

        var txn = AccountingTransaction.Create(
            order.TenantId, PostingSource.Sales, salesOrderId, result.Data!.Id, order.OrderNumber);
        _context.AccountingTransactions.Add(txn);
        _logger.LogInformation("Posted sales journal for order {OrderId}", salesOrderId);
        return Result.Success();
    }

    public async Task<Result> PostPaymentCompletedAsync(Guid paymentId, Guid userId, CancellationToken ct = default)
    {
        var exists = await _context.AccountingTransactions
            .AnyAsync(t => t.SourceModule == PostingSource.Payment && t.SourceDocumentId == paymentId, ct);
        if (exists) return Result.Success();

        var payment = await _context.Payments.AsNoTracking()
            .Include(p => p.Allocations)
            .FirstOrDefaultAsync(p => p.Id == paymentId, ct);
        if (payment is null) return Result.Failure("NotFound", "Payment not found.");
        if (payment.Status != PaymentStatus.Completed) return Result.Success();

        var amount = payment.Amount;
        if (amount <= 0) return Result.Success();

        var accounts = await EnsureStandardAccountsAsync(payment.TenantId, ct);
        if (accounts is null) return Result.Failure(ErrorCodes.StandardAccountsNotConfigured, "Standard accounts not configured.");

        var postingDate = DateOnly.FromDateTime(payment.ProcessedAt.UtcDateTime);
        var dto = new CreateJournalDto(
            postingDate, $"Payment {payment.ReceiptNumber}", PostingSource.Payment,
            payment.BranchId, null, payment.ReceiptNumber, paymentId,
            [
                new JournalLineDto(null, accounts.CashId, null, amount, 0, "Cash Receipt"),
                new JournalLineDto(null, accounts.ArId, null, 0, amount, "Accounts Receivable")
            ]);

        var result = await _postingService.CreateAndPostAsync(payment.TenantId, userId, dto, ct);
        if (!result.IsSuccess) return Result.Failure(result.ErrorCode!, result.ErrorMessage!);

        var txn = AccountingTransaction.Create(
            payment.TenantId, PostingSource.Payment, paymentId, result.Data!.Id, payment.ReceiptNumber);
        _context.AccountingTransactions.Add(txn);
        _logger.LogInformation("Posted payment journal for payment {PaymentId}", paymentId);
        return Result.Success();
    }

    public async Task<Result> PostPayrollRunAsync(Guid payrollRunId, Guid userId, CancellationToken ct = default)
    {
        var run = await _context.PayrollRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == payrollRunId, ct);
        if (run is null) return Result.Failure("NotFound", "Payroll run not found.");
        if (run.TotalNet <= 0) return Result.Success();

        var exists = await _context.AccountingTransactions
            .AnyAsync(t => t.SourceModule == PostingSource.Payroll && t.SourceDocumentId == payrollRunId, ct);
        if (exists) return Result.Success();

        var accounts = await EnsurePayrollAccountsAsync(run.TenantId, ct);
        if (accounts is null) return Result.Failure(ErrorCodes.StandardAccountsNotConfigured, "Payroll accounts not configured.");

        var postingDate = new DateOnly(run.Year, run.Month, DateTime.DaysInMonth(run.Year, run.Month));
        var dto = new CreateJournalDto(
            postingDate, $"Payroll {run.Year}-{run.Month:D2}", PostingSource.Payroll,
            null, run.CompanyId, $"PAY-{run.Year}{run.Month:D2}", payrollRunId,
            [
                new JournalLineDto(null, accounts.ExpenseId, null, run.TotalNet, 0, "Salary Expense"),
                new JournalLineDto(null, accounts.PayableId, null, 0, run.TotalNet, "Salaries Payable")
            ]);

        var result = await _postingService.CreateAndPostAsync(run.TenantId, userId, dto, ct);
        if (!result.IsSuccess) return Result.Failure(result.ErrorCode!, result.ErrorMessage!);

        var txn = AccountingTransaction.Create(run.TenantId, PostingSource.Payroll, payrollRunId, result.Data!.Id, $"PAY-{run.Year}{run.Month:D2}");
        _context.AccountingTransactions.Add(txn);

        _logger.LogInformation("Posted payroll journal for run {RunId}", payrollRunId);
        return Result.Success();
    }

    private async Task<PayrollAccounts?> EnsurePayrollAccountsAsync(Guid tenantId, CancellationToken ct)
    {
        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .ToListAsync(ct);

        var expense = accounts.FirstOrDefault(a => a.AccountNumber == StandardAccountCodes.SalaryExpense);
        var payable = accounts.FirstOrDefault(a => a.AccountNumber == StandardAccountCodes.SalariesPayable);

        if (expense is null)
        {
            expense = ChartOfAccount.Create(tenantId, StandardAccountCodes.SalaryExpense, "مصروف الرواتب", AccountType.Expense, AccountCategory.OperatingExpense);
            _context.ChartOfAccounts.Add(expense);
        }
        if (payable is null)
        {
            payable = ChartOfAccount.Create(tenantId, StandardAccountCodes.SalariesPayable, "رواتب مستحقة", AccountType.Liability, AccountCategory.CurrentLiability);
            _context.ChartOfAccounts.Add(payable);
        }

        await _context.SaveChangesAsync(ct);
        return new PayrollAccounts(expense.Id, payable.Id);
    }

    private async Task<StandardAccounts?> EnsureStandardAccountsAsync(Guid tenantId, CancellationToken ct)
    {
        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .ToListAsync(ct);

        var cash = accounts.FirstOrDefault(a => a.AccountNumber == StandardAccountCodes.Cash);
        var ar = accounts.FirstOrDefault(a => a.AccountNumber == StandardAccountCodes.AccountsReceivable);
        var vat = accounts.FirstOrDefault(a => a.AccountNumber == StandardAccountCodes.VatPayable);
        var revenue = accounts.FirstOrDefault(a => a.AccountNumber == StandardAccountCodes.SalesRevenue);

        if (cash is null)
        {
            cash = ChartOfAccount.Create(tenantId, StandardAccountCodes.Cash, "النقدية", AccountType.Asset, AccountCategory.CurrentAsset);
            _context.ChartOfAccounts.Add(cash);
        }
        if (ar is null)
        {
            ar = ChartOfAccount.Create(tenantId, StandardAccountCodes.AccountsReceivable, "ذمم مدينة", AccountType.Asset, AccountCategory.CurrentAsset);
            _context.ChartOfAccounts.Add(ar);
        }
        if (vat is null)
        {
            vat = ChartOfAccount.Create(tenantId, StandardAccountCodes.VatPayable, "ضريبة القيمة المضافة", AccountType.Liability, AccountCategory.CurrentLiability);
            _context.ChartOfAccounts.Add(vat);
        }
        if (revenue is null)
        {
            revenue = ChartOfAccount.Create(tenantId, StandardAccountCodes.SalesRevenue, "إيرادات المبيعات", AccountType.Revenue, AccountCategory.OperatingRevenue);
            _context.ChartOfAccounts.Add(revenue);
        }

        await _context.SaveChangesAsync(ct);
        return new StandardAccounts(cash.Id, ar.Id, vat.Id, revenue.Id);
    }

    private sealed record PayrollAccounts(Guid ExpenseId, Guid PayableId);
    private sealed record StandardAccounts(Guid CashId, Guid ArId, Guid VatId, Guid RevenueId);
}

public sealed class AccountBalanceService : IAccountBalanceService
{
    private readonly IApplicationDbContext _context;
    public AccountBalanceService(IApplicationDbContext context) => _context = context;

    public async Task<decimal> GetAccountBalanceAsync(Guid accountId, DateOnly? asOfDate = null, CancellationToken ct = default)
    {
        var query = _context.JournalEntryLines.AsNoTracking()
            .Where(l => l.ChartOfAccountId == accountId)
            .Join(_context.JournalEntries.AsNoTracking().Where(j => j.Status == JournalStatus.Posted),
                l => l.JournalEntryId, j => j.Id, (l, j) => new { l, j });

        if (asOfDate.HasValue)
            query = query.Where(x => x.j.PostingDate <= asOfDate.Value);

        var totals = await query
            .GroupBy(_ => 1)
            .Select(g => new { Debit = g.Sum(x => x.l.Debit), Credit = g.Sum(x => x.l.Credit) })
            .FirstOrDefaultAsync(ct);

        return (totals?.Debit ?? 0) - (totals?.Credit ?? 0);
    }

    public async Task<IReadOnlyList<GeneralLedgerLineDto>> GetGeneralLedgerAsync(GeneralLedgerFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.JournalEntryLines.AsNoTracking()
            .Where(l => l.ChartOfAccountId == filter.AccountId)
            .Join(_context.JournalEntries.AsNoTracking().Where(j => j.Status == JournalStatus.Posted),
                l => l.JournalEntryId, j => j.Id, (l, j) => new { l, j });

        if (filter.FromDate.HasValue) query = query.Where(x => x.j.PostingDate >= filter.FromDate.Value);
        if (filter.ToDate.HasValue) query = query.Where(x => x.j.PostingDate <= filter.ToDate.Value);

        var rows = await query.OrderBy(x => x.j.PostingDate).ThenBy(x => x.j.EntryNumber)
            .Select(x => new { x.j.PostingDate, x.j.EntryNumber, x.j.Description, x.l.Debit, x.l.Credit })
            .ToListAsync(ct);

        decimal running = 0;
        return rows.Select(r =>
        {
            running += r.Debit - r.Credit;
            return new GeneralLedgerLineDto(r.PostingDate, r.EntryNumber, r.Description, r.Debit, r.Credit, running);
        }).ToList();
    }
}

public sealed class TrialBalanceService : ITrialBalanceService
{
    private readonly IApplicationDbContext _context;
    public TrialBalanceService(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<TrialBalanceLineDto>> GetTrialBalanceAsync(
        Guid tenantId, TrialBalanceFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.JournalEntryLines.AsNoTracking()
            .Join(_context.JournalEntries.AsNoTracking().Where(j => j.TenantId == tenantId && j.Status == JournalStatus.Posted),
                l => l.JournalEntryId, j => j.Id, (l, j) => new { l, j })
            .Join(_context.ChartOfAccounts.AsNoTracking().Where(a => a.TenantId == tenantId),
                x => x.l.ChartOfAccountId, a => a.Id, (x, a) => new { x.l, x.j, a });

        if (filter.AsOfDate.HasValue)
            query = query.Where(x => x.j.PostingDate <= filter.AsOfDate.Value);

        var grouped = await query
            .GroupBy(x => new { x.a.Id, x.a.AccountNumber, x.a.NameAr, x.a.AccountType })
            .Select(g => new
            {
                g.Key.Id, g.Key.AccountNumber, g.Key.NameAr, g.Key.AccountType,
                Debit = g.Sum(x => x.l.Debit), Credit = g.Sum(x => x.l.Credit)
            })
            .OrderBy(x => x.AccountNumber)
            .ToListAsync(ct);

        return grouped.Select(g =>
        {
            var net = g.Debit - g.Credit;
            return new TrialBalanceLineDto(
                g.Id, g.AccountNumber, g.NameAr, g.AccountType,
                net > 0 ? net : 0, net < 0 ? Math.Abs(net) : 0);
        }).ToList();
    }

    public async Task<BalanceVerificationDto> VerifyBalanceAsync(
        Guid tenantId, TrialBalanceFilterDto filter, CancellationToken ct = default)
    {
        var lines = await GetTrialBalanceAsync(tenantId, filter, ct);
        var totalDebit = lines.Sum(l => l.DebitBalance);
        var totalCredit = lines.Sum(l => l.CreditBalance);

        var journalQuery = _context.JournalEntries.AsNoTracking()
            .Where(j => j.TenantId == tenantId && j.Status == JournalStatus.Posted);
        if (filter.AsOfDate.HasValue)
            journalQuery = journalQuery.Where(j => j.PostingDate <= filter.AsOfDate.Value);

        var count = await journalQuery.CountAsync(ct);
        return new BalanceVerificationDto(totalDebit, totalCredit, totalDebit == totalCredit, count);
    }
}
