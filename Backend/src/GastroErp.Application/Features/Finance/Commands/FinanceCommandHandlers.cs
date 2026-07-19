using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Mapping;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Commands;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<AccountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateAccountCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<AccountDto>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.ChartOfAccounts
            .AnyAsync(a => a.TenantId == request.TenantId && a.AccountNumber == request.Dto.AccountNumber.Trim(), cancellationToken);
        if (exists) return Result<AccountDto>.Failure(ErrorCodes.AccountNumberDuplicate, "Account number already exists.");

        if (request.Dto.ParentAccountId is Guid parentId)
        {
            var parent = await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.Id == parentId, cancellationToken);
            if (parent is null) return Result<AccountDto>.Failure(ErrorCodes.AccountInvalidParent, "Parent account not found.");
            if (parent.AccountType != request.Dto.AccountType)
                return Result<AccountDto>.Failure(ErrorCodes.AccountInvalidParent, "Parent account type must match.");
        }

        var account = ChartOfAccount.Create(
            request.TenantId, request.Dto.AccountNumber, request.Dto.NameAr, request.Dto.AccountType,
            request.Dto.AccountCategory, !request.Dto.IsSummaryAccount, request.Dto.IsSummaryAccount,
            request.Dto.ParentAccountId, request.Dto.NameEn, request.Dto.Currency, request.Dto.SortOrder,
            request.Dto.Notes, request.Dto.IsSystemAccount, request.Dto.AccountClassificationId);

        _context.ChartOfAccounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<AccountDto>.Success(_mapper.Map<AccountDto>(account));
    }
}

public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateAccountCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (account is null) return Result.Failure(ErrorCodes.AccountNotFound, "Account not found.");

        var hasTx = await AccountUsageGuard.HasTransactionsAsync(_context, account.Id, cancellationToken);
        try
        {
            account.Update(
                request.Dto.NameAr,
                request.Dto.NameEn,
                request.Dto.AccountCategory,
                request.Dto.IsSummaryAccount,
                request.Dto.SortOrder,
                request.Dto.Currency,
                request.Dto.Notes,
                allowCategoryChange: !hasTx && !account.IsSystemAccount,
                accountClassificationId: request.Dto.AccountClassificationId);
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }

        _context.ChartOfAccounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ReparentAccountCommandHandler : IRequestHandler<ReparentAccountCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public ReparentAccountCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ReparentAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (account is null) return Result.Failure(ErrorCodes.AccountNotFound, "Account not found.");

        if (request.Dto.NewParentAccountId is Guid parentId)
        {
            if (parentId == account.Id)
                return Result.Failure(ErrorCodes.AccountInvalidParent, "Account cannot be its own parent.");

            var parent = await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.Id == parentId, cancellationToken);
            if (parent is null) return Result.Failure(ErrorCodes.AccountInvalidParent, "Parent account not found.");
            if (parent.TenantId != account.TenantId)
                return Result.Failure(ErrorCodes.AccountInvalidParent, "Parent must belong to the same tenant.");
            if (parent.AccountType != account.AccountType)
                return Result.Failure(ErrorCodes.AccountInvalidParent, "Parent account type must match.");

            // Prevent cycles: walk up from new parent
            var cursor = parent;
            while (cursor.ParentAccountId is Guid up)
            {
                if (up == account.Id)
                    return Result.Failure(ErrorCodes.AccountInvalidParent, "Reparent would create a cycle.");
                cursor = await _context.ChartOfAccounts.FirstAsync(a => a.Id == up, cancellationToken);
            }
        }

        try { account.Reparent(request.Dto.NewParentAccountId); }
        catch (Domain.Common.Exceptions.BusinessException ex)
        { return Result.Failure(ex.ErrorCode, ex.Message); }

        var siblings = await _context.ChartOfAccounts
            .Where(a => a.TenantId == account.TenantId && a.ParentAccountId == account.ParentAccountId && a.Id != account.Id)
            .Select(a => a.SortOrder)
            .ToListAsync(cancellationToken);
        account.SetSortOrder((siblings.Count == 0 ? 0 : siblings.Max()) + 1);

        _context.ChartOfAccounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class RenumberAccountCommandHandler : IRequestHandler<RenumberAccountCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public RenumberAccountCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RenumberAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (account is null) return Result.Failure(ErrorCodes.AccountNotFound, "Account not found.");

        var newNumber = request.Dto.NewAccountNumber.Trim();
        var dup = await _context.ChartOfAccounts.AnyAsync(
            a => a.TenantId == account.TenantId && a.AccountNumber == newNumber && a.Id != account.Id, cancellationToken);
        if (dup) return Result.Failure(ErrorCodes.AccountNumberDuplicate, "Account number already exists.");

        try { account.Renumber(newNumber); }
        catch (Domain.Common.Exceptions.BusinessException ex)
        { return Result.Failure(ex.ErrorCode, ex.Message); }

        _context.ChartOfAccounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ActivateAccountCommandHandler : IRequestHandler<ActivateAccountCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public ActivateAccountCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (account is null) return Result.Failure(ErrorCodes.AccountNotFound, "Account not found.");
        account.Activate();
        _context.ChartOfAccounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeactivateAccountCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (account is null) return Result.Failure(ErrorCodes.AccountNotFound, "Account not found.");
        account.Deactivate();
        _context.ChartOfAccounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeleteAccountCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (account is null) return Result.Failure(ErrorCodes.AccountNotFound, "Account not found.");

        try { account.EnsureCanDelete(); }
        catch (Domain.Common.Exceptions.BusinessException ex)
        { return Result.Failure(ex.ErrorCode, "System accounts cannot be deleted. Deactivate instead."); }

        if (await _context.ChartOfAccounts.AnyAsync(a => a.ParentAccountId == account.Id, cancellationToken))
            return Result.Failure(ErrorCodes.AccountHasChildren, "Account has child accounts and cannot be deleted.");

        if (await AccountUsageGuard.HasTransactionsAsync(_context, account.Id, cancellationToken))
            return Result.Failure(ErrorCodes.AccountHasTransactions, "Account has journal entries or financial movements and cannot be deleted.");

        if (await _context.OpeningBalances.AnyAsync(o => o.ContraAccountId == account.Id, cancellationToken))
            return Result.Failure(ErrorCodes.AccountHasTransactions, "Account is used as opening-balance contra and cannot be deleted.");

        if (await _context.IssueDestinations.AnyAsync(d => d.DefaultGlAccountId == account.Id, cancellationToken))
            return Result.Failure(ErrorCodes.AccountHasTransactions, "Account is linked to an issue destination and cannot be deleted.");

        account.SoftDelete(null);
        _context.ChartOfAccounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal static class AccountUsageGuard
{
    public static async Task<bool> HasTransactionsAsync(IApplicationDbContext context, Guid accountId, CancellationToken ct)
        => await context.JournalEntryLines.AnyAsync(l => l.ChartOfAccountId == accountId, ct);
}

