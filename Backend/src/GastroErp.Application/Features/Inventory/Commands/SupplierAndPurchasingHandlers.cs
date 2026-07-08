using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Entities.Inventory.Suppliers;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

// ─── Supplier Handlers ────────────────────────────────────────────────────────

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, Result<SupplierDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSupplierCommandHandler> _logger;

    public CreateSupplierCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateSupplierCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<SupplierDto>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = new Supplier(request.Dto.TenantId, request.Dto.NameAr, request.Dto.NameEn, request.Dto.Currency);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Supplier created: {Id}", supplier.Id);
        return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(supplier));
    }
}

public class UpdateSupplierFinancialCommandHandler : IRequestHandler<UpdateSupplierFinancialCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateSupplierFinancialCommandHandler> _logger;

    public UpdateSupplierFinancialCommandHandler(IApplicationDbContext context, ILogger<UpdateSupplierFinancialCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(UpdateSupplierFinancialCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (supplier == null) return Result.Failure("SupplierNotFound", "Supplier not found.");
        supplier.UpdateFinancialInfo(request.Dto.TaxNumber, request.Dto.PaymentTerms, request.Dto.CreditLimit, request.Dto.LeadTimeDays);
        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Supplier financial info updated: {Id}", supplier.Id);
        return Result.Success();
    }
}

public class SetSupplierRatingCommandHandler : IRequestHandler<SetSupplierRatingCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetSupplierRatingCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SetSupplierRatingCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (supplier == null) return Result.Failure("SupplierNotFound", "Supplier not found.");
        supplier.SetRating(request.Rating);
        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SetSupplierPreferredCommandHandler : IRequestHandler<SetSupplierPreferredCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetSupplierPreferredCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SetSupplierPreferredCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (supplier == null) return Result.Failure("SupplierNotFound", "Supplier not found.");
        supplier.SetPreferred(request.IsPreferred);
        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class AddSupplierContactCommandHandler : IRequestHandler<AddSupplierContactCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AddSupplierContactCommandHandler> _logger;

    public AddSupplierContactCommandHandler(IApplicationDbContext context, ILogger<AddSupplierContactCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(AddSupplierContactCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers.Include(s => s.Contacts).FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken);
        if (supplier == null) return Result.Failure("SupplierNotFound", "Supplier not found.");
        supplier.AddContact(request.Dto.NameAr, request.Dto.PhoneNumber, request.Dto.Email, request.Dto.Position, request.Dto.NameEn);
        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Contact added to Supplier {SupplierId}", supplier.Id);
        return Result.Success();
    }
}

public class RemoveSupplierContactCommandHandler : IRequestHandler<RemoveSupplierContactCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveSupplierContactCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveSupplierContactCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers.Include(s => s.Contacts).FirstOrDefaultAsync(s => s.Id == request.SupplierId, cancellationToken);
        if (supplier == null) return Result.Failure("SupplierNotFound", "Supplier not found.");
        supplier.RemoveContact(request.ContactId);
        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeactivateSupplierCommandHandler : IRequestHandler<DeactivateSupplierCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeactivateSupplierCommandHandler> _logger;

    public DeactivateSupplierCommandHandler(IApplicationDbContext context, ILogger<DeactivateSupplierCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(DeactivateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (supplier == null) return Result.Failure("SupplierNotFound", "Supplier not found.");
        supplier.Deactivate();
        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Supplier deactivated: {Id}", supplier.Id);
        return Result.Success();
    }
}

public class ActivateSupplierCommandHandler : IRequestHandler<ActivateSupplierCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateSupplierCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ActivateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (supplier == null) return Result.Failure("SupplierNotFound", "Supplier not found.");
        supplier.Activate();
        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── PurchaseOrder Handlers ───────────────────────────────────────────────────

public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePurchaseOrderCommandHandler> _logger;

    public CreatePurchaseOrderCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreatePurchaseOrderCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<PurchaseOrderDto>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == request.Dto.SupplierId, cancellationToken);
        if (!supplierExists) return Result<PurchaseOrderDto>.Failure("SupplierNotFound", "Supplier not found.");

        var po = new PurchaseOrder(
            request.Dto.TenantId,
            request.Dto.SupplierId,
            request.Dto.DestinationWarehouseId,
            request.Dto.PoNumber,
            request.Dto.ExpectedDeliveryDate,
            request.Dto.Currency,
            request.Dto.Notes
        );

        _context.PurchaseOrders.Add(po);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("PurchaseOrder created: {Id}", po.Id);
        return Result<PurchaseOrderDto>.Success(_mapper.Map<PurchaseOrderDto>(po));
    }
}

