using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Mapping;
using GastroErp.Application.Features.Finance.Queries;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Queries;

public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, PagedResult<AccountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAccountsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<AccountDto>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.ChartOfAccounts.AsNoTracking().Where(a => a.TenantId == request.TenantId);
        if (filter.AccountType.HasValue) query = query.Where(a => a.AccountType == filter.AccountType);
        if (filter.IsActive.HasValue) query = query.Where(a => a.IsActive == filter.IsActive);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            query = query.Where(a => a.AccountNumber.Contains(s) || a.NameAr.Contains(s));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);
        var items = await query.OrderBy(a => a.AccountNumber)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<AccountDto>.Success(_mapper.Map<IReadOnlyList<AccountDto>>(items), page, pageSize, total);
    }
}

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, Result<AccountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAccountByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<AccountDto>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _context.ChartOfAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        return account is null
            ? Result<AccountDto>.Failure(ErrorCodes.AccountNotFound, "Account not found.")
            : Result<AccountDto>.Success(_mapper.Map<AccountDto>(account));
    }
}

public class GetAccountTreeQueryHandler : IRequestHandler<GetAccountTreeQuery, Result<IReadOnlyList<AccountTreeNodeDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAccountTreeQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<IReadOnlyList<AccountTreeNodeDto>>> Handle(GetAccountTreeQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.TenantId == request.TenantId);

        if (!request.IncludeInactive)
            query = query.Where(a => a.IsActive);
        if (request.AccountType.HasValue)
            query = query.Where(a => a.AccountType == request.AccountType);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLower();
            query = query.Where(a =>
                a.AccountNumber.ToLower().Contains(s) ||
                a.NameAr.ToLower().Contains(s) ||
                (a.NameEn != null && a.NameEn.ToLower().Contains(s)));
        }

        var accounts = await query
            .OrderBy(a => a.SortOrder).ThenBy(a => a.AccountNumber)
            .Select(a => new
            {
                a.Id, a.AccountNumber, a.NameAr, a.NameEn, a.ParentAccountId,
                a.AccountType, a.AccountCategory, a.Currency,
                a.IsPostingAllowed, a.IsSummaryAccount, a.IsSystemAccount,
                a.IsActive, a.SortOrder, a.Notes, a.AccountClassificationId
            })
            .ToListAsync(cancellationToken);

        // When searching, flatten matching nodes with ancestors so the tree remains navigable.
        HashSet<Guid>? keep = null;
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            keep = accounts.Select(a => a.Id).ToHashSet();
            var byId = accounts.ToDictionary(a => a.Id);
            foreach (var id in keep.ToList())
            {
                var cursor = byId[id];
                while (cursor.ParentAccountId is Guid pid && byId.TryGetValue(pid, out var parent))
                {
                    keep.Add(parent.Id);
                    cursor = parent;
                }
            }
            accounts = accounts.Where(a => keep.Contains(a.Id)).ToList();
        }

        List<AccountTreeNodeDto> BuildChildren(Guid? parentId) =>
            accounts.Where(a => a.ParentAccountId == parentId)
                .Select(a => new AccountTreeNodeDto(
                    a.Id, a.AccountNumber, a.NameAr, a.NameEn, a.ParentAccountId,
                    a.AccountType, a.AccountCategory, a.Currency,
                    a.IsPostingAllowed, a.IsSummaryAccount, a.IsSystemAccount,
                    a.IsActive, a.SortOrder, a.Notes, a.AccountClassificationId, BuildChildren(a.Id)))
                .ToList();

        // Roots are nodes whose parent is null or not in the filtered set
        var roots = accounts
            .Where(a => a.ParentAccountId is null || !accounts.Any(p => p.Id == a.ParentAccountId))
            .Select(a => new AccountTreeNodeDto(
                a.Id, a.AccountNumber, a.NameAr, a.NameEn, a.ParentAccountId,
                a.AccountType, a.AccountCategory, a.Currency,
                a.IsPostingAllowed, a.IsSummaryAccount, a.IsSystemAccount,
                a.IsActive, a.SortOrder, a.Notes, a.AccountClassificationId, BuildChildren(a.Id)))
            .ToList();

        return Result<IReadOnlyList<AccountTreeNodeDto>>.Success(roots);
    }
}

