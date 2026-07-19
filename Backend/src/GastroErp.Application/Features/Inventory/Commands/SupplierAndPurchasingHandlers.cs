using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Inventory;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Services;
using GastroErp.Domain.Entities.Inventory.Suppliers;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Commands;

// ─── Supplier Handlers ────────────────────────────────────────────────────────
// CreateSupplier / UpsertMaster / Delete / Blacklist → SupplierMasterCommandHandlers.cs

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
        supplier.AddContact(request.Dto.NameAr, request.Dto.PhoneNumber, request.Dto.Email, request.Dto.Position, request.Dto.NameEn, request.Dto.Mobile);
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
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.Dto.SupplierId, cancellationToken);
        if (supplier is null) return Result<PurchaseOrderDto>.Failure("SupplierNotFound", "Supplier not found.");
        try { supplier.EnsureCanPurchase(); }
        catch (Domain.Common.Exceptions.BusinessException ex)
        { return Result<PurchaseOrderDto>.Failure(ex.ErrorCode, ex.Message); }

        var po = new PurchaseOrder(
            request.Dto.TenantId,
            request.Dto.SupplierId,
            request.Dto.DestinationWarehouseId,
            request.Dto.PoNumber,
            request.Dto.ExpectedDeliveryDate,
            string.IsNullOrWhiteSpace(request.Dto.Currency) ? supplier.Currency : request.Dto.Currency,
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

// ─── GoodsReceipt Handlers moved to GoodsReceiptCommandHandlers.cs ────────────

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

// ─── PurchaseReturn Handlers moved to PurchaseReturnCommandHandlers.cs ────────

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
