using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Queries;

public sealed class GetDocumentTypesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDocumentTypesQuery, PagedResult<DocumentTypeDto>>
{
    public async Task<PagedResult<DocumentTypeDto>> Handle(GetDocumentTypesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = context.DocumentTypes.AsNoTracking()
            .Include(d => d.LifecycleStages)
            .Where(d => d.TenantId == request.TenantId);

        if (filter.Module.HasValue)
            query = query.Where(d => d.Module == filter.Module);
        if (filter.IsActive.HasValue)
            query = query.Where(d => d.IsActive == filter.IsActive);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(d =>
                d.Code.ToLower().Contains(s) ||
                d.NameAr.ToLower().Contains(s) ||
                d.NameEn.ToLower().Contains(s) ||
                d.Prefix.ToLower().Contains(s));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);
        var page = Math.Max(filter.Page, 1);
        var items = await query
            .OrderBy(d => d.SortOrder).ThenBy(d => d.Module).ThenBy(d => d.Code)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<DocumentTypeDto>.Success(items.Select(DocumentTypeMapper.ToDto).ToList(), page, pageSize, total);
    }
}

public sealed class GetDocumentTypeByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDocumentTypeByIdQuery, Result<DocumentTypeDto>>
{
    public async Task<Result<DocumentTypeDto>> Handle(GetDocumentTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var doc = await context.DocumentTypes.AsNoTracking()
            .Include(d => d.LifecycleStages)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<DocumentTypeDto>.Failure(ErrorCodes.DocumentTypeNotFound, "Document type not found.");
        return Result<DocumentTypeDto>.Success(DocumentTypeMapper.ToDto(doc));
    }
}
