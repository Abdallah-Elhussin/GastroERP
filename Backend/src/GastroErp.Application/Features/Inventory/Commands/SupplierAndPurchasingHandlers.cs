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

// ─── PurchaseOrder Handlers moved to PurchaseOrderCommandHandlers.cs ──────────

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
