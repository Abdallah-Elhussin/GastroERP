using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Sales.BackOffice;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.BackOffice.Quotations;

public sealed class CreateBackOfficeSalesQuotationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateBackOfficeSalesQuotationCommand, Result<BackOfficeSalesQuotationDto>>
{
    public async Task<Result<BackOfficeSalesQuotationDto>> Handle(
        CreateBackOfficeSalesQuotationCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var lines = dto.Lines ?? [];
        if (lines.Count == 0)
            return Result<BackOfficeSalesQuotationDto>.Failure("NoLines", "Quotation must have lines.");

        var validation = BackOfficeSalesQuotationMapping.ValidateLines(lines);
        if (validation is not null)
            return Result<BackOfficeSalesQuotationDto>.Failure(validation.Value.Code, validation.Value.Message);

        var customerExists = await context.Customers.AsNoTracking()
            .AnyAsync(c => c.Id == dto.CustomerId && c.TenantId == request.TenantId, cancellationToken);
        if (!customerExists)
            return Result<BackOfficeSalesQuotationDto>.Failure("CustomerNotFound", "Customer not found.");

        var number = string.IsNullOrWhiteSpace(dto.QuotationNumber)
            ? $"SQ-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : dto.QuotationNumber.Trim();

        try
        {
            var quotation = BackOfficeSalesQuotation.CreateDraft(
                request.TenantId, number, dto.CustomerId, dto.QuotationDate, dto.Currency,
                companyId: null, dto.BranchId, dto.WarehouseId, dto.SalesPersonId,
                dto.ValidUntil, dto.ExchangeRate, dto.Notes, dto.DiscountAmount);

            foreach (var l in lines)
                BackOfficeSalesQuotationMapping.AddLine(quotation, l);

            context.BackOfficeSalesQuotations.Add(quotation);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BackOfficeSalesQuotationDto>.Success(BackOfficeSalesQuotationMapping.Map(quotation));
        }
        catch (BusinessException ex)
        {
            return Result<BackOfficeSalesQuotationDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UpdateBackOfficeSalesQuotationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateBackOfficeSalesQuotationCommand, Result<BackOfficeSalesQuotationDto>>
{
    public async Task<Result<BackOfficeSalesQuotationDto>> Handle(
        UpdateBackOfficeSalesQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await context.BackOfficeSalesQuotations.Include(q => q.Lines)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);
        if (quotation is null)
            return Result<BackOfficeSalesQuotationDto>.Failure("QuotationNotFound", "Quotation not found.");

        var dto = request.Dto;
        var lines = dto.Lines ?? [];
        if (lines.Count == 0)
            return Result<BackOfficeSalesQuotationDto>.Failure("NoLines", "Quotation must have lines.");

        var validation = BackOfficeSalesQuotationMapping.ValidateLines(lines);
        if (validation is not null)
            return Result<BackOfficeSalesQuotationDto>.Failure(validation.Value.Code, validation.Value.Message);

        try
        {
            quotation.UpdateHeader(dto.QuotationDate, dto.WarehouseId, dto.SalesPersonId, dto.BranchId,
                dto.ValidUntil, dto.Notes, dto.DiscountAmount);
            quotation.ClearLines();
            foreach (var l in lines)
                BackOfficeSalesQuotationMapping.AddLine(quotation, l);

            context.BackOfficeSalesQuotations.Update(quotation);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BackOfficeSalesQuotationDto>.Success(BackOfficeSalesQuotationMapping.Map(quotation));
        }
        catch (BusinessException ex)
        {
            return Result<BackOfficeSalesQuotationDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ApproveBackOfficeSalesQuotationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ApproveBackOfficeSalesQuotationCommand, Result>
{
    public async Task<Result> Handle(ApproveBackOfficeSalesQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await context.BackOfficeSalesQuotations.Include(q => q.Lines)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);
        if (quotation is null)
            return Result.Failure("QuotationNotFound", "Quotation not found.");

        try
        {
            quotation.Approve(request.UserId);
            context.BackOfficeSalesQuotations.Update(quotation);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UnapproveBackOfficeSalesQuotationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UnapproveBackOfficeSalesQuotationCommand, Result>
{
    public async Task<Result> Handle(UnapproveBackOfficeSalesQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await context.BackOfficeSalesQuotations
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);
        if (quotation is null)
            return Result.Failure("QuotationNotFound", "Quotation not found.");

        try
        {
            quotation.Unapprove();
            context.BackOfficeSalesQuotations.Update(quotation);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class CancelBackOfficeSalesQuotationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelBackOfficeSalesQuotationCommand, Result>
{
    public async Task<Result> Handle(CancelBackOfficeSalesQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await context.BackOfficeSalesQuotations
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);
        if (quotation is null)
            return Result.Failure("QuotationNotFound", "Quotation not found.");

        try
        {
            quotation.Cancel();
            context.BackOfficeSalesQuotations.Update(quotation);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ConvertBackOfficeSalesQuotationToOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ConvertBackOfficeSalesQuotationToOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        ConvertBackOfficeSalesQuotationToOrderCommand request, CancellationToken cancellationToken)
    {
        var quotation = await context.BackOfficeSalesQuotations.Include(q => q.Lines)
            .FirstOrDefaultAsync(q => q.Id == request.QuotationId, cancellationToken);
        if (quotation is null)
            return Result<Guid>.Failure("QuotationNotFound", "Quotation not found.");
        if (quotation.Status != BackOfficeSalesDocumentStatus.Approved)
            return Result<Guid>.Failure(ErrorCodes.SalesInvalidStatusTransition,
                "Quotation must be approved before conversion.");
        if (quotation.ConvertedOrderId.HasValue)
            return Result<Guid>.Failure("AlreadyConverted",
                "This quotation has already been converted to a sales order.");

        var orderDate = request.OrderDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var orderNumber = string.IsNullOrWhiteSpace(request.OrderNumber)
            ? $"SO-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : request.OrderNumber.Trim();

        try
        {
            var order = BackOfficeSalesOrder.CreateDraft(
                quotation.TenantId, orderNumber, quotation.CustomerId, orderDate,
                quotation.Currency, companyId: null, quotation.BranchId, quotation.WarehouseId,
                quotation.SalesPersonId, quotation.Id, request.ExpectedDeliveryDate,
                quotation.ExchangeRate, quotation.Notes, quotation.DiscountAmount);

            foreach (var line in quotation.Lines)
            {
                order.AddLine(line.Description, line.Quantity, line.UnitPrice,
                    line.InventoryItemId, line.UnitId, line.LineNature, line.TaxPercent,
                    line.DiscountAmount, line.UnitCost);
            }

            context.BackOfficeSalesOrders.Add(order);

            order.Approve(request.UserId);
            quotation.MarkConverted(order.Id);

            context.BackOfficeSalesOrders.Update(order);
            context.BackOfficeSalesQuotations.Update(quotation);
            await context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(order.Id);
        }
        catch (BusinessException ex)
        {
            return Result<Guid>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class GetBackOfficeSalesQuotationsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBackOfficeSalesQuotationsQuery, PagedResult<BackOfficeSalesQuotationDto>>
{
    public async Task<PagedResult<BackOfficeSalesQuotationDto>> Handle(
        GetBackOfficeSalesQuotationsQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = context.BackOfficeSalesQuotations.AsNoTracking()
            .Include(q => q.Lines)
            .Where(q => q.TenantId == request.TenantId);

        if (request.Status.HasValue)
            query = query.Where(q => q.Status == request.Status.Value);
        if (request.CustomerId.HasValue)
            query = query.Where(q => q.CustomerId == request.CustomerId.Value);
        if (request.BranchId.HasValue)
            query = query.Where(q => q.BranchId == request.BranchId.Value);
        if (request.From.HasValue)
            query = query.Where(q => q.QuotationDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(q => q.QuotationDate <= request.To.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(q => q.QuotationNumber.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(q => q.QuotationDate)
            .ThenByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<BackOfficeSalesQuotationDto>.Success(
            rows.Select(BackOfficeSalesQuotationMapping.Map).ToList(), page, pageSize, total);
    }
}

public sealed class GetBackOfficeSalesQuotationByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBackOfficeSalesQuotationByIdQuery, Result<BackOfficeSalesQuotationDto>>
{
    public async Task<Result<BackOfficeSalesQuotationDto>> Handle(
        GetBackOfficeSalesQuotationByIdQuery request, CancellationToken cancellationToken)
    {
        var quotation = await context.BackOfficeSalesQuotations.AsNoTracking()
            .Include(q => q.Lines)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);
        if (quotation is null)
            return Result<BackOfficeSalesQuotationDto>.Failure("QuotationNotFound", "Quotation not found.");
        return Result<BackOfficeSalesQuotationDto>.Success(BackOfficeSalesQuotationMapping.Map(quotation));
    }
}

internal static class BackOfficeSalesQuotationMapping
{
    public static (string Code, string Message)? ValidateLines(IReadOnlyList<CreateBackOfficeSalesQuotationLineDto> lines)
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

    public static void AddLine(BackOfficeSalesQuotation q, CreateBackOfficeSalesQuotationLineDto l)
        => q.AddLine(l.Description, l.Quantity, l.UnitPrice, l.InventoryItemId, l.UnitId,
            l.LineNature, l.DiscountAmount, l.TaxPercent, l.TaxAmount, l.UnitCost);

    public static BackOfficeSalesQuotationDto Map(BackOfficeSalesQuotation q) => new(
        q.Id, q.QuotationNumber, q.Status, q.CustomerId,
        q.BranchId, q.WarehouseId, q.SalesPersonId,
        q.QuotationDate, q.ValidUntil, q.Currency, q.ExchangeRate,
        q.Notes, q.DiscountAmount, q.SubTotal, q.TaxAmount, q.TotalAmount,
        q.ConvertedOrderId, q.ApprovedAt, q.IsExpired,
        q.Lines.Select(l => new BackOfficeSalesQuotationLineDto(
            l.Id, l.InventoryItemId, l.UnitId, l.LineNature, l.Description,
            l.Quantity, l.UnitPrice, l.UnitCost, l.DiscountAmount,
            l.TaxPercent, l.TaxAmount, l.LineNet, l.LineTotal)).ToList());
}