public class AddPurchaseOrderLineCommandHandler : IRequestHandler<AddPurchaseOrderLineCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AddPurchaseOrderLineCommandHandler> _logger;

    public AddPurchaseOrderLineCommandHandler(IApplicationDbContext context, ILogger<AddPurchaseOrderLineCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(AddPurchaseOrderLineCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders.Include(p => p.Lines).FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, cancellationToken);
        if (po == null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");

        po.AddLine(request.Dto.InventoryItemId, request.Dto.UnitId, request.Dto.Quantity, request.Dto.UnitPrice, request.Dto.TaxAmount);
        _context.PurchaseOrders.Update(po);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Line added to PurchaseOrder {PoId}", po.Id);
        return Result.Success();
    }
}

public class RemovePurchaseOrderLineCommandHandler : IRequestHandler<RemovePurchaseOrderLineCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemovePurchaseOrderLineCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemovePurchaseOrderLineCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders.Include(p => p.Lines).FirstOrDefaultAsync(p => p.Id == request.PurchaseOrderId, cancellationToken);
        if (po == null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        po.RemoveLine(request.LineId);
        _context.PurchaseOrders.Update(po);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ApprovePurchaseOrderCommandHandler : IRequestHandler<ApprovePurchaseOrderCommand, Result>
{
    public Task<Result> Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
        => Task.FromResult(Result.Failure("WorkflowRequired",
            "Purchase order approval must go through POST /api/v1/workflow/instances/approve."));
}

public class SubmitPurchaseOrderForApprovalCommandHandler : IRequestHandler<SubmitPurchaseOrderForApprovalCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SubmitPurchaseOrderForApprovalCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SubmitPurchaseOrderForApprovalCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po is null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        po.SubmitForApproval();
        _context.PurchaseOrders.Update(po);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SendPurchaseOrderToSupplierCommandHandler : IRequestHandler<SendPurchaseOrderToSupplierCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SendPurchaseOrderToSupplierCommandHandler> _logger;

    public SendPurchaseOrderToSupplierCommandHandler(IApplicationDbContext context, ILogger<SendPurchaseOrderToSupplierCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(SendPurchaseOrderToSupplierCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po == null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        po.MarkAsSent();
        _context.PurchaseOrders.Update(po);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("PurchaseOrder sent to supplier: {Id}", po.Id);
        return Result.Success();
    }
}

public class CancelPurchaseOrderCommandHandler : IRequestHandler<CancelPurchaseOrderCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CancelPurchaseOrderCommandHandler> _logger;

    public CancelPurchaseOrderCommandHandler(IApplicationDbContext context, ILogger<CancelPurchaseOrderCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po == null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        po.Cancel();
        _context.PurchaseOrders.Update(po);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogWarning("PurchaseOrder cancelled: {Id}", po.Id);
        return Result.Success();
    }
}

// ─── GoodsReceipt Handlers ────────────────────────────────────────────────────

public class CreateGoodsReceiptCommandHandler : IRequestHandler<CreateGoodsReceiptCommand, Result<GoodsReceiptDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateGoodsReceiptCommandHandler> _logger;

    public CreateGoodsReceiptCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateGoodsReceiptCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<GoodsReceiptDto>> Handle(CreateGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == request.Dto.PurchaseOrderId, cancellationToken);
        if (po == null) return Result<GoodsReceiptDto>.Failure("PurchaseOrderNotFound", "Purchase order not found.");

        var gr = new GoodsReceipt(
            request.Dto.TenantId,
            po.SupplierId,
            request.Dto.WarehouseId,
            request.Dto.GrnNumber,
            request.Dto.PurchaseOrderId,
            null,
            request.Dto.Notes
        );

        _context.GoodsReceipts.Add(gr);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("GoodsReceipt created: {Id}", gr.Id);
        return Result<GoodsReceiptDto>.Success(_mapper.Map<GoodsReceiptDto>(gr));
    }
}

public class AddGoodsReceiptLineCommandHandler : IRequestHandler<AddGoodsReceiptLineCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AddGoodsReceiptLineCommandHandler> _logger;

    public AddGoodsReceiptLineCommandHandler(IApplicationDbContext context, ILogger<AddGoodsReceiptLineCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(AddGoodsReceiptLineCommand request, CancellationToken cancellationToken)
    {
        var gr = await _context.GoodsReceipts.Include(g => g.Lines).FirstOrDefaultAsync(g => g.Id == request.GoodsReceiptId, cancellationToken);
        if (gr == null) return Result.Failure("GoodsReceiptNotFound", "Goods receipt not found.");

        gr.AddLine(request.Dto.InventoryItemId, request.Dto.UnitId, request.Dto.ReceivedQuantity, request.Dto.UnitCost);
        _context.GoodsReceipts.Update(gr);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Line added to GoodsReceipt {GrId}", gr.Id);
        return Result.Success();
    }
}