public class ImportAccountsCommandHandler : IRequestHandler<ImportAccountsCommand, Result<AccountImportPreviewDto>>
{
    private readonly IApplicationDbContext _context;

    public ImportAccountsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<AccountImportPreviewDto>> Handle(ImportAccountsCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var valid = new List<AccountImportRowDto>();
        var existing = await _context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.TenantId == request.TenantId)
            .Select(a => new { a.AccountNumber, a.Id, a.AccountType })
            .ToListAsync(cancellationToken);
        var existingNumbers = existing.Select(a => a.AccountNumber).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var importNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (row, index) in request.Rows.Select((r, i) => (r, i + 1)))
        {
            if (string.IsNullOrWhiteSpace(row.AccountNumber) || string.IsNullOrWhiteSpace(row.NameAr))
            {
                errors.Add($"Row {index}: Account number and Arabic name are required.");
                continue;
            }

            var number = row.AccountNumber.Trim();
            if (!importNumbers.Add(number))
            {
                errors.Add($"Row {index}: Duplicate account number '{number}' in file.");
                continue;
            }

            if (existingNumbers.Contains(number))
            {
                errors.Add($"Row {index}: Account '{number}' already exists.");
                continue;
            }

            if (!string.IsNullOrWhiteSpace(row.ParentAccountNumber))
            {
                var parentNum = row.ParentAccountNumber.Trim();
                var parentExists = existingNumbers.Contains(parentNum) || importNumbers.Contains(parentNum);
                if (!parentExists)
                {
                    errors.Add($"Row {index}: Parent '{parentNum}' not found.");
                    continue;
                }

                var parentType = existing.FirstOrDefault(a => a.AccountNumber.Equals(parentNum, StringComparison.OrdinalIgnoreCase))?.AccountType;
                if (parentType is null && request.Rows.FirstOrDefault(r => r.AccountNumber.Equals(parentNum, StringComparison.OrdinalIgnoreCase)) is { } parentRow)
                    parentType = parentRow.AccountType;

                if (parentType is not null && parentType != row.AccountType)
                {
                    errors.Add($"Row {index}: Parent type mismatch for '{number}'.");
                    continue;
                }
            }

            valid.Add(row with
            {
                AccountNumber = number,
                NameAr = row.NameAr.Trim(),
                NameEn = string.IsNullOrWhiteSpace(row.NameEn) ? null : row.NameEn.Trim(),
                ParentAccountNumber = string.IsNullOrWhiteSpace(row.ParentAccountNumber) ? null : row.ParentAccountNumber.Trim(),
                Currency = string.IsNullOrWhiteSpace(row.Currency) ? "SAR" : row.Currency.Trim().ToUpperInvariant()
            });
        }

        var preview = new AccountImportPreviewDto(request.Rows.Count, valid.Count, errors.Count, errors, valid);
        if (!request.Commit || errors.Count > 0)
            return Result<AccountImportPreviewDto>.Success(preview);

        // Insert roots first then children by repeated passes
        var pending = valid.ToList();
        var created = existing.ToDictionary(a => a.AccountNumber, a => a.Id, StringComparer.OrdinalIgnoreCase);
        var guard = 0;
        while (pending.Count > 0 && guard++ < 20)
        {
            var progressed = false;
            foreach (var row in pending.ToList())
            {
                Guid? parentId = null;
                if (row.ParentAccountNumber is not null)
                {
                    if (!created.TryGetValue(row.ParentAccountNumber, out var pid))
                        continue;
                    parentId = pid;
                }

                var account = ChartOfAccount.Create(
                    request.TenantId, row.AccountNumber, row.NameAr, row.AccountType, row.AccountCategory,
                    !row.IsSummaryAccount, row.IsSummaryAccount, parentId, row.NameEn, row.Currency, row.SortOrder, row.Notes);
                _context.ChartOfAccounts.Add(account);
                created[row.AccountNumber] = account.Id;
                pending.Remove(row);
                progressed = true;
            }

            if (!progressed) break;
        }

        if (pending.Count > 0)
            return Result<AccountImportPreviewDto>.Failure(ErrorCodes.AccountImportInvalid, "Could not resolve parent hierarchy for some rows.");

        await _context.SaveChangesAsync(cancellationToken);
        return Result<AccountImportPreviewDto>.Success(preview with { ValidRows = valid.Count - pending.Count });
    }
}

