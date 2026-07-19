using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Queries;

public sealed class GetFinancialNotesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetFinancialNotesQuery, PagedResult<FinancialNoteDto>>
{
    public async Task<PagedResult<FinancialNoteDto>> Handle(
        GetFinancialNotesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = context.FinancialNotes.AsNoTracking()
            .Include(d => d.Lines)
            .Where(d => d.TenantId == request.TenantId);

        if (filter.CompanyId is Guid companyId)
            query = query.Where(d => d.CompanyId == companyId);
        if (filter.BranchId is Guid branchId)
            query = query.Where(d => d.BranchId == branchId);
        if (filter.FiscalPeriodId is Guid periodId)
            query = query.Where(d => d.FiscalPeriodId == periodId);
        if (filter.NoteKind is { } kind)
            query = query.Where(d => d.NoteKind == kind);
        if (filter.Status is { } status)
            query = query.Where(d => d.Status == status);
        if (filter.PartyType is { } partyType)
            query = query.Where(d => d.PartyType == partyType);
        if (filter.NotificationReasonId is Guid reasonId)
            query = query.Where(d => d.Lines.Any(l => l.NotificationReasonId == reasonId));
        if (filter.FromDate is DateOnly from)
            query = query.Where(d => d.NoteDate >= from);
        if (filter.ToDate is DateOnly to)
            query = query.Where(d => d.NoteDate <= to);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            query = query.Where(d =>
                d.DocumentNumber.Contains(s)
                || (d.PartyName != null && d.PartyName.Contains(s))
                || (d.ReferenceNumber != null && d.ReferenceNumber.Contains(s))
                || (d.Description != null && d.Description.Contains(s)));
        }

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 500 ? 200 : filter.PageSize;
        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(d => d.NoteDate)
            .ThenByDescending(d => d.Number)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<FinancialNoteDto>(rows.Count);
        foreach (var row in rows)
            items.Add(await CreateFinancialNoteCommandHandler.EnrichAsync(context, row, cancellationToken));

        return PagedResult<FinancialNoteDto>.Success(items, page, pageSize, total);
    }
}

public sealed class GetFinancialNoteByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetFinancialNoteByIdQuery, Result<FinancialNoteDto>>
{
    public async Task<Result<FinancialNoteDto>> Handle(
        GetFinancialNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await context.FinancialNotes.AsNoTracking()
            .AnyAsync(d => d.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<FinancialNoteDto>.Failure(ErrorCodes.FinancialNoteNotFound, "Financial note not found.");

        return Result<FinancialNoteDto>.Success(
            await CreateFinancialNoteCommandHandler.LoadDtoAsync(context, request.Id, cancellationToken));
    }
}
