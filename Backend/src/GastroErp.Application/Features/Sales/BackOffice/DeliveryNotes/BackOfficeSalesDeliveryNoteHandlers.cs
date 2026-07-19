using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.BackOffice.Fulfillment;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Sales.BackOffice;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.BackOffice.DeliveryNotes;

public sealed class CreateBackOfficeSalesDeliveryNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateBackOfficeSalesDeliveryNoteCommand, Result<BackOfficeSalesDeliveryNoteDto>>
{
    public async Task<Result<BackOfficeSalesDeliveryNoteDto>> Handle(
        CreateBackOfficeSalesDeliveryNoteCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var lines = dto.Lines ?? [];
        if (lines.Count == 0)
            return Result<BackOfficeSalesDeliveryNoteDto>.Failure("NoLines", "Delivery note must have lines.");

        var order = await context.BackOfficeSalesOrders.AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.TenantId == request.TenantId, cancellationToken);
        if (order is null)
            return Result<BackOfficeSalesDeliveryNoteDto>.Failure("OrderNotFound", "Sales order not found.");
        if (order.Status is not (BackOfficeSalesDocumentStatus.Approved or BackOfficeSalesDocumentStatus.Posted))
            return Result<BackOfficeSalesDeliveryNoteDto>.Failure(ErrorCodes.SalesInvalidStatusTransition,
                "Sales order must be approved before creating a delivery note.");
        if (order.CustomerId != dto.CustomerId)
            return Result<BackOfficeSalesDeliveryNoteDto>.Failure("CustomerMismatch",
                "Delivery customer must match the sales order customer.");

        var validation = ValidateAgainstOrder(order, lines);
        if (validation is not null)
            return Result<BackOfficeSalesDeliveryNoteDto>.Failure(validation.Value.Code, validation.Value.Message);

        var number = string.IsNullOrWhiteSpace(dto.DeliveryNumber)
            ? $"SDN-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : dto.DeliveryNumber.Trim();

        try
        {
            var note = BackOfficeSalesDeliveryNote.CreateDraft(
                request.TenantId, number, dto.CustomerId, dto.WarehouseId, dto.OrderId,
                dto.DeliveryDate, companyId: null, dto.BranchId, dto.Notes);

            foreach (var l in lines)
                note.AddLine(l.OrderLineId, l.Description, l.Quantity, l.InventoryItemId, l.UnitId, l.UnitCost);

            context.BackOfficeSalesDeliveryNotes.Add(note);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BackOfficeSalesDeliveryNoteDto>.Success(BackOfficeSalesDeliveryNoteMapping.Map(note));
        }
        catch (BusinessException ex)
        {
            return Result<BackOfficeSalesDeliveryNoteDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static (string Code, string Message)? ValidateAgainstOrder(
        BackOfficeSalesOrder order,
        IReadOnlyList<CreateBackOfficeSalesDeliveryNoteLineDto> lines)
    {
        var byOrderLine = lines.GroupBy(l => l.OrderLineId);
        foreach (var group in byOrderLine)
        {
            var orderLine = order.Lines.FirstOrDefault(l => l.Id == group.Key);
            if (orderLine is null)
                return ("OrderLineNotFound", $"Order line '{group.Key}' does not belong to the order.");

            var totalQty = group.Sum(l => l.Quantity);
            if (totalQty <= 0)
                return (ErrorCodes.InvalidQuantity, "Delivery quantity must be greater than zero.");
            if (totalQty > orderLine.RemainingToDeliver + 0.0001m)
                return (ErrorCodes.InvalidQuantity,
                    $"Delivery quantity exceeds remaining for '{orderLine.Description}'.");
        }
        return null;
    }
}

public sealed class UpdateBackOfficeSalesDeliveryNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateBackOfficeSalesDeliveryNoteCommand, Result<BackOfficeSalesDeliveryNoteDto>>
{
    public async Task<Result<BackOfficeSalesDeliveryNoteDto>> Handle(
        UpdateBackOfficeSalesDeliveryNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await context.BackOfficeSalesDeliveryNotes.Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (note is null)
            return Result<BackOfficeSalesDeliveryNoteDto>.Failure("DeliveryNoteNotFound", "Delivery note not found.");

        var dto = request.Dto;
        var lines = dto.Lines ?? [];
        if (lines.Count == 0)
            return Result<BackOfficeSalesDeliveryNoteDto>.Failure("NoLines", "Delivery note must have lines.");

        var order = await context.BackOfficeSalesOrders.AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == note.OrderId, cancellationToken);
        if (order is null)
            return Result<BackOfficeSalesDeliveryNoteDto>.Failure("OrderNotFound", "Related sales order not found.");

        var validation = CreateBackOfficeSalesDeliveryNoteCommandHandler.ValidateAgainstOrder(order, lines);
        if (validation is not null)
            return Result<BackOfficeSalesDeliveryNoteDto>.Failure(validation.Value.Code, validation.Value.Message);