public class ExportAccountsQueryHandler : IRequestHandler<ExportAccountsQuery, Result<IReadOnlyList<AccountExportRowDto>>>
{
    private readonly IApplicationDbContext _context;
    public ExportAccountsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<IReadOnlyList<AccountExportRowDto>>> Handle(ExportAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.TenantId == request.TenantId)
            .OrderBy(a => a.AccountNumber)
            .ToListAsync(cancellationToken);

        var byId = accounts.ToDictionary(a => a.Id);
        int LevelOf(ChartOfAccount a)
        {
            var level = 1;
            var cursor = a;
            while (cursor.ParentAccountId is Guid pid && byId.TryGetValue(pid, out var parent))
            {
                level++;
                cursor = parent;
            }
            return level;
        }

        var rows = accounts.Select(a => new AccountExportRowDto(
            a.AccountNumber, a.NameAr, a.NameEn,
            a.ParentAccountId is Guid pid && byId.TryGetValue(pid, out var p) ? p.AccountNumber : null,
            LevelOf(a), a.AccountType, a.AccountCategory, a.Currency,
            a.IsSummaryAccount, a.IsSystemAccount, a.IsActive, a.SortOrder, a.Notes)).ToList();

        return Result<IReadOnlyList<AccountExportRowDto>>.Success(rows);
    }
}

public class GetAccountingSettingsQueryHandler : IRequestHandler<GetAccountingSettingsQuery, Result<AccountingSettingsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAccountingSettingsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<AccountingSettingsDto>> Handle(GetAccountingSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _context.AccountingSettings
            .FirstOrDefaultAsync(s => s.TenantId == request.TenantId && s.CompanyId == null, cancellationToken);

        if (settings is null)
        {
            settings = AccountingSettings.CreateDefault(request.TenantId);
            _context.AccountingSettings.Add(settings);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<AccountingSettingsDto>.Success(_mapper.Map<AccountingSettingsDto>(settings));
    }
}

public class GetJournalsQueryHandler : IRequestHandler<GetJournalsQuery, PagedResult<JournalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetJournalsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<JournalDto>> Handle(GetJournalsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.JournalEntries.AsNoTracking()
            .Include(j => j.Lines)
            .Where(j => j.TenantId == request.TenantId);

        if (filter.Status.HasValue) query = query.Where(j => j.Status == filter.Status);
        if (filter.SourceModule.HasValue) query = query.Where(j => j.SourceModule == filter.SourceModule);
        if (filter.VoucherType.HasValue) query = query.Where(j => j.VoucherType == filter.VoucherType);
        if (filter.CompanyId.HasValue) query = query.Where(j => j.CompanyId == filter.CompanyId);
        if (filter.BranchId.HasValue) query = query.Where(j => j.BranchId == filter.BranchId);
        if (filter.FiscalPeriodId.HasValue) query = query.Where(j => j.FiscalPeriodId == filter.FiscalPeriodId);
        if (filter.FiscalYear.HasValue)
        {
            var year = filter.FiscalYear.Value;
            var periodIds = _context.FiscalPeriods.AsNoTracking()
                .Where(p => p.TenantId == request.TenantId && p.FiscalYear == year)
                .Select(p => p.Id);
            query = query.Where(j => periodIds.Contains(j.FiscalPeriodId));
        }
        if (filter.FromDate.HasValue) query = query.Where(j => j.PostingDate >= filter.FromDate);
        if (filter.ToDate.HasValue) query = query.Where(j => j.PostingDate <= filter.ToDate);
        if (!string.IsNullOrWhiteSpace(filter.EntryNumber))
        {
            var num = filter.EntryNumber.Trim();
            query = query.Where(j => j.EntryNumber.Contains(num));
        }
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(j =>
                j.EntryNumber.ToLower().Contains(s) ||
                j.Description.ToLower().Contains(s) ||
                (j.Reference != null && j.Reference.ToLower().Contains(s)));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);
        var page = Math.Max(filter.Page, 1);
        var items = await query.OrderByDescending(j => j.PostingDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<JournalDto>.Success(_mapper.Map<IReadOnlyList<JournalDto>>(items), page, pageSize, total);
    }
}

