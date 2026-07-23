using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.Services;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Commands;

public class CreatePurchaseInvoiceCommandHandler
    : IRequestHandler<CreatePurchaseInvoiceCommand, Result<PurchaseInvoiceDto>>
{
    private readonly IApplicationDbContext _context;

    public CreatePurchaseInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<PurchaseInvoiceDto>> Handle(
        CreatePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        IReadOnlyList<CreatePurchaseInvoiceLineDto> lines = dto.Lines ?? [];

        if (dto.Kind == PurchaseInvoiceKind.FromReceipt)
        {
            if (!dto.GoodsReceiptId.HasValue)
                return Result<PurchaseInvoiceDto>.Failure("RequiredField", "Goods receipt is required.");

            var gr = await _context.GoodsReceipts
                .Include(g => g.Lines)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == dto.GoodsReceiptId.Value, cancellationToken);

            if (gr is null || gr.Status != GoodsReceiptStatus.Posted)
                return Result<PurchaseInvoiceDto>.Failure("InvalidReceipt", "Invoice requires a posted goods receipt.");

            lines = gr.Lines
                .Where(l => l.RemainingToInvoice > 0)
                .Select(l => new CreatePurchaseInvoiceLineDto(
                    l.InventoryItemId,
                    l.UnitId,
                    l.RemainingToInvoice,
                    l.UnitCost,
                    TaxAmount: 0,
                    GoodsReceiptLineId: l.Id,
                    PurchaseOrderLineId: l.PurchaseOrderLineId))
                .ToList();

            if (lines.Count == 0)
                return Result<PurchaseInvoiceDto>.Failure("NothingToInvoice", "All received quantities are already invoiced.");

            dto = dto with
            {
                SupplierId = gr.SupplierId,
                PurchaseOrderId = gr.PurchaseOrderId ?? dto.PurchaseOrderId,
                WarehouseId = gr.WarehouseId,
                Nature = DirectPurchaseNature.Inventory,
                Lines = lines
            };
        }
        else if (lines.Count == 0)
        {
            return Result<PurchaseInvoiceDto>.Failure("NoLines", "Invoice must have lines.");
        }

        if (dto.Kind == PurchaseInvoiceKind.Direct)
        {
            var validation = ValidateDirectLines(lines, dto.Nature, dto.WarehouseId);
            if (validation is not null)
                return Result<PurchaseInvoiceDto>.Failure(validation.Value.Code, validation.Value.Message);

            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == dto.SupplierId, cancellationToken);
            if (supplier is null)
                return Result<PurchaseInvoiceDto>.Failure("SupplierNotFound", "Supplier not found.");
            try { supplier.EnsureCanPurchase(); }
            catch (Domain.Common.Exceptions.BusinessException ex)
            { return Result<PurchaseInvoiceDto>.Failure(ex.ErrorCode, ex.Message); }

            if (string.IsNullOrWhiteSpace(dto.Currency) || dto.Currency == "SAR")
                dto = dto with { Currency = supplier.Currency };
            if (!dto.DueDate.HasValue && supplier.PaymentDueDays > 0)
                dto = dto with { DueDate = dto.InvoiceDate.AddDays(supplier.PaymentDueDays) };
            if (dto.PaymentMode == default)
                dto = dto with
                {
                    PaymentMode = supplier.DefaultPaymentMethod is Domain.Enums.SupplierPaymentMethodKind.Cash
                        ? PurchaseInvoicePaymentMode.Cash
                        : PurchaseInvoicePaymentMode.Credit
                };
        }

        var number = await PurchaseInvoiceNumberAllocator.PeekNextAsync(
            _context, request.TenantId, dto.Kind, cancellationToken);
        if (!string.IsNullOrWhiteSpace(dto.InvoiceNumber))
        {
            var requested = dto.InvoiceNumber.Trim().ToUpperInvariant();
            // Accept client preview only when it is still free; otherwise keep server allocation.
            if (!await _context.PurchaseInvoices.AsNoTracking()
                    .AnyAsync(i => i.TenantId == request.TenantId && i.InvoiceNumber == requested, cancellationToken))
            {
                number = requested;
            }
        }

        var invoice = PurchaseInvoice.CreateDraft(
            request.TenantId,
            number,
            dto.Kind,
            dto.PaymentMode,
            dto.SupplierId,
            dto.InvoiceDate,
            dto.Currency,
            companyId: null,
            branchId: dto.BranchId,
            purchaseOrderId: dto.PurchaseOrderId,
            goodsReceiptId: dto.GoodsReceiptId,
            warehouseId: dto.WarehouseId,
            dueDate: dto.DueDate,
            supplierInvoiceNumber: dto.SupplierInvoiceNumber,
            notes: dto.Notes,
            nature: dto.Nature,
            exchangeRate: dto.ExchangeRate,
            externalReference: dto.ExternalReference,
            costCenterId: dto.CostCenterId,
            discountAmount: dto.DiscountAmount);

        foreach (var line in lines)
            AddLine(invoice, line);

        _context.PurchaseInvoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<PurchaseInvoiceDto>.Success(Map(invoice));
    }

    internal static void AddLine(PurchaseInvoice invoice, CreatePurchaseInvoiceLineDto line)
        => invoice.AddLine(
            line.InventoryItemId, line.UnitId, line.Quantity, line.UnitPrice, line.TaxAmount,
            line.GoodsReceiptLineId, line.PurchaseOrderLineId, line.Description,
            line.DiscountPercent, line.DiscountAmount, line.TaxPercent,
            line.BatchNumber, line.SerialNumber, line.ProductionDate, line.ExpiryDate,
            line.LineWarehouseId, line.CostCenterId);

    internal static (string Code, string Message)? ValidateDirectLines(
        IReadOnlyList<CreatePurchaseInvoiceLineDto> lines,
        DirectPurchaseNature nature,
        Guid? warehouseId)
    {
        if (lines.Count == 0)
            return ("NoLines", "Invoice must have lines.");

        if (nature == DirectPurchaseNature.Inventory && warehouseId is null
            && lines.Any(l => l.LineWarehouseId is null))
            return ("RequiredField", "Warehouse is required for inventory purchases.");

        if (lines.GroupBy(l => l.InventoryItemId).Any(g => g.Count() > 1))
            return ("DuplicateItem", "Duplicate items are not allowed on the same invoice.");

        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                return (ErrorCodes.InvalidQuantity, "Quantity must be greater than zero.");
            if (line.UnitPrice <= 0)
                return (ErrorCodes.InvalidAmount, "Unit price must be greater than zero.");
            if (line.TaxPercent is < 0 or > 100)
                return (ErrorCodes.InvalidAmount, "Tax percent is invalid.");
            if (line.DiscountPercent is < 0 or > 100)
                return (ErrorCodes.InvalidAmount, "Discount percent is invalid.");
        }

        return null;
    }

    internal static PurchaseInvoiceDto Map(PurchaseInvoice inv) => new(
        inv.Id, inv.InvoiceNumber, inv.Kind, inv.PaymentMode, inv.Nature, inv.Status, inv.SupplierId,
        inv.BranchId, inv.PurchaseOrderId, inv.GoodsReceiptId, inv.WarehouseId, inv.CostCenterId,
        inv.InvoiceDate, inv.DueDate, inv.Currency, inv.ExchangeRate, inv.SupplierInvoiceNumber,
        inv.ExternalReference, inv.Notes, inv.DiscountAmount, inv.SubTotal, inv.TaxAmount, inv.TotalAmount,
        inv.PaidAmount, inv.RemainingAmount, inv.PaymentStatus, inv.JournalEntryId, inv.ReversalJournalEntryId,
        inv.PostedAt,
        inv.Lines.Select(l => new PurchaseInvoiceLineDto(
            l.Id, l.InventoryItemId, l.UnitId, l.Quantity, l.UnitPrice,
            l.DiscountPercent, l.DiscountAmount, l.TaxPercent, l.TaxAmount, l.LineNet, l.LineTotal,
            l.GoodsReceiptLineId, l.PurchaseOrderLineId, l.LineWarehouseId, l.CostCenterId,
            l.BatchNumber, l.SerialNumber, l.ProductionDate, l.ExpiryDate, l.Description,
            l.ReturnedQuantity, l.RemainingToReturn)).ToList());
}

