using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Queries;
using GastroErp.Domain.Common.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Queries;

public sealed class GetAccountClassificationsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAccountClassificationsQuery, Result<IReadOnlyList<AccountClassificationDto>>>
{
    public async Task<Result<IReadOnlyList<AccountClassificationDto>>> Handle(
        GetAccountClassificationsQuery request, CancellationToken cancellationToken)
    {
        var query =
            from c in context.AccountClassifications.AsNoTracking()
            join m in context.AccountMainClassifications.AsNoTracking() on c.MainClassificationId equals m.Id
            where c.TenantId == request.TenantId
            select new { c, m };

        if (request.Filter.MainClassificationId is Guid mainId)
            query = query.Where(x => x.c.MainClassificationId == mainId);

        if (!string.IsNullOrWhiteSpace(request.Filter.Search))
        {
            var s = request.Filter.Search.Trim().ToLower();
            query = query.Where(x =>
                x.c.NameAr.ToLower().Contains(s) ||
                x.c.NameEn.ToLower().Contains(s) ||
                x.c.Code.ToLower().Contains(s) ||
                x.m.NameAr.ToLower().Contains(s) ||
                x.m.NameEn.ToLower().Contains(s));
        }

        var rows = await query
            .OrderBy(x => x.m.SortOrder)
            .ThenBy(x => x.c.Number)
            .ToListAsync(cancellationToken);

        var dtos = rows.Select(x => AccountClassificationMapper.ToDto(x.c, x.m)).ToList();
        return Result<IReadOnlyList<AccountClassificationDto>>.Success(dtos);
    }
}

public sealed class GetAccountClassificationByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAccountClassificationByIdQuery, Result<AccountClassificationDto>>
{
    public async Task<Result<AccountClassificationDto>> Handle(
        GetAccountClassificationByIdQuery request, CancellationToken cancellationToken)
    {
        var row = await (
            from c in context.AccountClassifications.AsNoTracking()
            join m in context.AccountMainClassifications.AsNoTracking() on c.MainClassificationId equals m.Id
            where c.Id == request.Id
            select new { c, m }).FirstOrDefaultAsync(cancellationToken);

        return row is null
            ? Result<AccountClassificationDto>.Failure(ErrorCodes.AccountClassificationNotFound, "Classification not found.")
            : Result<AccountClassificationDto>.Success(AccountClassificationMapper.ToDto(row.c, row.m));
    }
}

public sealed class GetAccountMainClassificationsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAccountMainClassificationsQuery, Result<IReadOnlyList<AccountMainClassificationDto>>>
{
    public async Task<Result<IReadOnlyList<AccountMainClassificationDto>>> Handle(
        GetAccountMainClassificationsQuery request, CancellationToken cancellationToken)
    {
        var rows = await context.AccountMainClassifications.AsNoTracking()
            .Where(m => m.TenantId == request.TenantId && m.IsActive)
            .OrderBy(m => m.SortOrder)
            .Select(m => new AccountMainClassificationDto(
                m.Id, m.Code, m.NameAr, m.NameEn, m.AccountType, m.SortOrder, m.IsActive))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AccountMainClassificationDto>>.Success(rows);
    }
}