public class UpsertAccountingSettingsCommandHandler : IRequestHandler<UpsertAccountingSettingsCommand, Result<AccountingSettingsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpsertAccountingSettingsCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<AccountingSettingsDto>> Handle(UpsertAccountingSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _context.AccountingSettings
            .FirstOrDefaultAsync(s => s.TenantId == request.TenantId && s.CompanyId == null, cancellationToken);

        if (settings is null)
        {
            settings = AccountingSettings.CreateDefault(request.TenantId);
            _context.AccountingSettings.Add(settings);
        }

        var d = request.Dto;
        settings.UpdateNumbering(d.AccountNumberMaxLength, d.MaxTreeLevels, d.LevelLengthsCsv, d.LevelSeparator);
        settings.UpdateAccountMappings(
            d.CashAccountId, d.BankAccountId, d.InventoryAccountId, d.CogsAccountId, d.SalesRevenueAccountId, d.PurchaseAccountId,
            d.AccountsReceivableAccountId, d.AccountsPayableAccountId, d.VatInputAccountId, d.VatOutputAccountId,
            d.DiscountAccountId, d.RoundOffAccountId, d.OpeningBalanceAccountId, d.RetainedEarningsAccountId,
            d.PayrollExpenseAccountId, d.PayrollLiabilityAccountId, d.ProductionVarianceAccountId, d.InventoryAdjustmentAccountId,
            d.WasteAccountId, d.DeliveryRevenueAccountId, d.DeliveryExpenseAccountId, d.KitchenConsumptionAccountId,
            d.CustomerAdvancesAccountId, d.SupplierAdvancesAccountId, d.ExchangeDifferenceAccountId,
            d.GrniAccountId, d.FixedAssetAccountId);
        settings.UpdatePostingFlags(
            d.AutoPostSales, d.AutoPostPurchases, d.AutoPostGoodsReceipt, d.AutoPostGoodsIssue,
            d.AutoPostStockTransfer, d.AutoPostWaste, d.AutoPostProduction, d.AutoPostPayroll);

        await _context.SaveChangesAsync(cancellationToken);
        return Result<AccountingSettingsDto>.Success(_mapper.Map<AccountingSettingsDto>(settings));
    }
}

