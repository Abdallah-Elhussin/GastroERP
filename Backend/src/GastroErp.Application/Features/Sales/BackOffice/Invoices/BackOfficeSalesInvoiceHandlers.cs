using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Sales.BackOffice;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.BackOffice.Invoices;

public sealed class CreateBackOfficeSalesInvoiceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateBackOfficeSalesInvoiceCommand, Result<BackOfficeSalesInvoiceDto>>
{
    public async Task<Result<BackOfficeSalesInvoiceDto>> Handle(
        CreateBackOfficeSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var lines = dto.Lines ?? [];
        if (lines.Count == 0)
            return Result<BackOfficeSalesInvoiceDto>.Failure("NoLines", "Invoice must have lines.");

        var validation = ValidateLines(lines, dto.Nature, dto.WarehouseId);
        if (validation is not null)
            return Result<BackOfficeSalesInvoiceDto>.Failure(validation.Value.Code, validation.Value.Message);

        var customerExists = await context.Customers.AsNoTracking()
            .AnyAsync(c => c.Id == dto.CustomerId && c.TenantId == request.TenantId, cancellationToken);
        if (!customerExists)
            return Result<BackOfficeSalesInvoiceDto>.Failure("CustomerNotFound", "Customer not found.");

        var number = string.IsNullOrWhiteSpace(dto.InvoiceNumber)
            ? $"SI-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : dto.InvoiceNumber.Trim();

        try
        {
            var invoice = BackOfficeSalesInvoice.CreateDraft(
                request.TenantId, number, dto.CustomerId, dto.InvoiceDate, dto.PaymentMode, dto.Nature,
                dto.Currency, companyId: null, dto.BranchId, dto.WarehouseId, dto.CostCenterId,
                dto.SalesPersonId, backOfficeSalesOrderId: null, dto.DueDate, dto.ExchangeRate,
                dto.ExternalReference, dto.Notes, dto.DiscountAmount);

            foreach (var line in lines)
                AddLine(invoice, line);

            context.BackOfficeSalesInvoices.Add(invoice);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BackOfficeSalesInvoiceDto>.Success(Map(invoice));
        }
        catch (BusinessException ex)
        {
            return Result<BackOfficeSalesInvoiceDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static void AddLine(BackOfficeSalesInvoice invoice, CreateBackOfficeSalesInvoiceLineDto line)
        => invoice.AddLine(
            line.InventoryItemId, line.ProductId, line.Description, line.Quantity, line.UnitPrice,
            line.LineNature, line.UnitId, line.LineWarehouseId, line.DiscountPercent, line.DiscountAmount,
            line.TaxPercent, line.TaxAmount, line.CostCenterId, line.UnitCost, line.SalesOrderLineId);

    internal static (string Code, string Message)? ValidateLines(
        IReadOnlyList<CreateBackOfficeSalesInvoiceLineDto> lines,
        BackOfficeSalesInvoiceNature nature,
        Guid? warehouseId)
    {
        if (nature == BackOfficeSalesInvoiceNature.Inventory && warehouseId is null
            && lines.Any(l => l.LineNature == BackOfficeSalesLineNature.Inventory && l.LineWarehouseId is null))
            return ("RequiredField", "Warehouse is required for inventory sales.");

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

    internal static BackOfficeSalesInvoiceDto Map(BackOfficeSalesInvoice inv) => new(
        inv.Id, inv.InvoiceNumber, inv.Nature, inv.PaymentMode, inv.Status, inv.CustomerId,
        inv.BranchId, inv.WarehouseId, inv.CostCenterId, inv.SalesPersonId, inv.BackOfficeSalesOrderId,
        inv.InvoiceDate, inv.DueDate, inv.Currency, inv.ExchangeRate, inv.ExternalReference, inv.Notes,
        inv.DiscountAmount, inv.SubTotal, inv.TaxAmount, inv.TotalAmount, inv.PaidAmount, inv.RemainingAmount,
        inv.PaymentStatus, inv.JournalEntryId, inv.CogsJournalEntryId, inv.ReversalJournalEntryId,
        inv.ApprovedAt, inv.PostedAt,
        inv.Lines.Select(l => new BackOfficeSalesInvoiceLineDto(
            l.Id, l.InventoryItemId, l.ProductId, l.UnitId, l.LineWarehouseId, l.CostCenterId, l.SalesOrderLineId,
            l.LineNature, l.Description, l.Quantity, l.UnitPrice, l.UnitCost, l.DiscountPercent, l.DiscountAmount,
            l.TaxPercent, l.TaxAmount, l.LineNet, l.LineTotal, l.ReturnedQuantity, l.RemainingToReturn)).ToList());
}

public sealed class UpdateBackOfficeSalesInvoiceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateBackOfficeSalesInvoiceCommand, Result<BackOfficeSalesInvoiceDto>>
{
    public async Task<Result<BackOfficeSalesInvoiceDto>> Handle(
        UpdateBackOfficeSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        var inv = await context.BackOfficeSalesInvoices.Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (inv is null)
            return Result<BackOfficeSalesInvoiceDto>.Failure("InvoiceNotFound", "Sales invoice not found.");

        var dto = request.Dto;
        var lines = dto.Lines ?? [];
        if (lines.Count == 0)
            return Result<BackOfficeSalesInvoiceDto>.Failure("NoLines", "Invoice must have lines.");

        var nature = dto.Nature ?? inv.Nature;
        var warehouseId = dto.WarehouseId ?? inv.WarehouseId;
        var validation = CreateBackOfficeSalesInvoiceCommandHandler.ValidateLines(lines, nature, warehouseId);
        if (validation is not null)
            return Result<BackOfficeSalesInvoiceDto>.Failure(validation.Value.Code, validation.Value.Message);

        try
        {
            inv.UpdateHeader(
                dto.InvoiceDate, dto.PaymentMode, dto.Nature, dto.DueDate, dto.WarehouseId,
                dto.CostCenterId, dto.SalesPersonId, dto.BranchId, dto.ExchangeRate,
                dto.ExternalReference, dto.Notes, dto.DiscountAmount);
            inv.ClearLines();
            foreach (var line in lines)
                CreateBackOfficeSalesInvoiceCommandHandler.AddLine(inv, line);

            context.BackOfficeSalesInvoices.Update(inv);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BackOfficeSalesInvoiceDto>.Success(CreateBackOfficeSalesInvoiceCommandHandler.Map(inv));
        }
        catch (BusinessException ex)
        {
            return Result<BackOfficeSalesInvoiceDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ApproveBackOfficeSalesInvoiceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ApproveBackOfficeSalesInvoiceCommand, Result>
{
    public async Task<Result> Handle(ApproveBackOfficeSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        var inv = await context.BackOfficeSalesInvoices.Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (inv is null)
            return Result.Failure("InvoiceNotFound", "Sales invoice not found.");

        try
        {
            inv.Approve(request.UserId);
            context.BackOfficeSalesInvoices.Update(inv);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UnapproveBackOfficeSalesInvoiceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UnapproveBackOfficeSalesInvoiceCommand, Result>
{
    public async Task<Result> Handle(UnapproveBackOfficeSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        var inv = await context.BackOfficeSalesInvoices.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (inv is null)
            return Result.Failure("InvoiceNotFound", "Sales invoice not found.");

        try
        {
            inv.Unapprove();
            context.BackOfficeSalesInvoices.Update(inv);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class PostBackOfficeSalesInvoiceCommandHandler(IBackOfficeSalesAccountingService accounting)
    : IRequestHandler<PostBackOfficeSalesInvoiceCommand, Result>
{
    public Task<Result> Handle(PostBackOfficeSalesInvoiceCommand request, CancellationToken cancellationToken)
        => accounting.PostInvoiceAsync(request.Id, request.UserId, cancellationToken);
}

public sealed class UnpostBackOfficeSalesInvoiceCommandHandler(IBackOfficeSalesAccountingService accounting)
    : IRequestHandler<UnpostBackOfficeSalesInvoiceCommand, Result>
{
    public Task<Result> Handle(UnpostBackOfficeSalesInvoiceCommand request, CancellationToken cancellationToken)
        => accounting.UnpostInvoiceAsync(request.Id, request.UserId, cancellationToken);
}

public sealed class CancelBackOfficeSalesInvoiceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelBackOfficeSalesInvoiceCommand, Result>
{
    public async Task<Result> Handle(CancelBackOfficeSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        var inv = await context.BackOfficeSalesInvoices.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (inv is null)
            return Result.Failure("InvoiceNotFound", "Sales invoice not found.");

        try
        {
            inv.Cancel();
            context.BackOfficeSalesInvoices.Update(inv);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class GetBackOfficeSalesInvoicesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBackOfficeSalesInvoicesQuery, PagedResult<BackOfficeSalesInvoiceDto>>
{
    public async Task<PagedResult<BackOfficeSalesInvoiceDto>> Handle(
        GetBackOfficeSalesInvoicesQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = context.BackOfficeSalesInvoices.AsNoTracking()
            .Include(i => i.Lines)
            .Where(i => i.TenantId == request.TenantId);

        if (request.Status.HasValue)
            query = query.Where(i => i.Status == request.Status.Value);
        if (request.CustomerId.HasValue)
            query = query.Where(i => i.CustomerId == request.CustomerId.Value);
        if (request.BranchId.HasValue)
            query = query.Where(i => i.BranchId == request.BranchId.Value);
        if (request.Nature.HasValue)
            query = query.Where(i => i.Nature == request.Nature.Value);
        if (request.PaymentMode.HasValue)
            query = query.Where(i => i.PaymentMode == request.PaymentMode.Value);
        if (request.From.HasValue)
            query = query.Where(i => i.InvoiceDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(i => i.InvoiceDate <= request.To.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(i =>
                i.InvoiceNumber.Contains(term)
                || (i.ExternalReference != null && i.ExternalReference.Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(i => i.InvoiceDate)
            .ThenByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<BackOfficeSalesInvoiceDto>.Success(
            rows.Select(CreateBackOfficeSalesInvoiceCommandHandler.Map).ToList(), page, pageSize, total);
    }
}

public sealed class GetBackOfficeSalesInvoiceByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBackOfficeSalesInvoiceByIdQuery, Result<BackOfficeSalesInvoiceDto>>
{
    public async Task<Result<BackOfficeSalesInvoiceDto>> Handle(
        GetBackOfficeSalesInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var inv = await context.BackOfficeSalesInvoices.AsNoTracking()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (inv is null)
            return Result<BackOfficeSalesInvoiceDto>.Failure("InvoiceNotFound", "Sales invoice not found.");

        return Result<BackOfficeSalesInvoiceDto>.Success(CreateBackOfficeSalesInvoiceCommandHandler.Map(inv));
    }
}
