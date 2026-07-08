using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Queries;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Domain.Common.Localization;
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
        var accounts = await _context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.TenantId == request.TenantId && a.IsActive)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.AccountNumber)
            .Select(a => new { a.Id, a.AccountNumber, a.NameAr, a.NameEn, a.ParentAccountId, a.AccountType, a.AccountCategory, a.IsActive, a.SortOrder })
            .ToListAsync(cancellationToken);

        List<AccountTreeNodeDto> BuildChildren(Guid parentId) =>
            accounts.Where(a => a.ParentAccountId == parentId)
                .Select(a => new AccountTreeNodeDto(
                    a.Id, a.AccountNumber, a.NameAr, a.NameEn, a.AccountType, a.AccountCategory,
                    a.IsActive, a.SortOrder, BuildChildren(a.Id)))
                .ToList();

        var roots = accounts.Where(a => a.ParentAccountId is null)
            .Select(a => new AccountTreeNodeDto(
                a.Id, a.AccountNumber, a.NameAr, a.NameEn, a.AccountType, a.AccountCategory,
                a.IsActive, a.SortOrder, BuildChildren(a.Id)))
            .ToList();

        return Result<IReadOnlyList<AccountTreeNodeDto>>.Success(roots);
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
        if (filter.FromDate.HasValue) query = query.Where(j => j.PostingDate >= filter.FromDate);
        if (filter.ToDate.HasValue) query = query.Where(j => j.PostingDate <= filter.ToDate);

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
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
                l.CostCenterId, l.Debit, l.Credit, l.Currency, l.Description);
        }).ToList();

        return Result<JournalDetailDto>.Success(new JournalDetailDto(
            journal.Id, journal.EntryNumber, journal.PostingDate, journal.FiscalPeriodId,
            journal.Description, journal.Reference, journal.SourceModule, journal.SourceDocumentId,
            journal.Status, journal.PostedAt, lines));
    }
}

public class GetFiscalPeriodsQueryHandler : IRequestHandler<GetFiscalPeriodsQuery, Result<IReadOnlyList<FiscalPeriodDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFiscalPeriodsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<FiscalPeriodDto>>> Handle(GetFiscalPeriodsQuery request, CancellationToken cancellationToken)
    {
        var periods = await _context.FiscalPeriods.AsNoTracking()
            .Where(p => p.TenantId == request.TenantId)
            .OrderByDescending(p => p.FiscalYear)
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<FiscalPeriodDto>>.Success(_mapper.Map<IReadOnlyList<FiscalPeriodDto>>(periods));
    }
}

public class GetFiscalPeriodByIdQueryHandler : IRequestHandler<GetFiscalPeriodByIdQuery, Result<FiscalPeriodDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFiscalPeriodByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<FiscalPeriodDto>> Handle(GetFiscalPeriodByIdQuery request, CancellationToken cancellationToken)
    {
        var period = await _context.FiscalPeriods.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        return period is null
            ? Result<FiscalPeriodDto>.Failure(ErrorCodes.FiscalPeriodNotFound, "Fiscal period not found.")
            : Result<FiscalPeriodDto>.Success(_mapper.Map<FiscalPeriodDto>(period));
    }
}

public class GetCostCentersQueryHandler : IRequestHandler<GetCostCentersQuery, PagedResult<CostCenterDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCostCentersQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<CostCenterDto>> Handle(GetCostCentersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.CostCenters.AsNoTracking().Where(c => c.TenantId == request.TenantId);
        if (filter.BranchId.HasValue) query = query.Where(c => c.BranchId == filter.BranchId);
        if (filter.Status.HasValue) query = query.Where(c => c.Status == filter.Status);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            query = query.Where(c => c.Code.Contains(s) || c.NameAr.Contains(s));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);
        var items = await query.OrderBy(c => c.Code)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<CostCenterDto>.Success(_mapper.Map<IReadOnlyList<CostCenterDto>>(items), page, pageSize, total);
    }
}

public class GetCostCenterByIdQueryHandler : IRequestHandler<GetCostCenterByIdQuery, Result<CostCenterDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCostCenterByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<CostCenterDto>> Handle(GetCostCenterByIdQuery request, CancellationToken cancellationToken)
    {
        var center = await _context.CostCenters.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        return center is null
            ? Result<CostCenterDto>.Failure(ErrorCodes.CostCenterNotFound, "Cost center not found.")
            : Result<CostCenterDto>.Success(_mapper.Map<CostCenterDto>(center));
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

public class GetGeneralLedgerQueryHandler : IRequestHandler<GetGeneralLedgerQuery, Result<IReadOnlyList<GeneralLedgerLineDto>>>
{
    private readonly IAccountBalanceService _balanceService;

    public GetGeneralLedgerQueryHandler(IAccountBalanceService balanceService) => _balanceService = balanceService;

    public async Task<Result<IReadOnlyList<GeneralLedgerLineDto>>> Handle(GetGeneralLedgerQuery request, CancellationToken cancellationToken)
    {
        var lines = await _balanceService.GetGeneralLedgerAsync(request.Filter, cancellationToken);
        return Result<IReadOnlyList<GeneralLedgerLineDto>>.Success(lines);
    }
}

public class GetAccountStatementQueryHandler : IRequestHandler<GetAccountStatementQuery, Result<IReadOnlyList<GeneralLedgerLineDto>>>
{
    private readonly IAccountBalanceService _balanceService;

    public GetAccountStatementQueryHandler(IAccountBalanceService balanceService) => _balanceService = balanceService;

    public async Task<Result<IReadOnlyList<GeneralLedgerLineDto>>> Handle(GetAccountStatementQuery request, CancellationToken cancellationToken)
    {
        var filter = new GeneralLedgerFilterDto(
            request.Filter.AccountId, request.Filter.FromDate, request.Filter.ToDate);
        var lines = await _balanceService.GetGeneralLedgerAsync(filter, cancellationToken);
        return Result<IReadOnlyList<GeneralLedgerLineDto>>.Success(lines);
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