public class CreateJournalCommandHandler : IRequestHandler<CreateJournalCommand, Result<JournalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJournalPostingService _postingService;

    public CreateJournalCommandHandler(
        IApplicationDbContext context, IMapper mapper, IJournalPostingService postingService)
        => (_context, _mapper, _postingService) = (context, mapper, postingService);

    public async Task<Result<JournalDto>> Handle(CreateJournalCommand request, CancellationToken cancellationToken)
    {
        var result = await _postingService.CreateDraftAsync(request.TenantId, request.Dto, cancellationToken);
        if (!result.IsSuccess) return Result<JournalDto>.Failure(result.ErrorCode!, result.ErrorMessage!);
        await _context.SaveChangesAsync(cancellationToken);
        var journal = await _context.JournalEntries.AsNoTracking()
            .Include(j => j.Lines)
            .FirstAsync(j => j.Id == result.Data!.Id, cancellationToken);
        return Result<JournalDto>.Success(_mapper.Map<JournalDto>(journal));
    }
}

public class UpdateJournalCommandHandler : IRequestHandler<UpdateJournalCommand, Result<JournalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFinancialValidationService _validationService;
    private readonly IFiscalPeriodService _fiscalPeriodService;

    public UpdateJournalCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IFinancialValidationService validationService,
        IFiscalPeriodService fiscalPeriodService)
        => (_context, _mapper, _validationService, _fiscalPeriodService)
            = (context, mapper, validationService, fiscalPeriodService);

    public async Task<Result<JournalDto>> Handle(UpdateJournalCommand request, CancellationToken cancellationToken)
    {
        var journal = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);
        if (journal is null)
            return Result<JournalDto>.Failure(ErrorCodes.JournalNotFound, "Journal not found.");

        var lines = request.Dto.Lines ?? Array.Empty<JournalLineDto>();
        if (lines.Count == 0)
            return Result<JournalDto>.Failure(ErrorCodes.JournalHasNoLines, "Journal must have lines.");

        var validation = await _validationService.ValidateJournalLinesAsync(
            journal.TenantId, lines, cancellationToken, requireBalance: false);
        if (!validation.IsSuccess)
            return Result<JournalDto>.Failure(validation.ErrorCode!, validation.ErrorMessage!);

        var period = request.Dto.FiscalPeriodId.HasValue
            ? await _context.FiscalPeriods.FirstOrDefaultAsync(
                p => p.Id == request.Dto.FiscalPeriodId.Value && p.TenantId == journal.TenantId, cancellationToken)
            : await _fiscalPeriodService.GetOrEnsureOpenPeriodAsync(
                journal.TenantId, request.Dto.PostingDate, cancellationToken);
        if (period is null)
            return Result<JournalDto>.Failure(ErrorCodes.FiscalPeriodNotFound, "Fiscal period not found.");
        var periodCheck = await _fiscalPeriodService.ValidatePeriodAcceptsPostingsAsync(period.Id, cancellationToken);

        if (!periodCheck.IsSuccess)
            return Result<JournalDto>.Failure(periodCheck.ErrorCode!, periodCheck.ErrorMessage!);

        try
        {
            journal.UpdateDraft(
                request.Dto.PostingDate, period.Id, request.Dto.Description,
                request.Dto.CompanyId, request.Dto.BranchId, request.Dto.Reference,
                request.Dto.VoucherType);

            var existing = journal.Lines.ToList();
            if (existing.Count > 0)
                _context.JournalEntryLines.RemoveRange(existing);
            journal.ClearLines();

            if (journal.SourceModule == PostingSource.Manual && lines.Count < 2)
                return Result<JournalDto>.Failure(ErrorCodes.JournalMinTwoLines, "Manual journal requires at least two lines.");

            var lineNum = 1;
            foreach (var line in lines)
                journal.AddLine(
                    line.ChartOfAccountId, line.Debit, line.Credit,
                    string.IsNullOrWhiteSpace(line.Currency) ? "SAR" : line.Currency,
                    lineNum++, line.CostCenterId, line.Description,
                    line.ExchangeRate <= 0 ? 1m : line.ExchangeRate, line.AnalyticalAccountId);

            await _context.SaveChangesAsync(cancellationToken);
            var refreshed = await _context.JournalEntries.AsNoTracking()
                .Include(j => j.Lines)
                .FirstAsync(j => j.Id == journal.Id, cancellationToken);
            return Result<JournalDto>.Success(_mapper.Map<JournalDto>(refreshed));
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result<JournalDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public class DeleteJournalCommandHandler : IRequestHandler<DeleteJournalCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteJournalCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteJournalCommand request, CancellationToken cancellationToken)
    {
        var journal = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);
        if (journal is null)
            return Result.Failure(ErrorCodes.JournalNotFound, "Journal not found.");

        try
        {
            journal.EnsureCanDelete();
            journal.SoftDelete(null);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public class ApproveJournalCommandHandler : IRequestHandler<ApproveJournalCommand, Result<JournalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ApproveJournalCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<JournalDto>> Handle(ApproveJournalCommand request, CancellationToken cancellationToken)
    {
        var journal = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);
        if (journal is null)
            return Result<JournalDto>.Failure(ErrorCodes.JournalNotFound, "Journal not found.");

        try
        {
            journal.Approve();
            await _context.SaveChangesAsync(cancellationToken);
            var refreshed = await _context.JournalEntries.AsNoTracking()
                .Include(j => j.Lines)
                .FirstAsync(j => j.Id == journal.Id, cancellationToken);
            return Result<JournalDto>.Success(_mapper.Map<JournalDto>(refreshed));
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result<JournalDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public class PostJournalCommandHandler : IRequestHandler<PostJournalCommand, Result<JournalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJournalPostingService _postingService;

    public PostJournalCommandHandler(
        IApplicationDbContext context, IMapper mapper, IJournalPostingService postingService)
        => (_context, _mapper, _postingService) = (context, mapper, postingService);

    public async Task<Result<JournalDto>> Handle(PostJournalCommand request, CancellationToken cancellationToken)
    {
        var result = await _postingService.PostExistingAsync(request.Id, request.UserId, cancellationToken);
        if (!result.IsSuccess) return Result<JournalDto>.Failure(result.ErrorCode!, result.ErrorMessage!);
        await _context.SaveChangesAsync(cancellationToken);
        var journal = await _context.JournalEntries.AsNoTracking()
            .Include(j => j.Lines)
            .FirstAsync(j => j.Id == result.Data!.Id, cancellationToken);
        return Result<JournalDto>.Success(_mapper.Map<JournalDto>(journal));
    }
}

public class ReverseJournalCommandHandler : IRequestHandler<ReverseJournalCommand, Result<JournalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJournalPostingService _postingService;

    public ReverseJournalCommandHandler(
        IApplicationDbContext context, IMapper mapper, IJournalPostingService postingService)
        => (_context, _mapper, _postingService) = (context, mapper, postingService);

    public async Task<Result<JournalDto>> Handle(ReverseJournalCommand request, CancellationToken cancellationToken)
    {
        var result = await _postingService.ReverseAsync(request.Id, request.UserId, cancellationToken);
        if (!result.IsSuccess) return Result<JournalDto>.Failure(result.ErrorCode!, result.ErrorMessage!);
        await _context.SaveChangesAsync(cancellationToken);
        var journal = await _context.JournalEntries.AsNoTracking()
            .Include(j => j.Lines)
            .FirstAsync(j => j.Id == result.Data!.Id, cancellationToken);
        return Result<JournalDto>.Success(_mapper.Map<JournalDto>(journal));
    }
}

