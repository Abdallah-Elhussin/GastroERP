using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.BackOffice.Fulfillment;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Sales.BackOffice;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.BackOffice.Returns;

public sealed class CreateBackOfficeSalesReturnCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateBackOfficeSalesReturnCommand, Result<BackOfficeSalesReturnDto>>
{
    public async Task<Result<BackOfficeSalesReturnDto>> Handle(
        CreateBackOfficeSalesReturnCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var lines = dto.Lines ?? [];
        if (lines.Count == 0)
            return Result<BackOfficeSalesReturnDto>.Failure("NoLines", "Return must have lines.");

        var invoice = await context.BackOfficeSalesInvoices.AsNoTracking()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == dto.InvoiceId && i.TenantId == request.TenantId, cancellationToken);
        if (invoice is null)
            return Result<BackOfficeSalesReturnDto>.Failure("InvoiceNotFound", "Related invoice not found.");
        if (invoice.Status != BackOfficeSalesDocumentStatus.Posted)
            return Result<BackOfficeSalesReturnDto>.Failure(ErrorCodes.SalesInvalidStatusTransition,
                "Invoice must be posted before returning.");
        if (invoice.CustomerId != dto.CustomerId)
            return Result<BackOfficeSalesReturnDto>.Failure("CustomerMismatch",
                "Return customer must match invoice customer.");

        var validation = ValidateAgainstInvoice(invoice, lines);
        if (validation is not null)
            return Result<BackOfficeSalesReturnDto>.Failure(validation.Value.Code, validation.Value.Message);

        var number = string.IsNullOrWhiteSpace(dto.ReturnNumber)
            ? $"SR-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : dto.ReturnNumber.Trim();

        try
        {
            var ret = BackOfficeSalesReturn.CreateDraft(
                request.TenantId, number, dto.CustomerId, dto.InvoiceId, dto.ReturnDate,
                companyId: null, dto.BranchId, dto.WarehouseId ?? invoice.WarehouseId,
                dto.Notes, dto.DiscountAmount);

            foreach (var l in lines)
                ret.AddLine(l.InvoiceLineId, l.Description, l.Quantity, l.UnitPrice,
                    l.InventoryItemId, l.UnitId, l.LineNature, l.TaxPercent, l.TaxAmount, l.UnitCost);

            context.BackOfficeSalesReturns.Add(ret);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BackOfficeSalesReturnDto>.Success(BackOfficeSalesReturnMapping.Map(ret));
        }
        catch (BusinessException ex)
        {
            return Result<BackOfficeSalesReturnDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static (string Code, string Message)? ValidateAgainstInvoice(
        BackOfficeSalesInvoice invoice,
        IReadOnlyList<CreateBackOfficeSalesReturnLineDto> lines)
    {
        var byInvoiceLine = lines.GroupBy(l => l.InvoiceLineId);
        foreach (var group in byInvoiceLine)
        {
            var invoiceLine = invoice.Lines.FirstOrDefault(l => l.Id == group.Key);
            if (invoiceLine is null)
                return ("InvoiceLineNotFound", $"Invoice line '{group.Key}' does not belong to the invoice.");

            var totalQty = group.Sum(l => l.Quantity);
            if (totalQty <= 0)
                return (ErrorCodes.InvalidQuantity, "Return quantity must be greater than zero.");
            if (totalQty > invoiceLine.RemainingToReturn + 0.0001m)
                return (ErrorCodes.InvalidQuantity,
                    $"Return quantity exceeds remaining for '{invoiceLine.Description}'.");
        }
        return null;
    }
}

public sealed class UpdateBackOfficeSalesReturnCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateBackOfficeSalesReturnCommand, Result<BackOfficeSalesReturnDto>>
{
    public async Task<Result<BackOfficeSalesReturnDto>> Handle(
        UpdateBackOfficeSalesReturnCommand request, CancellationToken cancellationToken)
    {
        var ret = await context.BackOfficeSalesReturns.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (ret is null)
            return Result<BackOfficeSalesReturnDto>.Failure("ReturnNotFound", "Sales return not found.");

        var dto = request.Dto;
        var lines = dto.Lines ?? [];
        if (lines.Count == 0)
            return Result<BackOfficeSalesReturnDto>.Failure("NoLines", "Return must have lines.");

        var invoice = await context.BackOfficeSalesInvoices.AsNoTracking()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == ret.InvoiceId, cancellationToken);
        if (invoice is null)
            return Result<BackOfficeSalesReturnDto>.Failure("InvoiceNotFound", "Related invoice not found.");

        var validation = CreateBackOfficeSalesReturnCommandHandler.ValidateAgainstInvoice(invoice, lines);
        if (validation is not null)
            return Result<BackOfficeSalesReturnDto>.Failure(validation.Value.Code, validation.Value.Message);

