using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Queries;

public sealed class GetBanksQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBanksQuery, PagedResult<BankDto>>
{
    public async Task<PagedResult<BankDto>> Handle(GetBanksQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = context.Banks.AsNoTracking()
            .Include(b => b.Accounts)
            .Where(b => b.TenantId == request.TenantId);

        if (filter.CompanyId.HasValue)
            query = query.Where(b => b.CompanyId == filter.CompanyId);
        if (filter.BranchId.HasValue)
            query = query.Where(b => b.BranchId == filter.BranchId);
        if (filter.CurrencyId.HasValue)
            query = query.Where(b =>
                b.BaseCurrencyId == filter.CurrencyId
                || b.Accounts.Any(a => a.CurrencyId == filter.CurrencyId));
        if (filter.IsActive.HasValue)
            query = query.Where(b => b.IsActive == filter.IsActive);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(b =>
                b.NameAr.ToLower().Contains(s) ||
                (b.NameEn != null && b.NameEn.ToLower().Contains(s)) ||
                (b.Code != null && b.Code.ToLower().Contains(s)));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);
        var page = Math.Max(filter.Page, 1);
        var items = await query
            .OrderBy(b => b.Number)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<BankDto>();
        foreach (var bank in items)
            dtos.Add(await CreateBankCommandHandler.EnrichAsync(context, bank, cancellationToken));

        return PagedResult<BankDto>.Success(dtos, page, pageSize, total);
    }
}

public sealed class GetBankByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBankByIdQuery, Result<BankDto>>
{
    public async Task<Result<BankDto>> Handle(GetBankByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await context.Banks.AnyAsync(b => b.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<BankDto>.Failure(ErrorCodes.BankNotFound, "Bank not found.");
        return Result<BankDto>.Success(await CreateBankCommandHandler.LoadDtoAsync(context, request.Id, cancellationToken));
    }
}