public class CreateFiscalPeriodCommandHandler : IRequestHandler<CreateFiscalPeriodCommand, Result<FiscalPeriodDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateFiscalPeriodCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<FiscalPeriodDto>> Handle(CreateFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.FiscalPeriods
            .AnyAsync(p => p.TenantId == request.TenantId && p.FiscalYear == request.Dto.FiscalYear, cancellationToken);
        if (exists) return Result<FiscalPeriodDto>.Failure(ErrorCodes.InvalidFiscalPeriodDates, "Fiscal year already exists.");

        try
        {
            var period = FiscalPeriod.Create(
                request.TenantId,
                request.Dto.FiscalYear,
                request.Dto.StartMonth,
                request.Dto.Notes,
                request.Dto.PeriodPolicy);

            if (request.Dto.GenerateDetails)
                period.GenerateMonthlyDetails();

            _context.FiscalPeriods.Add(period);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<FiscalPeriodDto>.Success(FiscalPeriodMapper.ToDto(period));
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result<FiscalPeriodDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public class UpdateFiscalPeriodCommandHandler : IRequestHandler<UpdateFiscalPeriodCommand, Result<FiscalPeriodDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateFiscalPeriodCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<FiscalPeriodDto>> Handle(UpdateFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _context.FiscalPeriods
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (period is null)
            return Result<FiscalPeriodDto>.Failure(ErrorCodes.FiscalPeriodNotFound, "Fiscal period not found.");

        try
        {
            period.Update(request.Dto.StartMonth, request.Dto.Notes);

            if (request.Dto.Details is { Count: > 0 })
            {
                foreach (var d in request.Dto.Details)
                {
                    var detail = period.Details.FirstOrDefault(x => x.Id == d.Id)
                                 ?? period.Details.FirstOrDefault(x => x.PeriodNumber == d.PeriodNumber);
                    if (detail is null) continue;
                    detail.Rename(d.NameAr, d.NameEn);
                    detail.SetStatus(d.Status);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<FiscalPeriodDto>.Success(FiscalPeriodMapper.ToDto(period));
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result<FiscalPeriodDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public class DeleteFiscalPeriodCommandHandler : IRequestHandler<DeleteFiscalPeriodCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteFiscalPeriodCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _context.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (period is null)
            return Result.Failure(ErrorCodes.FiscalPeriodNotFound, "Fiscal period not found.");

        if (period.Status == Domain.Enums.FiscalPeriodStatus.Locked)
            return Result.Failure(ErrorCodes.FiscalPeriodLocked, "Cannot delete a locked fiscal period.");

        period.SoftDelete(null);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class GenerateFiscalPeriodDetailsCommandHandler : IRequestHandler<GenerateFiscalPeriodDetailsCommand, Result<FiscalPeriodDto>>
{
    private readonly IApplicationDbContext _context;

    public GenerateFiscalPeriodDetailsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<FiscalPeriodDto>> Handle(
        GenerateFiscalPeriodDetailsCommand request,
        CancellationToken cancellationToken)
    {
        var period = await _context.FiscalPeriods
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (period is null)
            return Result<FiscalPeriodDto>.Failure(ErrorCodes.FiscalPeriodNotFound, "Fiscal period not found.");

        try
        {
            period.GenerateMonthlyDetails();
            await _context.SaveChangesAsync(cancellationToken);
            return Result<FiscalPeriodDto>.Success(FiscalPeriodMapper.ToDto(period));
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result<FiscalPeriodDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public class CloseFiscalPeriodCommandHandler : IRequestHandler<CloseFiscalPeriodCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public CloseFiscalPeriodCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(CloseFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _context.FiscalPeriods
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (period is null) return Result.Failure(ErrorCodes.FiscalPeriodNotFound, "Fiscal period not found.");
        try { period.Close(); }
        catch (Domain.Common.Exceptions.BusinessException ex) { return Result.Failure(ex.ErrorCode, ex.Message); }
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class LockFiscalPeriodCommandHandler : IRequestHandler<LockFiscalPeriodCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public LockFiscalPeriodCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(LockFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _context.FiscalPeriods
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (period is null) return Result.Failure(ErrorCodes.FiscalPeriodNotFound, "Fiscal period not found.");
        try { period.Lock(); }
        catch (Domain.Common.Exceptions.BusinessException ex) { return Result.Failure(ex.ErrorCode, ex.Message); }
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ReopenFiscalPeriodCommandHandler : IRequestHandler<ReopenFiscalPeriodCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public ReopenFiscalPeriodCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ReopenFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _context.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (period is null) return Result.Failure(ErrorCodes.FiscalPeriodNotFound, "Fiscal period not found.");
        try { period.Reopen(); }
        catch (Domain.Common.Exceptions.BusinessException ex) { return Result.Failure(ex.ErrorCode, ex.Message); }
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
