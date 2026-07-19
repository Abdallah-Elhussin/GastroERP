using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Queries;

public sealed class GetTaxRegistrationsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetTaxRegistrationsQuery, PagedResult<TaxRegistrationProfileDto>>
{
    public async Task<PagedResult<TaxRegistrationProfileDto>> Handle(
        GetTaxRegistrationsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = context.TaxRegistrationProfiles.AsNoTracking()
            .Include(p => p.Certificates)
            .Where(p => p.TenantId == request.TenantId);

        if (filter.CompanyId.HasValue)
            query = query.Where(p => p.CompanyId == filter.CompanyId);
        if (filter.BranchId.HasValue)
            query = query.Where(p => p.BranchId == filter.BranchId);
        if (filter.Status.HasValue)
            query = query.Where(p => p.Status == filter.Status);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(p =>
                p.VatNumber.ToLower().Contains(s) ||
                (p.BranchVatNumber != null && p.BranchVatNumber.ToLower().Contains(s)) ||
                (p.TaxOffice != null && p.TaxOffice.ToLower().Contains(s)) ||
                (p.ActivityCode != null && p.ActivityCode.ToLower().Contains(s)) ||
                (p.ActivityNameAr != null && p.ActivityNameAr.ToLower().Contains(s)));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);
        var page = Math.Max(filter.Page, 1);
        var items = await query
            .OrderBy(p => p.Number)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<TaxRegistrationProfileDto>();
        foreach (var profile in items)
            dtos.Add(await CreateTaxRegistrationCommandHandler.EnrichAsync(context, profile, cancellationToken));

        return PagedResult<TaxRegistrationProfileDto>.Success(dtos, page, pageSize, total);
    }
}

public sealed class GetTaxRegistrationByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetTaxRegistrationByIdQuery, Result<TaxRegistrationProfileDto>>
{
    public async Task<Result<TaxRegistrationProfileDto>> Handle(
        GetTaxRegistrationByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await context.TaxRegistrationProfiles.AnyAsync(p => p.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<TaxRegistrationProfileDto>.Failure(
                ErrorCodes.TaxRegistrationNotFound, "Tax registration not found.");
        return Result<TaxRegistrationProfileDto>.Success(
            await CreateTaxRegistrationCommandHandler.LoadDtoAsync(context, request.Id, cancellationToken));
    }
}