public class GetJournalByIdQueryHandler : IRequestHandler<GetJournalByIdQuery, Result<JournalDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetJournalByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<JournalDetailDto>> Handle(GetJournalByIdQuery request, CancellationToken cancellationToken)
    {
        var journal = await _context.JournalEntries.AsNoTracking()
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);
        if (journal is null) return Result<JournalDetailDto>.Failure(ErrorCodes.JournalNotFound, "Journal not found.");

        var accountIds = journal.Lines.Select(l => l.ChartOfAccountId).Distinct().ToList();
        var accounts = await _context.ChartOfAccounts.AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        var lines = journal.Lines.OrderBy(l => l.LineNumber).Select(l =>
        {
            accounts.TryGetValue(l.ChartOfAccountId, out var acct);
            return new JournalLineDetailDto(
                l.Id, l.LineNumber, l.ChartOfAccountId,
                acct?.AccountNumber ?? "", acct?.NameAr ?? "",
                l.CostCenterId, l.Debit, l.Credit, l.Currency, l.Description,
                l.ExchangeRate, l.AnalyticalAccountId);
        }).ToList();

        return Result<JournalDetailDto>.Success(new JournalDetailDto(
            journal.Id, journal.EntryNumber, journal.PostingDate, journal.FiscalPeriodId,
            journal.Description, journal.Reference, journal.SourceModule, journal.SourceDocumentId,
            journal.Status, journal.PostedAt, journal.CompanyId, journal.BranchId, lines,
            journal.VoucherType));
    }
}

public class GetFiscalPeriodsQueryHandler : IRequestHandler<GetFiscalPeriodsQuery, Result<IReadOnlyList<FiscalPeriodDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetFiscalPeriodsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<IReadOnlyList<FiscalPeriodDto>>> Handle(GetFiscalPeriodsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FiscalPeriods.AsNoTracking()
            .Include(p => p.Details)
            .Where(p => p.TenantId == request.TenantId);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var s = request.SearchTerm.Trim();
            query = query.Where(p =>
                p.Name.Contains(s) ||
                p.FiscalYear.ToString().Contains(s) ||
                (p.Notes != null && p.Notes.Contains(s)));
        }

        var periods = await query
            .OrderByDescending(p => p.FiscalYear)
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<FiscalPeriodDto>>.Success(periods.Select(FiscalPeriodMapper.ToDto).ToList());
    }
}

public class GetFiscalPeriodByIdQueryHandler : IRequestHandler<GetFiscalPeriodByIdQuery, Result<FiscalPeriodDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFiscalPeriodByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<FiscalPeriodDto>> Handle(GetFiscalPeriodByIdQuery request, CancellationToken cancellationToken)
    {
        var period = await _context.FiscalPeriods.AsNoTracking()
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        return period is null
            ? Result<FiscalPeriodDto>.Failure(ErrorCodes.FiscalPeriodNotFound, "Fiscal period not found.")
            : Result<FiscalPeriodDto>.Success(FiscalPeriodMapper.ToDto(period));
    }
}

public class GetCostCentersQueryHandler : IRequestHandler<GetCostCentersQuery, PagedResult<CostCenterDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCostCentersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PagedResult<CostCenterDto>> Handle(GetCostCentersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.CostCenters.AsNoTracking().Where(c => c.TenantId == request.TenantId);
        if (filter.BranchId.HasValue) query = query.Where(c => c.BranchId == filter.BranchId);
        if (filter.Status.HasValue) query = query.Where(c => c.Status == filter.Status);
        if (filter.CostCenterType.HasValue) query = query.Where(c => c.CostCenterType == filter.CostCenterType);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(c =>
                c.Code.ToLower().Contains(s) ||
                c.NameAr.ToLower().Contains(s) ||
                (c.NameEn != null && c.NameEn.ToLower().Contains(s)));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);
        var page = Math.Max(filter.Page, 1);
        var items = await query.OrderBy(c => c.Number).ThenBy(c => c.Code)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var ids = items.Select(c => c.Id).ToList();
        var linkCounts = await _context.CostCenterAllowedAccounts.AsNoTracking()
            .Where(a => ids.Contains(a.CostCenterId))
            .GroupBy(a => a.CostCenterId)
            .Select(g => new { CostCenterId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CostCenterId, x => x.Count, cancellationToken);

        var allowedMap = await _context.CostCenterAllowedAccounts.AsNoTracking()
            .Where(a => ids.Contains(a.CostCenterId))
            .GroupBy(a => a.CostCenterId)
            .Select(g => new { CostCenterId = g.Key, Ids = g.Select(x => x.ChartOfAccountId).ToList() })
            .ToDictionaryAsync(x => x.CostCenterId, x => (IReadOnlyList<Guid>)x.Ids, cancellationToken);

        var dtos = items.Select(c => CostCenterMapper.ToDto(
            c,
            linkCounts.GetValueOrDefault(c.Id),
            allowedMap.GetValueOrDefault(c.Id) ?? Array.Empty<Guid>())).ToList();

        return PagedResult<CostCenterDto>.Success(dtos, page, pageSize, total);
    }
}

