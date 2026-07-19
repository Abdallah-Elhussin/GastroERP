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

        period = FiscalPeriod.Create(tenantId, year, 1);
        period.GenerateMonthlyDetails();
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

    public async Task<Result> ValidateJournalLinesAsync(
        Guid tenantId, IReadOnlyList<JournalLineDto> lines, CancellationToken ct = default,
        bool requireBalance = true)
    {
        if (lines.Count == 0) return Result.Failure(ErrorCodes.JournalHasNoLines, "Journal must have at least one line.");

        if (requireBalance)
        {
            var totalDebit = lines.Sum(l => l.Debit);
            var totalCredit = lines.Sum(l => l.Credit);
            if (totalDebit != totalCredit)
                return Result.Failure(ErrorCodes.JournalNotBalanced, "Debit and credit totals must be equal.");
        }

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
        if (dto.SourceModule == PostingSource.Manual && dto.Lines.Count < 2)
            return Result<JournalEntry>.Failure(ErrorCodes.JournalMinTwoLines, "Manual journal requires at least two lines.");

        var validation = await _validationService.ValidateJournalLinesAsync(tenantId, dto.Lines, ct, requireBalance: false);
        if (!validation.IsSuccess) return Result<JournalEntry>.Failure(validation.ErrorCode!, validation.ErrorMessage!);

        var periodResult = await ResolvePeriodAsync(tenantId, dto.PostingDate, dto.FiscalPeriodId, ct);
        if (!periodResult.IsSuccess)
            return Result<JournalEntry>.Failure(periodResult.ErrorCode!, periodResult.ErrorMessage!);
        var period = periodResult.Data!;

        var voucherType = ResolveVoucherType(dto);
        var entryNumber = await _numberGenerator.GenerateAsync(tenantId, ct);
        var journal = JournalEntry.CreateDraft(
            tenantId, entryNumber, dto.PostingDate, period.Id, dto.Description,
            dto.SourceModule, dto.CompanyId, dto.BranchId, dto.Reference, dto.SourceDocumentId,
            voucherType: voucherType);

        var lineNum = 1;
        foreach (var line in dto.Lines)
            journal.AddLine(
                line.ChartOfAccountId, line.Debit, line.Credit,
                string.IsNullOrWhiteSpace(line.Currency) ? "SAR" : line.Currency,
                lineNum++, line.CostCenterId, line.Description,
                line.ExchangeRate <= 0 ? 1m : line.ExchangeRate, line.AnalyticalAccountId);

        _context.JournalEntries.Add(journal);
        return Result<JournalEntry>.Success(journal);
    }

    public async Task<Result<JournalEntry>> CreateAndPostAsync(
        Guid tenantId, Guid userId, CreateJournalDto dto, CancellationToken ct = default)
    {
        if (dto.Lines is null || dto.Lines.Count == 0)
            return Result<JournalEntry>.Failure(ErrorCodes.JournalHasNoLines, "Journal must have lines.");

        var validation = await _validationService.ValidateJournalLinesAsync(tenantId, dto.Lines, ct, requireBalance: true);
        if (!validation.IsSuccess) return Result<JournalEntry>.Failure(validation.ErrorCode!, validation.ErrorMessage!);

        var periodResult = await ResolvePeriodAsync(tenantId, dto.PostingDate, dto.FiscalPeriodId, ct);
        if (!periodResult.IsSuccess)
            return Result<JournalEntry>.Failure(periodResult.ErrorCode!, periodResult.ErrorMessage!);
        var period = periodResult.Data!;

        var voucherType = ResolveVoucherType(dto);
        var entryNumber = await _numberGenerator.GenerateAsync(tenantId, ct);
        var journal = JournalEntry.CreateDraft(
            tenantId, entryNumber, dto.PostingDate, period.Id, dto.Description,
            dto.SourceModule, dto.CompanyId, dto.BranchId, dto.Reference, dto.SourceDocumentId,
            voucherType: voucherType);

        var lineNum = 1;
        foreach (var line in dto.Lines)
            journal.AddLine(
                line.ChartOfAccountId, line.Debit, line.Credit,
                string.IsNullOrWhiteSpace(line.Currency) ? "SAR" : line.Currency,
                lineNum++, line.CostCenterId, line.Description,
                line.ExchangeRate <= 0 ? 1m : line.ExchangeRate, line.AnalyticalAccountId);

        try { journal.Post(userId); }
        catch (Domain.Common.Exceptions.BusinessException ex)
        { return Result<JournalEntry>.Failure(ex.ErrorCode, ex.Message); }

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
            original.CompanyId, original.BranchId, original.EntryNumber, original.SourceDocumentId, original.Id,
            JournalVoucherType.Reversal);

        var lineNum = 1;
        foreach (var line in original.Lines)
            reversal.AddLine(
                line.ChartOfAccountId, line.Credit, line.Debit, line.Currency, lineNum++,
                line.CostCenterId, line.Description, line.ExchangeRate, line.AnalyticalAccountId);

        reversal.Post(userId);
        original.MarkReversed(reversal.Id);

        _context.JournalEntries.Add(reversal);
        _context.JournalEntries.Update(original);
        return Result<JournalEntry>.Success(reversal);
    }

    private static JournalVoucherType ResolveVoucherType(CreateJournalDto dto)
    {
        if (dto.VoucherType.HasValue) return dto.VoucherType.Value;
        return dto.SourceModule switch
        {
            PostingSource.OpeningBalance => JournalVoucherType.Opening,
            _ => JournalVoucherType.Ordinary
        };
    }

    private async Task<Result<FiscalPeriod>> ResolvePeriodAsync(
        Guid tenantId, DateOnly postingDate, Guid? fiscalPeriodId, CancellationToken ct)
    {
        if (fiscalPeriodId.HasValue)
        {
            var existing = await _context.FiscalPeriods
                .FirstOrDefaultAsync(p => p.Id == fiscalPeriodId.Value && p.TenantId == tenantId, ct);
            if (existing is null)
                return Result<FiscalPeriod>.Failure(ErrorCodes.FiscalPeriodNotFound, "Fiscal period not found.");
            var check = await _fiscalPeriodService.ValidatePeriodAcceptsPostingsAsync(existing.Id, ct);
            if (!check.IsSuccess)
                return Result<FiscalPeriod>.Failure(check.ErrorCode!, check.ErrorMessage!);
            return Result<FiscalPeriod>.Success(existing);
        }

        var period = await _fiscalPeriodService.GetOrEnsureOpenPeriodAsync(tenantId, postingDate, ct);
        var periodCheck = await _fiscalPeriodService.ValidatePeriodAcceptsPostingsAsync(period.Id, ct);
        if (!periodCheck.IsSuccess)
            return Result<FiscalPeriod>.Failure(periodCheck.ErrorCode!, periodCheck.ErrorMessage!);
        return Result<FiscalPeriod>.Success(period);
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
        if (accounts is null)
        {
            if (await IsAutoPostDisabledAsync(order.TenantId, sales: true, ct: ct))
                return Result.Success();
            return Result.Failure(ErrorCodes.StandardAccountsNotConfigured, "Standard accounts not configured.");
        }

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
        if (accounts is null)
        {
            if (await IsAutoPostDisabledAsync(payment.TenantId, sales: true, ct: ct))
                return Result.Success();
            return Result.Failure(ErrorCodes.StandardAccountsNotConfigured, "Standard accounts not configured.");
        }

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
        if (accounts is null)
        {
            if (await IsAutoPostDisabledAsync(run.TenantId, payroll: true, ct: ct))
                return Result.Success();
            return Result.Failure(ErrorCodes.StandardAccountsNotConfigured, "Payroll accounts not configured.");
        }

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

    private async Task<bool> IsAutoPostDisabledAsync(
        Guid tenantId, bool sales = false, bool payroll = false, CancellationToken ct = default)
    {
        var settings = await _context.AccountingSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.CompanyId == null, ct);
        if (settings is null) return false;
        if (sales && !settings.AutoPostSales) return true;
        if (payroll && !settings.AutoPostPayroll) return true;
        return false;
    }

    private async Task<PayrollAccounts?> EnsurePayrollAccountsAsync(Guid tenantId, CancellationToken ct)
    {
        var settings = await _context.AccountingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.CompanyId == null, ct);

        if (settings is { AutoPostPayroll: false })
            return null; // caller treats as skip when flagged separately

        if (settings?.PayrollExpenseAccountId is Guid expId && settings.PayrollLiabilityAccountId is Guid payId)
            return new PayrollAccounts(expId, payId);

        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .ToListAsync(ct);

        var expense = accounts.FirstOrDefault(a => a.AccountNumber == StandardAccountCodes.SalaryExpense);
        var payable = accounts.FirstOrDefault(a => a.AccountNumber == StandardAccountCodes.SalariesPayable);

        if (expense is null)
        {
            expense = ChartOfAccount.Create(tenantId, StandardAccountCodes.SalaryExpense, "مصروف الرواتب", AccountType.Expense, AccountCategory.OperatingExpense, isSystemAccount: true);
            _context.ChartOfAccounts.Add(expense);
        }
        if (payable is null)
        {
            payable = ChartOfAccount.Create(tenantId, StandardAccountCodes.SalariesPayable, "رواتب مستحقة", AccountType.Liability, AccountCategory.CurrentLiability, isSystemAccount: true);
            _context.ChartOfAccounts.Add(payable);
        }

        await _context.SaveChangesAsync(ct);
        return new PayrollAccounts(expense.Id, payable.Id);
    }

    private async Task<StandardAccounts?> EnsureStandardAccountsAsync(Guid tenantId, CancellationToken ct)
    {
        var settings = await _context.AccountingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.CompanyId == null, ct);

        if (settings is { AutoPostSales: false })
            return null;

        if (settings?.CashAccountId is Guid cashId &&
            settings.AccountsReceivableAccountId is Guid arId &&
            settings.VatOutputAccountId is Guid vatId &&
            settings.SalesRevenueAccountId is Guid revId)
            return new StandardAccounts(cashId, arId, vatId, revId);

        var accounts = await _context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .ToListAsync(ct);

        var cash = accounts.FirstOrDefault(a => a.AccountNumber == StandardAccountCodes.Cash);
        var ar = accounts.FirstOrDefault(a => a.AccountNumber == StandardAccountCodes.AccountsReceivable);
        var vat = accounts.FirstOrDefault(a => a.AccountNumber == StandardAccountCodes.VatPayable);
        var revenue = accounts.FirstOrDefault(a => a.AccountNumber == StandardAccountCodes.SalesRevenue);

        if (cash is null)
        {
            cash = ChartOfAccount.Create(tenantId, StandardAccountCodes.Cash, "النقدية", AccountType.Asset, AccountCategory.CurrentAsset, isSystemAccount: true);
            _context.ChartOfAccounts.Add(cash);
        }
        if (ar is null)
        {
            ar = ChartOfAccount.Create(tenantId, StandardAccountCodes.AccountsReceivable, "ذمم مدينة", AccountType.Asset, AccountCategory.CurrentAsset, isSystemAccount: true);
            _context.ChartOfAccounts.Add(ar);
        }
        if (vat is null)
        {
            vat = ChartOfAccount.Create(tenantId, StandardAccountCodes.VatPayable, "ضريبة القيمة المضافة", AccountType.Liability, AccountCategory.CurrentLiability, isSystemAccount: true);
            _context.ChartOfAccounts.Add(vat);
        }
        if (revenue is null)
        {
            revenue = ChartOfAccount.Create(tenantId, StandardAccountCodes.SalesRevenue, "إيرادات المبيعات", AccountType.Revenue, AccountCategory.OperatingRevenue, isSystemAccount: true);
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

    public async Task<GeneralLedgerResultDto> GetGeneralLedgerAsync(Guid tenantId, GeneralLedgerFilterDto filter, CancellationToken ct = default)
    {
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);
        var page = Math.Max(filter.Page, 1);
        var skip = (page - 1) * pageSize;

        List<Guid>? accountIds = null;
        if (filter.AccountId.HasValue)
            accountIds = [filter.AccountId.Value];
        else if (filter.ParentAccountId.HasValue || filter.AccountType.HasValue)
            accountIds = await ResolveAccountIdsAsync(tenantId, filter, ct);

        var posted = _context.JournalEntries.AsNoTracking()
            .Where(j => j.TenantId == tenantId && j.Status == JournalStatus.Posted);

        if (filter.CompanyId.HasValue)
            posted = posted.Where(j => j.CompanyId == filter.CompanyId);
        if (filter.BranchId.HasValue)
            posted = posted.Where(j => j.BranchId == filter.BranchId);
        if (filter.FiscalPeriodId.HasValue)
            posted = posted.Where(j => j.FiscalPeriodId == filter.FiscalPeriodId);
        if (filter.FiscalYear.HasValue)
        {
            var year = filter.FiscalYear.Value;
            var periodIds = _context.FiscalPeriods.AsNoTracking()
                .Where(p => p.TenantId == tenantId && p.FiscalYear == year)
                .Select(p => p.Id);
            posted = posted.Where(j => periodIds.Contains(j.FiscalPeriodId));
        }
        if (filter.SourceModule.HasValue)
            posted = posted.Where(j => j.SourceModule == filter.SourceModule);
        if (filter.PostedBy.HasValue)
            posted = posted.Where(j => j.PostedBy == filter.PostedBy);
        if (!string.IsNullOrWhiteSpace(filter.DocumentNumber))
        {
            var doc = filter.DocumentNumber.Trim();
            posted = posted.Where(j => j.EntryNumber.Contains(doc));
        }

        var baseQuery = _context.JournalEntryLines.AsNoTracking()
            .Join(posted, l => l.JournalEntryId, j => j.Id, (l, j) => new { l, j });

        if (accountIds is { Count: > 0 })
            baseQuery = baseQuery.Where(x => accountIds.Contains(x.l.ChartOfAccountId));
        if (filter.CostCenterId.HasValue)
            baseQuery = baseQuery.Where(x => x.l.CostCenterId == filter.CostCenterId);
        if (!string.IsNullOrWhiteSpace(filter.Currency))
        {
            var currency = filter.Currency.Trim().ToUpperInvariant();
            baseQuery = baseQuery.Where(x => x.l.Currency == currency);
        }
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            baseQuery = baseQuery.Where(x =>
                x.j.EntryNumber.Contains(s)
                || x.j.Description.Contains(s)
                || (x.j.Reference != null && x.j.Reference.Contains(s))
                || (x.l.Description != null && x.l.Description.Contains(s)));
        }

        // Opening balance: before FromDate (same structural filters, account scoped when possible).
        decimal openingBalance = 0;
        if (filter.IncludeOpeningBalance && filter.FromDate.HasValue && accountIds is { Count: > 0 })
        {
            var openingQuery = baseQuery.Where(x => x.j.PostingDate < filter.FromDate.Value);
            var openingTotals = await openingQuery
                .GroupBy(_ => 1)
                .Select(g => new { Debit = g.Sum(x => x.l.Debit), Credit = g.Sum(x => x.l.Credit) })
                .FirstOrDefaultAsync(ct);
            openingBalance = (openingTotals?.Debit ?? 0) - (openingTotals?.Credit ?? 0);
        }

        var periodQuery = baseQuery;
        if (filter.FromDate.HasValue)
            periodQuery = periodQuery.Where(x => x.j.PostingDate >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            periodQuery = periodQuery.Where(x => x.j.PostingDate <= filter.ToDate.Value);

        var periodTotals = await periodQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Debit = g.Sum(x => x.l.Debit),
                Credit = g.Sum(x => x.l.Credit)
            })
            .FirstOrDefaultAsync(ct);

        var totalCount = periodTotals?.Count ?? 0;
        var totalDebit = periodTotals?.Debit ?? 0;
        var totalCredit = periodTotals?.Credit ?? 0;
        var closingBalance = openingBalance + totalDebit - totalCredit;

        decimal priorBalance = openingBalance;
        if (skip > 0)
        {
            var prior = await periodQuery
                .OrderBy(x => x.j.PostingDate)
                .ThenBy(x => x.j.EntryNumber)
                .ThenBy(x => x.l.LineNumber)
                .Take(skip)
                .GroupBy(_ => 1)
                .Select(g => new { Debit = g.Sum(x => x.l.Debit), Credit = g.Sum(x => x.l.Credit) })
                .FirstOrDefaultAsync(ct);
            priorBalance += (prior?.Debit ?? 0) - (prior?.Credit ?? 0);
        }

        var pageRows = await periodQuery
            .OrderBy(x => x.j.PostingDate)
            .ThenBy(x => x.j.EntryNumber)
            .ThenBy(x => x.l.LineNumber)
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new
            {
                JournalEntryId = x.j.Id,
                x.j.PostingDate,
                x.j.EntryNumber,
                Description = x.l.Description ?? x.j.Description,
                x.l.Debit,
                x.l.Credit,
                x.j.SourceModule,
                x.j.SourceDocumentId,
                x.j.Reference,
                x.l.CostCenterId,
                x.l.ChartOfAccountId
            })
            .ToListAsync(ct);

        var costCenterIds = pageRows.Where(r => r.CostCenterId.HasValue).Select(r => r.CostCenterId!.Value).Distinct().ToList();
        var accountIdsOnPage = pageRows.Select(r => r.ChartOfAccountId).Distinct().ToList();

        var costCenterNames = costCenterIds.Count == 0
            ? new Dictionary<Guid, (string NameAr, string? NameEn)>()
            : await _context.CostCenters.AsNoTracking()
                .Where(c => costCenterIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => (c.NameAr, c.NameEn), ct);

        var accountNames = accountIdsOnPage.Count == 0
            ? new Dictionary<Guid, (string Number, string NameAr)>()
            : await _context.ChartOfAccounts.AsNoTracking()
                .Where(a => accountIdsOnPage.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, a => (Number: a.AccountNumber, NameAr: a.NameAr), ct);

        var lines = new List<GeneralLedgerLineDto>(pageRows.Count + (page == 1 && filter.IncludeOpeningBalance && filter.FromDate.HasValue ? 1 : 0));

        if (page == 1 && filter.IncludeOpeningBalance && filter.FromDate.HasValue && accountIds is { Count: > 0 })
        {
            lines.Add(new GeneralLedgerLineDto(
                filter.FromDate.Value,
                string.Empty,
                "Opening Balance",
                0,
                0,
                openingBalance,
                IsOpeningBalance: true));
        }

        var running = priorBalance;
        foreach (var r in pageRows)
        {
            running += r.Debit - r.Credit;
            costCenterNames.TryGetValue(r.CostCenterId ?? Guid.Empty, out var cc);
            accountNames.TryGetValue(r.ChartOfAccountId, out var acc);
            lines.Add(new GeneralLedgerLineDto(
                r.PostingDate,
                r.EntryNumber,
                r.Description,
                r.Debit,
                r.Credit,
                running,
                r.JournalEntryId,
                r.SourceModule,
                r.SourceDocumentId,
                r.Reference,
                r.CostCenterId,
                r.CostCenterId.HasValue ? cc.NameAr : null,
                r.CostCenterId.HasValue ? cc.NameEn : null,
                r.ChartOfAccountId,
                acc.Number,
                acc.NameAr));
        }

        return new GeneralLedgerResultDto(
            openingBalance,
            totalDebit,
            totalCredit,
            closingBalance,
            totalCount,
            page,
            pageSize,
            lines);
    }

    private async Task<List<Guid>> ResolveAccountIdsAsync(Guid tenantId, GeneralLedgerFilterDto filter, CancellationToken ct)
    {
        var q = _context.ChartOfAccounts.AsNoTracking().Where(a => a.TenantId == tenantId);
        if (filter.AccountType.HasValue)
            q = q.Where(a => a.AccountType == filter.AccountType.Value);
        if (filter.ParentAccountId.HasValue)
        {
            var parentId = filter.ParentAccountId.Value;
            q = q.Where(a => a.Id == parentId || a.ParentAccountId == parentId);
        }

        return await q.Select(a => a.Id).ToListAsync(ct);
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