        try
        {
            note.UpdateHeader(dto.DeliveryDate, dto.WarehouseId, dto.BranchId, dto.Notes);
            note.ClearLines();
            foreach (var l in lines)
                note.AddLine(l.OrderLineId, l.Description, l.Quantity, l.InventoryItemId, l.UnitId, l.UnitCost);

            context.BackOfficeSalesDeliveryNotes.Update(note);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BackOfficeSalesDeliveryNoteDto>.Success(BackOfficeSalesDeliveryNoteMapping.Map(note));
        }
        catch (BusinessException ex)
        {
            return Result<BackOfficeSalesDeliveryNoteDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ApproveBackOfficeSalesDeliveryNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ApproveBackOfficeSalesDeliveryNoteCommand, Result>
{
    public async Task<Result> Handle(ApproveBackOfficeSalesDeliveryNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await context.BackOfficeSalesDeliveryNotes.Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (note is null)
            return Result.Failure("DeliveryNoteNotFound", "Delivery note not found.");

        try
        {
            note.Approve(request.UserId);
            context.BackOfficeSalesDeliveryNotes.Update(note);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UnapproveBackOfficeSalesDeliveryNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UnapproveBackOfficeSalesDeliveryNoteCommand, Result>
{
    public async Task<Result> Handle(UnapproveBackOfficeSalesDeliveryNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await context.BackOfficeSalesDeliveryNotes
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (note is null)
            return Result.Failure("DeliveryNoteNotFound", "Delivery note not found.");

        try
        {
            note.Unapprove();
            context.BackOfficeSalesDeliveryNotes.Update(note);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class PostBackOfficeSalesDeliveryNoteCommandHandler(IBackOfficeSalesFulfillmentService fulfillment)
    : IRequestHandler<PostBackOfficeSalesDeliveryNoteCommand, Result>
{
    public Task<Result> Handle(PostBackOfficeSalesDeliveryNoteCommand request, CancellationToken cancellationToken)
        => fulfillment.PostDeliveryAsync(request.Id, request.UserId, cancellationToken);
}

public sealed class UnpostBackOfficeSalesDeliveryNoteCommandHandler(IBackOfficeSalesFulfillmentService fulfillment)
    : IRequestHandler<UnpostBackOfficeSalesDeliveryNoteCommand, Result>
{
    public Task<Result> Handle(UnpostBackOfficeSalesDeliveryNoteCommand request, CancellationToken cancellationToken)
        => fulfillment.UnpostDeliveryAsync(request.Id, request.UserId, cancellationToken);
}

public sealed class CancelBackOfficeSalesDeliveryNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelBackOfficeSalesDeliveryNoteCommand, Result>
{
    public async Task<Result> Handle(CancelBackOfficeSalesDeliveryNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await context.BackOfficeSalesDeliveryNotes
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (note is null)
            return Result.Failure("DeliveryNoteNotFound", "Delivery note not found.");

        try
        {
            note.Cancel();
            context.BackOfficeSalesDeliveryNotes.Update(note);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class GetBackOfficeSalesDeliveryNotesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBackOfficeSalesDeliveryNotesQuery, PagedResult<BackOfficeSalesDeliveryNoteDto>>
{
    public async Task<PagedResult<BackOfficeSalesDeliveryNoteDto>> Handle(
        GetBackOfficeSalesDeliveryNotesQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = context.BackOfficeSalesDeliveryNotes.AsNoTracking()
            .Include(d => d.Lines)
            .Where(d => d.TenantId == request.TenantId);

        if (request.Status.HasValue)
            query = query.Where(d => d.Status == request.Status.Value);
        if (request.CustomerId.HasValue)
            query = query.Where(d => d.CustomerId == request.CustomerId.Value);
        if (request.OrderId.HasValue)
            query = query.Where(d => d.OrderId == request.OrderId.Value);
        if (request.WarehouseId.HasValue)
            query = query.Where(d => d.WarehouseId == request.WarehouseId.Value);
        if (request.BranchId.HasValue)
            query = query.Where(d => d.BranchId == request.BranchId.Value);
        if (request.From.HasValue)
            query = query.Where(d => d.DeliveryDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(d => d.DeliveryDate <= request.To.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(d => d.DeliveryNumber.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(d => d.DeliveryDate)
            .ThenByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<BackOfficeSalesDeliveryNoteDto>.Success(
            rows.Select(BackOfficeSalesDeliveryNoteMapping.Map).ToList(), page, pageSize, total);
    }
}

public sealed class GetBackOfficeSalesDeliveryNoteByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBackOfficeSalesDeliveryNoteByIdQuery, Result<BackOfficeSalesDeliveryNoteDto>>
{
    public async Task<Result<BackOfficeSalesDeliveryNoteDto>> Handle(
        GetBackOfficeSalesDeliveryNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var note = await context.BackOfficeSalesDeliveryNotes.AsNoTracking()
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (note is null)
            return Result<BackOfficeSalesDeliveryNoteDto>.Failure("DeliveryNoteNotFound", "Delivery note not found.");
        return Result<BackOfficeSalesDeliveryNoteDto>.Success(BackOfficeSalesDeliveryNoteMapping.Map(note));
    }
}

internal static class BackOfficeSalesDeliveryNoteMapping
{
    public static BackOfficeSalesDeliveryNoteDto Map(BackOfficeSalesDeliveryNote note) => new(
        note.Id, note.DeliveryNumber, note.Status, note.CustomerId, note.WarehouseId, note.OrderId,
        note.BranchId, note.DeliveryDate, note.Notes, note.TotalCost,
        note.JournalEntryId, note.ReversalJournalEntryId, note.ApprovedAt, note.PostedAt,
        note.Lines.Select(l => new BackOfficeSalesDeliveryNoteLineDto(
            l.Id, l.OrderLineId, l.InventoryItemId, l.UnitId,
            l.Description, l.Quantity, l.UnitCost, l.LineCost)).ToList());
}