public class GetCostCenterByIdQueryHandler : IRequestHandler<GetCostCenterByIdQuery, Result<CostCenterDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCostCenterByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<CostCenterDto>> Handle(GetCostCenterByIdQuery request, CancellationToken cancellationToken)
    {
        var center = await _context.CostCenters.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (center is null)
            return Result<CostCenterDto>.Failure(ErrorCodes.CostCenterNotFound, "Cost center not found.");

        var allowed = await _context.CostCenterAllowedAccounts.AsNoTracking()
            .Where(a => a.CostCenterId == center.Id)
            .Select(a => a.ChartOfAccountId)
            .ToListAsync(cancellationToken);

        return Result<CostCenterDto>.Success(CostCenterMapper.ToDto(center, allowed.Count, allowed));
    }
}

public class GetCurrenciesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCurrenciesQuery, PagedResult<CurrencyDto>>
{
    public async Task<PagedResult<CurrencyDto>> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = context.Currencies.AsNoTracking().Where(c => c.TenantId == request.TenantId);
        if (filter.Status.HasValue) query = query.Where(c => c.Status == filter.Status);
        if (filter.IsCompanyCurrency.HasValue) query = query.Where(c => c.IsCompanyCurrency == filter.IsCompanyCurrency);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(c =>
                c.Code.ToLower().Contains(s) ||
                c.NameAr.ToLower().Contains(s) ||
                c.NameEn.ToLower().Contains(s));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);
        var page = Math.Max(filter.Page, 1);
        var items = await query
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Number).ThenBy(c => c.Code)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(CurrencyMapper.ToDto).ToList();
        return PagedResult<CurrencyDto>.Success(dtos, page, pageSize, total);
    }
}

public class GetCurrencyByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCurrencyByIdQuery, Result<CurrencyDto>>
{
    public async Task<Result<CurrencyDto>> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
    {
        var currency = await context.Currencies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (currency is null)
            return Result<CurrencyDto>.Failure(ErrorCodes.CurrencyNotFound, "Currency not found.");
        return Result<CurrencyDto>.Success(CurrencyMapper.ToDto(currency));
    }
}

public class GetCurrencyExchangeRatesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCurrencyExchangeRatesQuery, PagedResult<CurrencyExchangeRateDto>>
{
    public async Task<PagedResult<CurrencyExchangeRateDto>> Handle(
        GetCurrencyExchangeRatesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = context.CurrencyExchangeRates.AsNoTracking()
            .Where(r => r.TenantId == request.TenantId);

        if (filter.CurrencyId.HasValue)
            query = query.Where(r => r.CurrencyId == filter.CurrencyId);
        if (filter.ActiveOnly == true)
            query = query.Where(r => r.IsActive);
        if (filter.AsOfDate.HasValue)
        {
            var d = filter.AsOfDate.Value;
            query = query.Where(r => r.StartDate <= d && (r.EndDate == null || r.EndDate >= d));
        }
        if (filter.FromDate.HasValue)
            query = query.Where(r => r.StartDate >= filter.FromDate || (r.EndDate != null && r.EndDate >= filter.FromDate) || r.EndDate == null);
        if (filter.ToDate.HasValue)
            query = query.Where(r => r.StartDate <= filter.ToDate);

        var joined = from r in query
                     join c in context.Currencies.AsNoTracking() on r.CurrencyId equals c.Id
                     select new { Rate = r, c.Code, c.NameAr };

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            joined = joined.Where(x =>
                x.Code.ToLower().Contains(s) ||
                x.NameAr.ToLower().Contains(s));
        }

        var total = await joined.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);
        var page = Math.Max(filter.Page, 1);

        var items = await joined
            .OrderByDescending(x => x.Rate.StartDate)
            .ThenByDescending(x => x.Rate.Number)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(x => CurrencyMapper.ToRateDto(x.Rate, x.Code, x.NameAr)).ToList();
        return PagedResult<CurrencyExchangeRateDto>.Success(dtos, page, pageSize, total);
    }
}

