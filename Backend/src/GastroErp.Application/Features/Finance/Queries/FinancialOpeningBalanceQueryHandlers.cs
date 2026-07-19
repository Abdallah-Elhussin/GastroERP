using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Queries;

public sealed class GetFinancialOpeningBalancesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetFinancialOpeningBalancesQuery, PagedResult<FinancialOpeningBalanceDto>>
{
    public async Task<PagedResult<FinancialOpeningBalanceDto>> Handle(
        GetFinancialOpeningBalancesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = context.FinancialOpeningBalances.AsNoTracking()
            .Include(d => d.Lines)
            .Where(d => d.TenantId == request.TenantId);

        if (filter.CompanyId.HasValue)
            query = query.Where(d => d.CompanyId == filter.CompanyId);
        if (filter.BranchId.HasValue)
            query = query.Where(d => d.BranchId == filter.BranchId);
        if (filter.FiscalPeriodId.HasValue)
            query = query.Where(d => d.FiscalPeriodId == filter.FiscalPeriodId);
        if (filter.Status.HasValue)
            query = query.Where(d => d.Status == filter.Status);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(d =>
                d.DocumentNumber.ToLower().Contains(s) ||
                (d.Description != null && d.Description.ToLower().Contains(s)));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);
        var page = Math.Max(filter.Page, 1);
        var items = await query
            .OrderByDescending(d => d.Number)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<FinancialOpeningBalanceDto>();
        foreach (var item in items)
            dtos.Add(await CreateFinancialOpeningBalanceCommandHandler.EnrichAsync(
                context, item, cancellationToken));

        return PagedResult<FinancialOpeningBalanceDto>.Success(dtos, page, pageSize, total);
    }
}

public sealed class GetFinancialOpeningBalanceByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetFinancialOpeningBalanceByIdQuery, Result<FinancialOpeningBalanceDto>>
{
    public async Task<Result<FinancialOpeningBalanceDto>> Handle(
        GetFinancialOpeningBalanceByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await context.FinancialOpeningBalances.AnyAsync(
            d => d.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<FinancialOpeningBalanceDto>.Failure(
                ErrorCodes.OpeningBalanceNotFound, "Opening balance not found.");
        return Result<FinancialOpeningBalanceDto>.Success(
            await CreateFinancialOpeningBalanceCommandHandler.LoadDtoAsync(
                context, request.Id, cancellationToken));
    }
}
