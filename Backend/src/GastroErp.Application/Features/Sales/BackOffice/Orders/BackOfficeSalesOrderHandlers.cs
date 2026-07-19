using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Sales.BackOffice;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.BackOffice.Orders;

public sealed class CreateBackOfficeSalesOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateBackOfficeSalesOrderCommand, Result<BackOfficeSalesOrderDto>>
{
    public async Task<Result<BackOfficeSalesOrderDto>> Handle(
        CreateBackOfficeSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var lines = dto.Lines ?? [];
        if (lines.Count == 0)
            return Result<BackOfficeSalesOrderDto>.Failure("NoLines", "Order must have lines.");

        var validation = BackOfficeSalesOrderMapping.ValidateLines(lines);
        if (validation is not null)
            return Result<BackOfficeSalesOrderDto>.Failure(validation.Value.Code, validation.Value.Message);

        var customerExists = await context.Customers.AsNoTracking()
            .AnyAsync(c => c.Id == dto.CustomerId && c.TenantId == request.TenantId, cancellationToken);
        if (!customerExists)
            return Result<BackOfficeSalesOrderDto>.Failure("CustomerNotFound", "Customer not found.");

        var number = string.IsNullOrWhiteSpace(dto.OrderNumber)
            ? $"SO-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : dto.OrderNumber.Trim();

        try
        {
            var order = BackOfficeSalesOrder.CreateDraft(
                request.TenantId, number, dto.CustomerId, dto.OrderDate, dto.Currency,
                companyId: null, dto.BranchId, dto.WarehouseId, dto.SalesPersonId, dto.QuotationId,
                dto.ExpectedDeliveryDate, dto.ExchangeRate, dto.Notes, dto.DiscountAmount);

            foreach (var l in lines)
                BackOfficeSalesOrderMapping.AddLine(order, l);

            context.BackOfficeSalesOrders.Add(order);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BackOfficeSalesOrderDto>.Success(BackOfficeSalesOrderMapping.Map(order));
        }
        catch (BusinessException ex)
        {
            return Result<BackOfficeSalesOrderDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UpdateBackOfficeSalesOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateBackOfficeSalesOrderCommand, Result<BackOfficeSalesOrderDto>>
{
    public async Task<Result<BackOfficeSalesOrderDto>> Handle(
        UpdateBackOfficeSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.BackOfficeSalesOrders.Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order is null)
            return Result<BackOfficeSalesOrderDto>.Failure("OrderNotFound", "Sales order not found.");

        var dto = request.Dto;
        var lines = dto.Lines ?? [];
        if (lines.Count == 0)
            return Result<BackOfficeSalesOrderDto>.Failure("NoLines", "Order must have lines.");

        var validation = BackOfficeSalesOrderMapping.ValidateLines(lines);
        if (validation is not null)
            return Result<BackOfficeSalesOrderDto>.Failure(validation.Value.Code, validation.Value.Message);

        try
        {
            order.UpdateHeader(dto.OrderDate, dto.WarehouseId, dto.SalesPersonId, dto.BranchId,
                dto.ExpectedDeliveryDate, dto.Notes, dto.DiscountAmount);
            order.ClearLines();
            foreach (var l in lines)
                BackOfficeSalesOrderMapping.AddLine(order, l);

            context.BackOfficeSalesOrders.Update(order);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BackOfficeSalesOrderDto>.Success(BackOfficeSalesOrderMapping.Map(order));
        }
        catch (BusinessException ex)
        {
            return Result<BackOfficeSalesOrderDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ApproveBackOfficeSalesOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ApproveBackOfficeSalesOrderCommand, Result>
{
    public async Task<Result> Handle(ApproveBackOfficeSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.BackOfficeSalesOrders.Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order is null)
            return Result.Failure("OrderNotFound", "Sales order not found.");

        try
        {
            order.Approve(request.UserId);
            context.BackOfficeSalesOrders.Update(order);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UnapproveBackOfficeSalesOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UnapproveBackOfficeSalesOrderCommand, Result>
{
    public async Task<Result> Handle(UnapproveBackOfficeSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.BackOfficeSalesOrders
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order is null)
            return Result.Failure("OrderNotFound", "Sales order not found.");

        try
        {
            order.Unapprove();
            context.BackOfficeSalesOrders.Update(order);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class CancelBackOfficeSalesOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelBackOfficeSalesOrderCommand, Result>
{
    public async Task<Result> Handle(CancelBackOfficeSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.BackOfficeSalesOrders.Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order is null)
            return Result.Failure("OrderNotFound", "Sales order not found.");

        try
        {
            order.Cancel();
            context.BackOfficeSalesOrders.Update(order);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class CloseBackOfficeSalesOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CloseBackOfficeSalesOrderCommand, Result>
{
    public async Task<Result> Handle(CloseBackOfficeSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.BackOfficeSalesOrders
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order is null)
            return Result.Failure("OrderNotFound", "Sales order not found.");

        try
        {
            order.Close();
            context.BackOfficeSalesOrders.Update(order);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class GetBackOfficeSalesOrdersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBackOfficeSalesOrdersQuery, PagedResult<BackOfficeSalesOrderDto>>
{
    public async Task<PagedResult<BackOfficeSalesOrderDto>> Handle(
        GetBackOfficeSalesOrdersQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = context.BackOfficeSalesOrders.AsNoTracking()
            .Include(o => o.Lines)
            .Where(o => o.TenantId == request.TenantId);

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);
        if (request.FulfillmentStatus.HasValue)
            query = query.Where(o => o.FulfillmentStatus == request.FulfillmentStatus.Value);
        if (request.CustomerId.HasValue)
            query = query.Where(o => o.CustomerId == request.CustomerId.Value);
        if (request.BranchId.HasValue)
            query = query.Where(o => o.BranchId == request.BranchId.Value);
        if (request.From.HasValue)
            query = query.Where(o => o.OrderDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(o => o.OrderDate <= request.To.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(o => o.OrderNumber.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<BackOfficeSalesOrderDto>.Success(
            rows.Select(BackOfficeSalesOrderMapping.Map).ToList(), page, pageSize, total);
    }
}

public sealed class GetBackOfficeSalesOrderByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBackOfficeSalesOrderByIdQuery, Result<BackOfficeSalesOrderDto>>
{
    public async Task<Result<BackOfficeSalesOrderDto>> Handle(
        GetBackOfficeSalesOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await context.BackOfficeSalesOrders.AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order is null)
            return Result<BackOfficeSalesOrderDto>.Failure("OrderNotFound", "Sales order not found.");
        return Result<BackOfficeSalesOrderDto>.Success(BackOfficeSalesOrderMapping.Map(order));
    }
}

internal static class BackOfficeSalesOrderMapping
{
    public static (string Code, string Message)? ValidateLines(IReadOnlyList<CreateBackOfficeSalesOrderLineDto> lines)
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

    public static void AddLine(BackOfficeSalesOrder order, CreateBackOfficeSalesOrderLineDto l)
        => order.AddLine(l.Description, l.Quantity, l.UnitPrice, l.InventoryItemId, l.UnitId,
            l.LineNature, l.TaxPercent, l.DiscountAmount, l.UnitCost);

    public static BackOfficeSalesOrderDto Map(BackOfficeSalesOrder o) => new(
        o.Id, o.OrderNumber, o.Status, o.FulfillmentStatus, o.CustomerId,
        o.BranchId, o.WarehouseId, o.SalesPersonId, o.QuotationId,
        o.OrderDate, o.ExpectedDeliveryDate, o.Currency, o.ExchangeRate,
        o.Notes, o.DiscountAmount, o.SubTotal, o.TaxAmount, o.TotalAmount, o.ApprovedAt,
        o.Lines.Select(l => new BackOfficeSalesOrderLineDto(
            l.Id, l.InventoryItemId, l.UnitId, l.LineNature, l.Description,
            l.Quantity, l.UnitPrice, l.UnitCost, l.DiscountAmount,
            l.TaxPercent, l.TaxAmount, l.LineNet,
            l.DeliveredQuantity, l.InvoicedQuantity,
            l.RemainingToDeliver, l.RemainingToInvoice)).ToList());
}
