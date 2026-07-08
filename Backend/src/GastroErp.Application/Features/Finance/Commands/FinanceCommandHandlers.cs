using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
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

        var account = ChartOfAccount.Create(
            request.TenantId, request.Dto.AccountNumber, request.Dto.NameAr, request.Dto.AccountType,
            request.Dto.AccountCategory, !request.Dto.IsSummaryAccount, request.Dto.IsSummaryAccount,
            request.Dto.ParentAccountId, request.Dto.NameEn, request.Dto.Currency, request.Dto.SortOrder);

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
        account.Update(request.Dto.NameAr, request.Dto.NameEn, request.Dto.AccountCategory, request.Dto.IsSummaryAccount, request.Dto.SortOrder);
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
        account.SoftDelete(null);
        _context.ChartOfAccounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
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
    private readonly IMapper _mapper;

    public CreateFiscalPeriodCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<FiscalPeriodDto>> Handle(CreateFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.FiscalPeriods
            .AnyAsync(p => p.TenantId == request.TenantId && p.FiscalYear == request.Dto.FiscalYear, cancellationToken);
        if (exists) return Result<FiscalPeriodDto>.Failure(ErrorCodes.InvalidFiscalPeriodDates, "Fiscal year already exists.");

        var period = FiscalPeriod.Create(
            request.TenantId, request.Dto.FiscalYear, request.Dto.Name,
            request.Dto.StartDate, request.Dto.EndDate);

        _context.FiscalPeriods.Add(period);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<FiscalPeriodDto>.Success(_mapper.Map<FiscalPeriodDto>(period));
    }
}

public class CloseFiscalPeriodCommandHandler : IRequestHandler<CloseFiscalPeriodCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public CloseFiscalPeriodCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(CloseFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _context.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (period is null) return Result.Failure(ErrorCodes.FiscalPeriodNotFound, "Fiscal period not found.");
        try { period.Close(); }
        catch (Domain.Common.Exceptions.BusinessException ex) { return Result.Failure(ex.ErrorCode, ex.Message); }
        _context.FiscalPeriods.Update(period);
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
        var period = await _context.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (period is null) return Result.Failure(ErrorCodes.FiscalPeriodNotFound, "Fiscal period not found.");
        try { period.Lock(); }
        catch (Domain.Common.Exceptions.BusinessException ex) { return Result.Failure(ex.ErrorCode, ex.Message); }
        _context.FiscalPeriods.Update(period);
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
        _context.FiscalPeriods.Update(period);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CreateCostCenterCommandHandler : IRequestHandler<CreateCostCenterCommand, Result<CostCenterDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateCostCenterCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<CostCenterDto>> Handle(CreateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.CostCenters
            .AnyAsync(c => c.TenantId == request.TenantId && c.Code == request.Dto.Code.ToUpperInvariant(), cancellationToken);
        if (exists) return Result<CostCenterDto>.Failure(ErrorCodes.RequiredField, "Cost center code already exists.");

        var center = CostCenter.Create(
            request.TenantId, request.Dto.BranchId, request.Dto.Code,
            request.Dto.NameAr, request.Dto.DepartmentId, request.Dto.NameEn);

        _context.CostCenters.Add(center);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<CostCenterDto>.Success(_mapper.Map<CostCenterDto>(center));
    }
}

public class UpdateCostCenterCommandHandler : IRequestHandler<UpdateCostCenterCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateCostCenterCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var center = await _context.CostCenters.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (center is null) return Result.Failure(ErrorCodes.CostCenterNotFound, "Cost center not found.");
        center.Update(request.Dto.NameAr, request.Dto.NameEn, request.Dto.DepartmentId);
        _context.CostCenters.Update(center);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ActivateCostCenterCommandHandler : IRequestHandler<ActivateCostCenterCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public ActivateCostCenterCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var center = await _context.CostCenters.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (center is null) return Result.Failure(ErrorCodes.CostCenterNotFound, "Cost center not found.");
        center.Activate();
        _context.CostCenters.Update(center);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeactivateCostCenterCommandHandler : IRequestHandler<DeactivateCostCenterCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeactivateCostCenterCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeactivateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var center = await _context.CostCenters.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (center is null) return Result.Failure(ErrorCodes.CostCenterNotFound, "Cost center not found.");
        center.Deactivate();
        _context.CostCenters.Update(center);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
