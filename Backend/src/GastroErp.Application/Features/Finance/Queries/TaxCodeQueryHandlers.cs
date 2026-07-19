using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Queries;

public sealed class GetTaxCodesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetTaxCodesQuery, PagedResult<TaxCodeDto>>
{
    public async Task<PagedResult<TaxCodeDto>> Handle(
        GetTaxCodesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = context.TaxCodes.AsNoTracking()
            .Include(t => t.Rates)
            .Where(t => t.TenantId == request.TenantId);

        if (filter.CompanyId.HasValue)
            query = query.Where(t => t.CompanyId == filter.CompanyId);
        if (filter.BranchId.HasValue)
            query = query.Where(t => t.BranchId == filter.BranchId);
        if (filter.AppliesTo.HasValue)
            query = query.Where(t => t.AppliesTo == filter.AppliesTo || t.AppliesTo == Domain.Entities.Finance.TaxAppliesTo.Both);
        if (filter.IsActive.HasValue)
            query = query.Where(t => t.IsActive == filter.IsActive);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(t =>
                t.Code.ToLower().Contains(s) ||
                t.NameAr.ToLower().Contains(s) ||
                (t.NameEn != null && t.NameEn.ToLower().Contains(s)));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);
        var page = Math.Max(filter.Page, 1);
        var items = await query
            .OrderBy(t => t.Number)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<TaxCodeDto>();
        foreach (var item in items)
            dtos.Add(await CreateTaxCodeCommandHandler.EnrichAsync(context, item, cancellationToken));

        return PagedResult<TaxCodeDto>.Success(dtos, page, pageSize, total);
    }
}

public sealed class GetTaxCodeByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetTaxCodeByIdQuery, Result<TaxCodeDto>>
{
    public async Task<Result<TaxCodeDto>> Handle(
        GetTaxCodeByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await context.TaxCodes.AnyAsync(t => t.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<TaxCodeDto>.Failure(ErrorCodes.TaxCodeNotFound, "Tax code not found.");
        return Result<TaxCodeDto>.Success(
            await CreateTaxCodeCommandHandler.LoadDtoAsync(context, request.Id, cancellationToken));
    }
}
