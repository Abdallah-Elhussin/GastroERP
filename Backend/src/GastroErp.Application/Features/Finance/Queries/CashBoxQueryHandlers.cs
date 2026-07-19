using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Queries;

public sealed class GetCashBoxesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCashBoxesQuery, PagedResult<CashBoxDto>>
{
    public async Task<PagedResult<CashBoxDto>> Handle(GetCashBoxesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = context.CashBoxes.AsNoTracking()
            .Include(b => b.AuthorizedUsers)
            .Include(b => b.Devices)
            .Where(b => b.TenantId == request.TenantId);

        if (filter.CompanyId.HasValue)
            query = query.Where(b => b.CompanyId == filter.CompanyId);
        if (filter.BranchId.HasValue)
            query = query.Where(b => b.BranchId == filter.BranchId);
        if (filter.CurrencyId.HasValue)
            query = query.Where(b => b.CurrencyId == filter.CurrencyId);
        if (filter.IsActive.HasValue)
            query = query.Where(b => b.IsActive == filter.IsActive);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(b =>
                b.NameAr.ToLower().Contains(s) ||
                (b.NameEn != null && b.NameEn.ToLower().Contains(s)) ||
                b.Code.ToLower().Contains(s));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);
        var page = Math.Max(filter.Page, 1);
        var items = await query
            .OrderBy(b => b.Number)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<CashBoxDto>();
        foreach (var box in items)
            dtos.Add(await CreateCashBoxCommandHandler.EnrichAsync(context, box, cancellationToken));

        return PagedResult<CashBoxDto>.Success(dtos, page, pageSize, total);
    }
}

public sealed class GetCashBoxByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCashBoxByIdQuery, Result<CashBoxDto>>
{
    public async Task<Result<CashBoxDto>> Handle(GetCashBoxByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await context.CashBoxes.AnyAsync(b => b.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<CashBoxDto>.Failure(ErrorCodes.CashBoxNotFound, "Cash box not found.");
        return Result<CashBoxDto>.Success(
            await CreateCashBoxCommandHandler.LoadDtoAsync(context, request.Id, cancellationToken));
    }
}