public class UpdatePurchaseInvoiceCommandHandler
    : IRequestHandler<UpdatePurchaseInvoiceCommand, Result<PurchaseInvoiceDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdatePurchaseInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<PurchaseInvoiceDto>> Handle(
        UpdatePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var inv = await _context.PurchaseInvoices.Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (inv is null)
            return Result<PurchaseInvoiceDto>.Failure("PurchaseInvoiceNotFound", "Purchase invoice not found.");
        if (inv.Status != PurchasingDocumentStatus.Draft)
            return Result<PurchaseInvoiceDto>.Failure(ErrorCodes.CannotModifyApprovedDocument, "Only draft invoices can be updated.");

        var dto = request.Dto;
        var lines = dto.Lines ?? [];
        if (inv.Kind == PurchaseInvoiceKind.Direct)
        {
            var nature = dto.Nature ?? inv.Nature;
            var warehouseId = dto.WarehouseId ?? inv.WarehouseId;
            var validation = CreatePurchaseInvoiceCommandHandler.ValidateDirectLines(lines, nature, warehouseId);
            if (validation is not null)
                return Result<PurchaseInvoiceDto>.Failure(validation.Value.Code, validation.Value.Message);
        }
        else if (lines.Count == 0)
        {
            return Result<PurchaseInvoiceDto>.Failure("NoLines", "Invoice must have lines.");
        }

        inv.UpdateHeader(
            dto.InvoiceDate, dto.PaymentMode, dto.DueDate, dto.SupplierInvoiceNumber, dto.Notes,
            dto.WarehouseId, dto.Nature, dto.ExchangeRate, dto.ExternalReference,
            dto.CostCenterId, dto.DiscountAmount, dto.BranchId);

        inv.ClearLines();
        foreach (var line in lines)
            CreatePurchaseInvoiceCommandHandler.AddLine(inv, line);

        _context.PurchaseInvoices.Update(inv);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<PurchaseInvoiceDto>.Success(CreatePurchaseInvoiceCommandHandler.Map(inv));
    }
}

