using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Queries;

public sealed class GetGeneralLedgerSettingsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetGeneralLedgerSettingsQuery, PagedResult<GeneralLedgerSettingDto>>
{
    public async Task<PagedResult<GeneralLedgerSettingDto>> Handle(
        GetGeneralLedgerSettingsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = context.GeneralLedgerSettings.AsNoTracking()
            .Where(s => s.TenantId == request.TenantId);

        if (filter.CompanyId.HasValue)
            query = query.Where(s => s.CompanyId == filter.CompanyId);
        if (filter.BranchId.HasValue)
            query = query.Where(s => s.BranchId == filter.BranchId);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            var companyIds = await context.Companies.AsNoTracking()
                .Where(c => c.TenantId == request.TenantId && c.NameAr.ToLower().Contains(s))
                .Select(c => c.Id).ToListAsync(cancellationToken);
            var branchIds = await context.Branches.AsNoTracking()
                .Where(b => b.TenantId == request.TenantId && b.NameAr.ToLower().Contains(s))
                .Select(b => b.Id).ToListAsync(cancellationToken);
            query = query.Where(x =>
                companyIds.Contains(x.CompanyId) || branchIds.Contains(x.BranchId)
                || x.ClosingMethod.ToString().ToLower().Contains(s)
                || x.Number.ToString().Contains(s));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);
        var page = Math.Max(filter.Page, 1);
        var items = await query
            .OrderBy(s => s.Number)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<GeneralLedgerSettingDto>();
        foreach (var item in items)
            dtos.Add(await CreateGeneralLedgerSettingCommandHandler.EnrichAsync(context, item, cancellationToken));

        return PagedResult<GeneralLedgerSettingDto>.Success(dtos, page, pageSize, total);
    }
}

public sealed class GetGeneralLedgerSettingByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetGeneralLedgerSettingByIdQuery, Result<GeneralLedgerSettingDto>>
{
    public async Task<Result<GeneralLedgerSettingDto>> Handle(
        GetGeneralLedgerSettingByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await context.GeneralLedgerSettings.AnyAsync(s => s.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<GeneralLedgerSettingDto>.Failure(
                ErrorCodes.GeneralLedgerSettingNotFound, "GL settings not found.");
        return Result<GeneralLedgerSettingDto>.Success(
            await CreateGeneralLedgerSettingCommandHandler.LoadDtoAsync(context, request.Id, cancellationToken));
    }
}
