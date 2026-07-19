using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Mapping;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

public sealed class CreatePurchaseOrderCommandHandler(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    ILogger<CreatePurchaseOrderCommandHandler> logger)
    : IRequestHandler<CreatePurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    public async Task<Result<PurchaseOrderDto>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var tenantId = currentUser.TenantId;
        if (tenantId == Guid.Empty)
            return Result<PurchaseOrderDto>.Failure("TenantRequired", "Tenant is required.");

        var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == dto.SupplierId, cancellationToken);
        if (supplier is null)
            return Result<PurchaseOrderDto>.Failure("SupplierNotFound", "Supplier not found.");

        try { supplier.EnsureCanPurchase(); }
        catch (BusinessException ex)
        { return Result<PurchaseOrderDto>.Failure(ex.ErrorCode, ex.Message); }

        var warehouseOk = await context.Warehouses.AnyAsync(w => w.Id == dto.DestinationWarehouseId, cancellationToken);
        if (!warehouseOk)
            return Result<PurchaseOrderDto>.Failure("WarehouseNotFound", "Warehouse not found.");

        try
        {
            var number = string.IsNullOrWhiteSpace(dto.PoNumber)
                ? await NextPoNumberAsync(context, tenantId, cancellationToken)
                : dto.PoNumber.Trim();

            var po = new PurchaseOrder(
                tenantId,
                dto.SupplierId,
                dto.DestinationWarehouseId,
                number,
                dto.OrderDate,
                dto.ExpectedDeliveryDate,
                string.IsNullOrWhiteSpace(dto.Currency) ? supplier.Currency : dto.Currency!,
                dto.ExchangeRate <= 0 ? 1 : dto.ExchangeRate,
                dto.OrderType,
                dto.BranchId,
                dto.CostCenterId,
                dto.ResponsibleEmployeeId,
                dto.PaymentMethod,
                dto.PaymentTerms,
                dto.ExternalReference,
                dto.Notes);

            if (dto.Lines is { Count: > 0 })
            {
                po.ReplaceLines(dto.Lines.Select(l => (
                    l.InventoryItemId, l.UnitId, l.Quantity, l.UnitPrice,
                    l.DiscountAmount, l.TaxAmount, l.Description, l.WarehouseId, l.LineNotes)));
            }

            context.PurchaseOrders.Add(po);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("PurchaseOrder created: {Id} {Number}", po.Id, po.PoNumber);
            return Result<PurchaseOrderDto>.Success(await PurchaseOrderMapper.ToDtoAsync(context, po, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<PurchaseOrderDto>.Failure(ex.ErrorCode, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<PurchaseOrderDto>.Failure("Validation", ex.Message);
        }
    }

    internal static async Task<string> NextPoNumberAsync(
        IApplicationDbContext context, Guid tenantId, CancellationToken cancellationToken)
    {
        var prefix = $"PO-{DateTime.UtcNow:yyyyMMdd}-";
        var count = await context.PurchaseOrders
            .IgnoreQueryFilters()
            .CountAsync(p => p.TenantId == tenantId && p.PoNumber.StartsWith(prefix), cancellationToken);
        return $"{prefix}{(count + 1):D4}";
    }
}

public sealed class UpdatePurchaseOrderCommandHandler(
    IApplicationDbContext context,
    ILogger<UpdatePurchaseOrderCommandHandler> logger)
    : IRequestHandler<UpdatePurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    public async Task<Result<PurchaseOrderDto>> Handle(UpdatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po is null)
            return Result<PurchaseOrderDto>.Failure("PurchaseOrderNotFound", "Purchase order not found.");

        var dto = request.Dto;
        try
        {
            po.UpdateHeader(
                dto.SupplierId, dto.DestinationWarehouseId, dto.OrderDate, dto.ExpectedDeliveryDate,
                dto.Currency, dto.ExchangeRate <= 0 ? 1 : dto.ExchangeRate, dto.OrderType,
                dto.BranchId, dto.CostCenterId, dto.ResponsibleEmployeeId,
                dto.PaymentMethod, dto.PaymentTerms, dto.ExternalReference, dto.Notes);

            po.ReplaceLines((dto.Lines ?? []).Select(l => (
                l.InventoryItemId, l.UnitId, l.Quantity, l.UnitPrice,
                l.DiscountAmount, l.TaxAmount, l.Description, l.WarehouseId, l.LineNotes)));

            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("PurchaseOrder updated: {Id}", po.Id);
            return Result<PurchaseOrderDto>.Success(await PurchaseOrderMapper.ToDtoAsync(context, po, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<PurchaseOrderDto>.Failure(ex.ErrorCode, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result<PurchaseOrderDto>.Failure("Validation", ex.Message);
        }
    }
}

public sealed class DeletePurchaseOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeletePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(DeletePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po is null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        try
        {
            po.MarkDeleted();
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class CopyPurchaseOrderCommandHandler(
    IApplicationDbContext context,
    ILogger<CopyPurchaseOrderCommandHandler> logger)
    : IRequestHandler<CopyPurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    public async Task<Result<PurchaseOrderDto>> Handle(CopyPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var source = await context.PurchaseOrders.AsNoTracking().Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (source is null)
            return Result<PurchaseOrderDto>.Failure("PurchaseOrderNotFound", "Purchase order not found.");

        var number = await CreatePurchaseOrderCommandHandler.NextPoNumberAsync(context, source.TenantId, cancellationToken);
        var clone = source.CloneAsDraft(number);
        context.PurchaseOrders.Add(clone);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("PurchaseOrder copied: {Source} -> {Id}", source.Id, clone.Id);
        return Result<PurchaseOrderDto>.Success(await PurchaseOrderMapper.ToDtoAsync(context, clone, cancellationToken));
    }
}

public sealed class AddPurchaseOrderLineCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AddPurchaseOrderLineCommand, Result>
{
    public async Task<Result> Handle(AddPurchaseOrderLineCommand request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, cancellationToken);
        if (po is null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        try
        {
            po.AddLine(
                request.Dto.InventoryItemId, request.Dto.UnitId, request.Dto.Quantity,
                request.Dto.UnitPrice, taxAmount: request.Dto.TaxAmount);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex) { return Result.Failure(ex.ErrorCode, ex.Message); }
        catch (ArgumentException ex) { return Result.Failure("Validation", ex.Message); }
    }
}

public sealed class RemovePurchaseOrderLineCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RemovePurchaseOrderLineCommand, Result>
{
    public async Task<Result> Handle(RemovePurchaseOrderLineCommand request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, cancellationToken);
        if (po is null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        try
        {
            po.RemoveLine(request.LineId);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex) { return Result.Failure(ex.ErrorCode, ex.Message); }
    }
}

public sealed class ApprovePurchaseOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ApprovePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po is null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        try
        {
            po.Approve();
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex) { return Result.Failure(ex.ErrorCode, ex.Message); }
    }
}

public sealed class SubmitPurchaseOrderForApprovalCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SubmitPurchaseOrderForApprovalCommand, Result>
{
    public async Task<Result> Handle(SubmitPurchaseOrderForApprovalCommand request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po is null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        try
        {
            po.SubmitForApproval();
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex) { return Result.Failure(ex.ErrorCode, ex.Message); }
    }
}

public sealed class SendPurchaseOrderToSupplierCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SendPurchaseOrderToSupplierCommand, Result>
{
    public async Task<Result> Handle(SendPurchaseOrderToSupplierCommand request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po is null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        try
        {
            po.MarkAsSent();
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex) { return Result.Failure(ex.ErrorCode, ex.Message); }
    }
}

public sealed class CancelPurchaseOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelPurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po is null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        try
        {
            po.Cancel();
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex) { return Result.Failure(ex.ErrorCode, ex.Message); }
    }
}

public sealed class RejectPurchaseOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RejectPurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(RejectPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po is null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        try
        {
            po.Reject();
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex) { return Result.Failure(ex.ErrorCode, ex.Message); }
    }
}

public sealed class ClosePurchaseOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ClosePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(ClosePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po is null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        try
        {
            po.Close();
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex) { return Result.Failure(ex.ErrorCode, ex.Message); }
    }
}