public class ApprovePurchaseInvoiceCommandHandler : IRequestHandler<ApprovePurchaseInvoiceCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ApprovePurchaseInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ApprovePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var inv = await _context.PurchaseInvoices.Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (inv is null)
            return Result.Failure("PurchaseInvoiceNotFound", "Purchase invoice not found.");

        var supplierExists = await _context.Suppliers.AsNoTracking()
            .AnyAsync(s => s.Id == inv.SupplierId, cancellationToken);
        if (!supplierExists)
            return Result.Failure("SupplierNotFound", "Supplier not found.");

        var itemIds = inv.Lines.Select(l => l.InventoryItemId).Distinct().ToList();
        var itemCount = await _context.InventoryItems.AsNoTracking()
            .CountAsync(i => itemIds.Contains(i.Id), cancellationToken);
        if (itemCount != itemIds.Count)
            return Result.Failure("ItemNotFound", "One or more items were not found.");

        if (inv.AffectsInventory)
        {
            var warehouseId = inv.WarehouseId;
            if (warehouseId is null)
                return Result.Failure("RequiredField", "Warehouse is required.");
            var whExists = await _context.Warehouses.AsNoTracking()
                .AnyAsync(w => w.Id == warehouseId.Value, cancellationToken);
            if (!whExists)
                return Result.Failure("WarehouseNotFound", "Warehouse not found.");
        }

        try
        {
            inv.Approve();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }

        _context.PurchaseInvoices.Update(inv);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class PostPurchaseInvoiceCommandHandler : IRequestHandler<PostPurchaseInvoiceCommand, Result>
{
    private readonly IPurchaseAccountingService _accounting;

    public PostPurchaseInvoiceCommandHandler(IPurchaseAccountingService accounting) => _accounting = accounting;

    public Task<Result> Handle(PostPurchaseInvoiceCommand request, CancellationToken cancellationToken)
        => _accounting.PostPurchaseInvoiceAsync(request.Id, request.UserId, cancellationToken);
}

public class UnpostPurchaseInvoiceCommandHandler : IRequestHandler<UnpostPurchaseInvoiceCommand, Result>
{
    private readonly IPurchaseAccountingService _accounting;

    public UnpostPurchaseInvoiceCommandHandler(IPurchaseAccountingService accounting) => _accounting = accounting;

    public Task<Result> Handle(UnpostPurchaseInvoiceCommand request, CancellationToken cancellationToken)
        => _accounting.UnpostPurchaseInvoiceAsync(request.Id, request.UserId, cancellationToken);
}

public class CancelPurchaseInvoiceCommandHandler : IRequestHandler<CancelPurchaseInvoiceCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CancelPurchaseInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(CancelPurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        var inv = await _context.PurchaseInvoices.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (inv is null)
            return Result.Failure("PurchaseInvoiceNotFound", "Purchase invoice not found.");

        try
        {
            inv.Cancel();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }

        _context.PurchaseInvoices.Update(inv);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