public class ConfirmGoodsReceiptCommandHandler : IRequestHandler<ConfirmGoodsReceiptCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ConfirmGoodsReceiptCommandHandler> _logger;

    public ConfirmGoodsReceiptCommandHandler(IApplicationDbContext context, ILogger<ConfirmGoodsReceiptCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(ConfirmGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        var gr = await _context.GoodsReceipts.Include(g => g.Lines).FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (gr == null) return Result.Failure("GoodsReceiptNotFound", "Goods receipt not found.");
        if (!gr.Lines.Any()) return Result.Failure("NoLines", "Cannot confirm goods receipt with no lines.");

        gr.Complete();
        _context.GoodsReceipts.Update(gr);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("GoodsReceipt confirmed: {Id}", gr.Id);
        return Result.Success();
    }
}

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateSupplierCommandHandler> _logger;

    public UpdateSupplierCommandHandler(IApplicationDbContext context, ILogger<UpdateSupplierCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (supplier == null) return Result.Failure("SupplierNotFound", "Supplier not found.");

        supplier.UpdateInfo(request.Dto.NameAr, request.Dto.NameEn, request.Dto.Currency);

        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Supplier updated: {Id}", supplier.Id);
        return Result.Success();
    }
}

// ─── PurchaseReturn Handlers ──────────────────────────────────────────────────

public class CreatePurchaseReturnCommandHandler : IRequestHandler<CreatePurchaseReturnCommand, Result<PurchaseReturnDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePurchaseReturnCommandHandler> _logger;

    public CreatePurchaseReturnCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreatePurchaseReturnCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<PurchaseReturnDto>> Handle(CreatePurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == request.Dto.SupplierId, cancellationToken);
        if (!supplierExists) return Result<PurchaseReturnDto>.Failure("SupplierNotFound", "Supplier not found.");

        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == request.Dto.WarehouseId, cancellationToken);
        if (!warehouseExists) return Result<PurchaseReturnDto>.Failure("WarehouseNotFound", "Warehouse not found.");

        var purchaseReturn = new PurchaseReturn(
            request.TenantId,
            request.Dto.SupplierId,
            request.Dto.WarehouseId,
            request.Dto.ReturnNumber,
            request.Dto.GoodsReceiptId,
            request.Dto.Reason
        );

        _context.PurchaseReturns.Add(purchaseReturn);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("PurchaseReturn created: {Id}", purchaseReturn.Id);
        return Result<PurchaseReturnDto>.Success(_mapper.Map<PurchaseReturnDto>(purchaseReturn));
    }
}

public class AddPurchaseReturnLineCommandHandler : IRequestHandler<AddPurchaseReturnLineCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AddPurchaseReturnLineCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(AddPurchaseReturnLineCommand request, CancellationToken cancellationToken)
    {
        var purchaseReturn = await _context.PurchaseReturns.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == request.PurchaseReturnId, cancellationToken);
        if (purchaseReturn == null) return Result.Failure("PurchaseReturnNotFound", "Purchase return not found.");

        purchaseReturn.AddLine(request.Dto.InventoryItemId, request.Dto.UnitId, request.Dto.ReturnQuantity, request.Dto.UnitCost, request.Dto.Notes);
        _context.PurchaseReturns.Update(purchaseReturn);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ApprovePurchaseReturnCommandHandler : IRequestHandler<ApprovePurchaseReturnCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ApprovePurchaseReturnCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ApprovePurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        var purchaseReturn = await _context.PurchaseReturns.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (purchaseReturn == null) return Result.Failure("PurchaseReturnNotFound", "Purchase return not found.");

        purchaseReturn.Complete();
        _context.PurchaseReturns.Update(purchaseReturn);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── Additional PurchaseOrder Workflow Handlers ─────────────────────────────────

public class RejectPurchaseOrderCommandHandler : IRequestHandler<RejectPurchaseOrderCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RejectPurchaseOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RejectPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po == null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");

        po.Reject();
        _context.PurchaseOrders.Update(po);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ClosePurchaseOrderCommandHandler : IRequestHandler<ClosePurchaseOrderCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ClosePurchaseOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ClosePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po == null) return Result.Failure("PurchaseOrderNotFound", "Purchase order not found.");

        po.Close();
        _context.PurchaseOrders.Update(po);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
