using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.ValuationGroups.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.ValuationGroups.Queries;

public sealed record GetInventoryValuationGroupsQuery(
    Guid TenantId,
    string? Search = null,
    bool? IsActive = null,
    Guid? CostCenterId = null,
    string? SortBy = null,
    bool SortDesc = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PagedResult<InventoryValuationGroupDto>>;

public sealed class GetInventoryValuationGroupsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetInventoryValuationGroupsQuery, PagedResult<InventoryValuationGroupDto>>
{
    public async Task<PagedResult<InventoryValuationGroupDto>> Handle(
        GetInventoryValuationGroupsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 50 : request.PageSize;

        var query = db.InventoryValuationGroups.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId);

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.CostCenterId.HasValue)
            query = query.Where(x => x.CostCenterId == request.CostCenterId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x =>
                x.Code.Contains(term)
                || x.NameAr.Contains(term)
                || (x.NameEn != null && x.NameEn.Contains(term))
                || (x.Description != null && x.Description.Contains(term)));
        }

        query = (request.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "code" => request.SortDesc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "nameen" => request.SortDesc ? query.OrderByDescending(x => x.NameEn) : query.OrderBy(x => x.NameEn),
            "description" => request.SortDesc ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
            "isactive" => request.SortDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            "sortorder" => request.SortDesc ? query.OrderByDescending(x => x.SortOrder) : query.OrderBy(x => x.SortOrder),
            _ => request.SortDesc ? query.OrderByDescending(x => x.NameAr) : query.OrderBy(x => x.NameAr)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var costCenterIds = items.Where(x => x.CostCenterId.HasValue)
            .Select(x => x.CostCenterId!.Value).Distinct().ToList();
        var costCenters = costCenterIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.CostCenters.AsNoTracking()
                .Where(c => costCenterIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.NameAr, cancellationToken);

        var dtos = items.Select(x => ValuationGroups.Commands.ValuationGroupMapper.ToDto(
            x,
            x.CostCenterId.HasValue && costCenters.TryGetValue(x.CostCenterId.Value, out var n) ? n : null)).ToList();

        return PagedResult<InventoryValuationGroupDto>.Success(dtos, page, pageSize, total);
    }
}

public sealed record GetInventoryValuationGroupByIdQuery(Guid Id, Guid TenantId)
    : IRequest<Result<InventoryValuationGroupDto>>;

public sealed class GetInventoryValuationGroupByIdQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetInventoryValuationGroupByIdQuery, Result<InventoryValuationGroupDto>>
{
    public async Task<Result<InventoryValuationGroupDto>> Handle(
        GetInventoryValuationGroupByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await db.InventoryValuationGroups.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.TenantId == request.TenantId, cancellationToken);
        if (entity is null)
            return Result<InventoryValuationGroupDto>.Failure("ValuationGroupNotFound", "Valuation group not found.");

        string? costCenterName = null;
        if (entity.CostCenterId.HasValue)
        {
            costCenterName = await db.CostCenters.AsNoTracking()
                .Where(c => c.Id == entity.CostCenterId.Value)
                .Select(c => c.NameAr)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return Result<InventoryValuationGroupDto>.Success(
            ValuationGroups.Commands.ValuationGroupMapper.ToDto(entity, costCenterName));
    }
}

public sealed record GetInventoryValuationGroupLookupQuery(Guid TenantId, bool ActiveOnly = true)
    : IRequest<Result<List<InventoryValuationGroupDto>>>;

public sealed class GetInventoryValuationGroupLookupQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetInventoryValuationGroupLookupQuery, Result<List<InventoryValuationGroupDto>>>
{
    public async Task<Result<List<InventoryValuationGroupDto>>> Handle(
        GetInventoryValuationGroupLookupQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.InventoryValuationGroups.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId);
        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);

        var items = await query.OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr).ToListAsync(cancellationToken);
        return Result<List<InventoryValuationGroupDto>>.Success(
            items.Select(x => ValuationGroups.Commands.ValuationGroupMapper.ToDto(x)).ToList());
    }
}
