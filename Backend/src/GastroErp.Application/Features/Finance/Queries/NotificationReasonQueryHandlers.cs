using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Queries;

public sealed class GetNotificationReasonsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetNotificationReasonsQuery, PagedResult<NotificationReasonDto>>
{
    public async Task<PagedResult<NotificationReasonDto>> Handle(
        GetNotificationReasonsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = context.NotificationReasons.AsNoTracking()
            .Where(r => r.TenantId == request.TenantId);

        if (filter.NoteType.HasValue)
            query = query.Where(r => r.NoteType == filter.NoteType);
        if (filter.PartyType.HasValue)
            query = query.Where(r => r.PartyType == filter.PartyType);
        if (filter.IsActive.HasValue)
            query = query.Where(r => r.IsActive == filter.IsActive);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(r =>
                r.Code.ToLower().Contains(s) ||
                r.NameAr.ToLower().Contains(s) ||
                (r.NameEn != null && r.NameEn.ToLower().Contains(s)));
        }

        var total = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 500);
        var page = Math.Max(filter.Page, 1);
        var items = await query
            .OrderBy(r => r.Number)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<NotificationReasonDto>();
        foreach (var item in items)
            dtos.Add(await CreateNotificationReasonCommandHandler.EnrichAsync(
                context, item, cancellationToken));

        return PagedResult<NotificationReasonDto>.Success(dtos, page, pageSize, total);
    }
}

public sealed class GetNotificationReasonByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetNotificationReasonByIdQuery, Result<NotificationReasonDto>>
{
    public async Task<Result<NotificationReasonDto>> Handle(
        GetNotificationReasonByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await context.NotificationReasons.AnyAsync(
            r => r.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<NotificationReasonDto>.Failure(
                ErrorCodes.NotificationReasonNotFound, "Notification reason not found.");
        return Result<NotificationReasonDto>.Success(
            await CreateNotificationReasonCommandHandler.LoadDtoAsync(
                context, request.Id, cancellationToken));
    }
}
