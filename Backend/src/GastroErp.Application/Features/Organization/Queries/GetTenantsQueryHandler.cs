using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Organization.Queries;

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, PagedResult<TenantDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTenantsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tenants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(t => 
                t.NameAr.Contains(request.SearchTerm) || 
                (t.NameEn != null && t.NameEn.Contains(request.SearchTerm)) ||
                t.Slug.Contains(request.SearchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<TenantDto>>(items);

        return PagedResult<TenantDto>.Success(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