public class GetTrialBalanceQueryHandler : IRequestHandler<GetTrialBalanceQuery, Result<IReadOnlyList<TrialBalanceLineDto>>>
{
    private readonly ITrialBalanceService _trialBalanceService;

    public GetTrialBalanceQueryHandler(ITrialBalanceService trialBalanceService)
        => _trialBalanceService = trialBalanceService;

    public async Task<Result<IReadOnlyList<TrialBalanceLineDto>>> Handle(GetTrialBalanceQuery request, CancellationToken cancellationToken)
    {
        var lines = await _trialBalanceService.GetTrialBalanceAsync(request.TenantId, request.Filter, cancellationToken);
        return Result<IReadOnlyList<TrialBalanceLineDto>>.Success(lines);
    }
}

public class GetGeneralLedgerQueryHandler : IRequestHandler<GetGeneralLedgerQuery, Result<GeneralLedgerResultDto>>
{
    private readonly IAccountBalanceService _balanceService;

    public GetGeneralLedgerQueryHandler(IAccountBalanceService balanceService) => _balanceService = balanceService;

    public async Task<Result<GeneralLedgerResultDto>> Handle(GetGeneralLedgerQuery request, CancellationToken cancellationToken)
    {
        var result = await _balanceService.GetGeneralLedgerAsync(request.TenantId, request.Filter, cancellationToken);
        return Result<GeneralLedgerResultDto>.Success(result);
    }
}

public class GetAccountStatementQueryHandler : IRequestHandler<GetAccountStatementQuery, Result<IReadOnlyList<GeneralLedgerLineDto>>>
{
    private readonly IAccountBalanceService _balanceService;
    private readonly GastroErp.Application.Common.Interfaces.Platform.ITenantResolver _tenantResolver;

    public GetAccountStatementQueryHandler(
        IAccountBalanceService balanceService,
        GastroErp.Application.Common.Interfaces.Platform.ITenantResolver tenantResolver)
        => (_balanceService, _tenantResolver) = (balanceService, tenantResolver);

    public async Task<Result<IReadOnlyList<GeneralLedgerLineDto>>> Handle(GetAccountStatementQuery request, CancellationToken cancellationToken)
    {
        var filter = new GeneralLedgerFilterDto(
            AccountId: request.Filter.AccountId,
            FromDate: request.Filter.FromDate,
            ToDate: request.Filter.ToDate,
            IncludeOpeningBalance: true,
            Page: 1,
            PageSize: 10_000);
        var tenantId = _tenantResolver.ResolveTenantId() ?? Guid.Empty;
        var result = await _balanceService.GetGeneralLedgerAsync(tenantId, filter, cancellationToken);
        return Result<IReadOnlyList<GeneralLedgerLineDto>>.Success(result.Lines);
    }
}

public class GetJournalRegisterQueryHandler : IRequestHandler<GetJournalRegisterQuery, PagedResult<JournalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetJournalRegisterQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<JournalDto>> Handle(GetJournalRegisterQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.JournalEntries.AsNoTracking()
            .Include(j => j.Lines)
            .Where(j => j.TenantId == request.TenantId && j.Status == JournalStatus.Posted);

        if (filter.FromDate.HasValue) query = query.Where(j => j.PostingDate >= filter.FromDate);
        if (filter.ToDate.HasValue) query = query.Where(j => j.PostingDate <= filter.ToDate);
        if (filter.SourceModule.HasValue) query = query.Where(j => j.SourceModule == filter.SourceModule);

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);
        var items = await query.OrderByDescending(j => j.PostingDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<JournalDto>.Success(_mapper.Map<IReadOnlyList<JournalDto>>(items), page, pageSize, total);
    }
}

public class GetBalanceVerificationQueryHandler : IRequestHandler<GetBalanceVerificationQuery, Result<BalanceVerificationDto>>
{
    private readonly ITrialBalanceService _trialBalanceService;

    public GetBalanceVerificationQueryHandler(ITrialBalanceService trialBalanceService)
        => _trialBalanceService = trialBalanceService;

    public async Task<Result<BalanceVerificationDto>> Handle(GetBalanceVerificationQuery request, CancellationToken cancellationToken)
    {
        var result = await _trialBalanceService.VerifyBalanceAsync(request.TenantId, request.Filter, cancellationToken);
        return Result<BalanceVerificationDto>.Success(result);
    }
}
