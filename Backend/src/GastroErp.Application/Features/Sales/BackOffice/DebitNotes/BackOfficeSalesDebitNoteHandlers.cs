using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.BackOffice.Fulfillment;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Sales.BackOffice;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.BackOffice.DebitNotes;

public sealed class CreateBackOfficeSalesDebitNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateBackOfficeSalesDebitNoteCommand, Result<BackOfficeSalesDebitNoteDto>>
{
    public async Task<Result<BackOfficeSalesDebitNoteDto>> Handle(
        CreateBackOfficeSalesDebitNoteCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var lines = dto.Lines ?? [];
        if (lines.Count == 0)
            return Result<BackOfficeSalesDebitNoteDto>.Failure("NoLines", "Debit note must have lines.");

        var validation = ValidateLines(lines);
        if (validation is not null)
            return Result<BackOfficeSalesDebitNoteDto>.Failure(validation.Value.Code, validation.Value.Message);

        var customerExists = await context.Customers.AsNoTracking()
            .AnyAsync(c => c.Id == dto.CustomerId && c.TenantId == request.TenantId, cancellationToken);
        if (!customerExists)
            return Result<BackOfficeSalesDebitNoteDto>.Failure("CustomerNotFound", "Customer not found.");

        var number = string.IsNullOrWhiteSpace(dto.DebitNoteNumber)
            ? $"SDN-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : dto.DebitNoteNumber.Trim();

        try
        {
            var note = BackOfficeSalesDebitNote.CreateDraft(
                request.TenantId, number, dto.CustomerId, dto.DebitDate, dto.Currency,
                companyId: null, dto.BranchId, dto.InvoiceId, dto.Notes);

            foreach (var l in lines)
                note.AddLine(l.Description, l.Quantity, l.UnitPrice, l.TaxPercent, l.TaxAmount);

            context.BackOfficeSalesDebitNotes.Add(note);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BackOfficeSalesDebitNoteDto>.Success(BackOfficeSalesDebitNoteMapping.Map(note));
        }
        catch (BusinessException ex)
        {
            return Result<BackOfficeSalesDebitNoteDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static (string Code, string Message)? ValidateLines(IReadOnlyList<CreateBackOfficeSalesDebitNoteLineDto> lines)
    {
        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                return (ErrorCodes.InvalidQuantity, "Quantity must be greater than zero.");
            if (line.UnitPrice < 0)
                return (ErrorCodes.InvalidAmount, "Unit price cannot be negative.");
            if (line.TaxPercent is < 0 or > 100)
                return (ErrorCodes.InvalidAmount, "Tax percent is invalid.");
        }
        return null;
    }
}

public sealed class UpdateBackOfficeSalesDebitNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateBackOfficeSalesDebitNoteCommand, Result<BackOfficeSalesDebitNoteDto>>
{
    public async Task<Result<BackOfficeSalesDebitNoteDto>> Handle(
        UpdateBackOfficeSalesDebitNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await context.BackOfficeSalesDebitNotes.Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (note is null)
            return Result<BackOfficeSalesDebitNoteDto>.Failure("DebitNoteNotFound", "Debit note not found.");

        var dto = request.Dto;
        var lines = dto.Lines ?? [];
        if (lines.Count == 0)
            return Result<BackOfficeSalesDebitNoteDto>.Failure("NoLines", "Debit note must have lines.");

        var validation = CreateBackOfficeSalesDebitNoteCommandHandler.ValidateLines(lines);
        if (validation is not null)
            return Result<BackOfficeSalesDebitNoteDto>.Failure(validation.Value.Code, validation.Value.Message);

        try
        {
            note.UpdateHeader(dto.DebitDate, dto.InvoiceId, dto.BranchId, dto.Notes);
            note.ClearLines();
            foreach (var l in lines)
                note.AddLine(l.Description, l.Quantity, l.UnitPrice, l.TaxPercent, l.TaxAmount);

            context.BackOfficeSalesDebitNotes.Update(note);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BackOfficeSalesDebitNoteDto>.Success(BackOfficeSalesDebitNoteMapping.Map(note));
        }
        catch (BusinessException ex)
        {
            return Result<BackOfficeSalesDebitNoteDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ApproveBackOfficeSalesDebitNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ApproveBackOfficeSalesDebitNoteCommand, Result>
{
    public async Task<Result> Handle(ApproveBackOfficeSalesDebitNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await context.BackOfficeSalesDebitNotes.Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (note is null)
            return Result.Failure("DebitNoteNotFound", "Debit note not found.");

        try
        {
            note.Approve(request.UserId);
            context.BackOfficeSalesDebitNotes.Update(note);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UnapproveBackOfficeSalesDebitNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UnapproveBackOfficeSalesDebitNoteCommand, Result>
{
    public async Task<Result> Handle(UnapproveBackOfficeSalesDebitNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await context.BackOfficeSalesDebitNotes
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (note is null)
            return Result.Failure("DebitNoteNotFound", "Debit note not found.");

        try
        {
            note.Unapprove();
            context.BackOfficeSalesDebitNotes.Update(note);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class PostBackOfficeSalesDebitNoteCommandHandler(IBackOfficeSalesFulfillmentService fulfillment)
    : IRequestHandler<PostBackOfficeSalesDebitNoteCommand, Result>
{
    public Task<Result> Handle(PostBackOfficeSalesDebitNoteCommand request, CancellationToken cancellationToken)
        => fulfillment.PostDebitNoteAsync(request.Id, request.UserId, cancellationToken);
}

public sealed class UnpostBackOfficeSalesDebitNoteCommandHandler(IBackOfficeSalesFulfillmentService fulfillment)
    : IRequestHandler<UnpostBackOfficeSalesDebitNoteCommand, Result>
{
    public Task<Result> Handle(UnpostBackOfficeSalesDebitNoteCommand request, CancellationToken cancellationToken)
        => fulfillment.UnpostDebitNoteAsync(request.Id, request.UserId, cancellationToken);
}

public sealed class CancelBackOfficeSalesDebitNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelBackOfficeSalesDebitNoteCommand, Result>
{
    public async Task<Result> Handle(CancelBackOfficeSalesDebitNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await context.BackOfficeSalesDebitNotes
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (note is null)
            return Result.Failure("DebitNoteNotFound", "Debit note not found.");

        try
        {
            note.Cancel();
            context.BackOfficeSalesDebitNotes.Update(note);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class GetBackOfficeSalesDebitNotesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBackOfficeSalesDebitNotesQuery, PagedResult<BackOfficeSalesDebitNoteDto>>
{
    public async Task<PagedResult<BackOfficeSalesDebitNoteDto>> Handle(
        GetBackOfficeSalesDebitNotesQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = context.BackOfficeSalesDebitNotes.AsNoTracking()
            .Include(d => d.Lines)
            .Where(d => d.TenantId == request.TenantId);

        if (request.Status.HasValue)
            query = query.Where(d => d.Status == request.Status.Value);
        if (request.CustomerId.HasValue)
            query = query.Where(d => d.CustomerId == request.CustomerId.Value);
        if (request.InvoiceId.HasValue)
            query = query.Where(d => d.InvoiceId == request.InvoiceId.Value);
        if (request.BranchId.HasValue)
            query = query.Where(d => d.BranchId == request.BranchId.Value);
        if (request.From.HasValue)
            query = query.Where(d => d.DebitDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(d => d.DebitDate <= request.To.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(d => d.DebitNoteNumber.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(d => d.DebitDate)
            .ThenByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<BackOfficeSalesDebitNoteDto>.Success(
            rows.Select(BackOfficeSalesDebitNoteMapping.Map).ToList(), page, pageSize, total);
    }
}

public sealed class GetBackOfficeSalesDebitNoteByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBackOfficeSalesDebitNoteByIdQuery, Result<BackOfficeSalesDebitNoteDto>>
{
    public async Task<Result<BackOfficeSalesDebitNoteDto>> Handle(
        GetBackOfficeSalesDebitNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var note = await context.BackOfficeSalesDebitNotes.AsNoTracking()
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (note is null)
            return Result<BackOfficeSalesDebitNoteDto>.Failure("DebitNoteNotFound", "Debit note not found.");
        return Result<BackOfficeSalesDebitNoteDto>.Success(BackOfficeSalesDebitNoteMapping.Map(note));
    }
}

internal static class BackOfficeSalesDebitNoteMapping
{
    public static BackOfficeSalesDebitNoteDto Map(BackOfficeSalesDebitNote n) => new(
        n.Id, n.DebitNoteNumber, n.Status, n.CustomerId, n.InvoiceId,
        n.BranchId, n.DebitDate, n.Currency, n.Notes,
        n.SubTotal, n.TaxAmount, n.TotalAmount,
        n.JournalEntryId, n.ReversalJournalEntryId, n.ApprovedAt, n.PostedAt,
        n.Lines.Select(l => new BackOfficeSalesDebitNoteLineDto(
            l.Id, l.Description, l.Quantity, l.UnitPrice,
            l.TaxPercent, l.TaxAmount, l.LineNet, l.LineTotal)).ToList());
}