        try
        {
            ret.UpdateHeader(dto.ReturnDate, dto.WarehouseId, dto.BranchId, dto.Notes, dto.DiscountAmount);
            ret.ClearLines();
            foreach (var l in lines)
                ret.AddLine(l.InvoiceLineId, l.Description, l.Quantity, l.UnitPrice,
                    l.InventoryItemId, l.UnitId, l.LineNature, l.TaxPercent, l.TaxAmount, l.UnitCost);

            context.BackOfficeSalesReturns.Update(ret);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BackOfficeSalesReturnDto>.Success(BackOfficeSalesReturnMapping.Map(ret));
        }
        catch (BusinessException ex)
        {
            return Result<BackOfficeSalesReturnDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ApproveBackOfficeSalesReturnCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ApproveBackOfficeSalesReturnCommand, Result>
{
    public async Task<Result> Handle(ApproveBackOfficeSalesReturnCommand request, CancellationToken cancellationToken)
    {
        var ret = await context.BackOfficeSalesReturns.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (ret is null)
            return Result.Failure("ReturnNotFound", "Sales return not found.");

        try
        {
            ret.Approve(request.UserId);
            context.BackOfficeSalesReturns.Update(ret);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UnapproveBackOfficeSalesReturnCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UnapproveBackOfficeSalesReturnCommand, Result>
{
    public async Task<Result> Handle(UnapproveBackOfficeSalesReturnCommand request, CancellationToken cancellationToken)
    {
        var ret = await context.BackOfficeSalesReturns
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (ret is null)
            return Result.Failure("ReturnNotFound", "Sales return not found.");

        try
        {
            ret.Unapprove();
            context.BackOfficeSalesReturns.Update(ret);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class PostBackOfficeSalesReturnCommandHandler(IBackOfficeSalesFulfillmentService fulfillment)
    : IRequestHandler<PostBackOfficeSalesReturnCommand, Result>
{
    public Task<Result> Handle(PostBackOfficeSalesReturnCommand request, CancellationToken cancellationToken)
        => fulfillment.PostReturnAsync(request.Id, request.UserId, cancellationToken);
}

public sealed class UnpostBackOfficeSalesReturnCommandHandler(IBackOfficeSalesFulfillmentService fulfillment)
    : IRequestHandler<UnpostBackOfficeSalesReturnCommand, Result>
{
    public Task<Result> Handle(UnpostBackOfficeSalesReturnCommand request, CancellationToken cancellationToken)
        => fulfillment.UnpostReturnAsync(request.Id, request.UserId, cancellationToken);
}

public sealed class CancelBackOfficeSalesReturnCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelBackOfficeSalesReturnCommand, Result>
{
    public async Task<Result> Handle(CancelBackOfficeSalesReturnCommand request, CancellationToken cancellationToken)
    {
        var ret = await context.BackOfficeSalesReturns
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (ret is null)
            return Result.Failure("ReturnNotFound", "Sales return not found.");

        try
        {
            ret.Cancel();
            context.BackOfficeSalesReturns.Update(ret);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class GetBackOfficeSalesReturnsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBackOfficeSalesReturnsQuery, PagedResult<BackOfficeSalesReturnDto>>
{
    public async Task<PagedResult<BackOfficeSalesReturnDto>> Handle(
        GetBackOfficeSalesReturnsQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = context.BackOfficeSalesReturns.AsNoTracking()
            .Include(r => r.Lines)
            .Where(r => r.TenantId == request.TenantId);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);
        if (request.CustomerId.HasValue)
            query = query.Where(r => r.CustomerId == request.CustomerId.Value);
        if (request.InvoiceId.HasValue)
            query = query.Where(r => r.InvoiceId == request.InvoiceId.Value);
        if (request.BranchId.HasValue)
            query = query.Where(r => r.BranchId == request.BranchId.Value);
        if (request.From.HasValue)
            query = query.Where(r => r.ReturnDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(r => r.ReturnDate <= request.To.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(r => r.ReturnNumber.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(r => r.ReturnDate)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<BackOfficeSalesReturnDto>.Success(
            rows.Select(BackOfficeSalesReturnMapping.Map).ToList(), page, pageSize, total);
    }
}

public sealed class GetBackOfficeSalesReturnByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBackOfficeSalesReturnByIdQuery, Result<BackOfficeSalesReturnDto>>
{
    public async Task<Result<BackOfficeSalesReturnDto>> Handle(
        GetBackOfficeSalesReturnByIdQuery request, CancellationToken cancellationToken)
    {
        var ret = await context.BackOfficeSalesReturns.AsNoTracking()
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (ret is null)
            return Result<BackOfficeSalesReturnDto>.Failure("ReturnNotFound", "Sales return not found.");
        return Result<BackOfficeSalesReturnDto>.Success(BackOfficeSalesReturnMapping.Map(ret));
    }
}

internal static class BackOfficeSalesReturnMapping
{
    public static BackOfficeSalesReturnDto Map(BackOfficeSalesReturn r) => new(
        r.Id, r.ReturnNumber, r.Status, r.CustomerId, r.WarehouseId, r.InvoiceId,
        r.BranchId, r.ReturnDate, r.Notes, r.DiscountAmount, r.SubTotal, r.TaxAmount, r.TotalAmount,
        r.JournalEntryId, r.ReversalJournalEntryId, r.ApprovedAt, r.PostedAt,
        r.Lines.Select(l => new BackOfficeSalesReturnLineDto(
            l.Id, l.InvoiceLineId, l.InventoryItemId, l.UnitId, l.LineNature,
            l.Description, l.Quantity, l.UnitPrice, l.UnitCost,
            l.TaxPercent, l.TaxAmount, l.LineNet, l.LineTotal)).ToList());
}
