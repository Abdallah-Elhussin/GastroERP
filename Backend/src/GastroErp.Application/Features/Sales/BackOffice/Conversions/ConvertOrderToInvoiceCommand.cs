using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Sales.BackOffice;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.BackOffice.Conversions;

public record ConvertOrderToInvoiceLineDto(Guid OrderLineId, decimal Quantity);

public record ConvertOrderToInvoiceCommand(
    Guid OrderId,
    BackOfficeSalesPaymentMode PaymentMode,
    BackOfficeSalesInvoiceNature Nature = BackOfficeSalesInvoiceNature.Inventory,
    DateOnly? InvoiceDate = null,
    Guid? WarehouseId = null,
    Guid? CostCenterId = null,
    DateOnly? DueDate = null,
    string? InvoiceNumber = null,
    IReadOnlyList<ConvertOrderToInvoiceLineDto>? Selection = null) : IRequest<Result<Guid>>;

public sealed class ConvertOrderToInvoiceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ConvertOrderToInvoiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        ConvertOrderToInvoiceCommand request, CancellationToken cancellationToken)
    {
        var order = await context.BackOfficeSalesOrders.Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        if (order is null)
            return Result<Guid>.Failure("OrderNotFound", "Sales order not found.");
        if (order.Status is not (BackOfficeSalesDocumentStatus.Approved or BackOfficeSalesDocumentStatus.Posted))
            return Result<Guid>.Failure(ErrorCodes.SalesInvalidStatusTransition,
                "Sales order must be approved before invoicing.");

        var invoiceLines = ResolveInvoiceLines(order, request.Selection);
        if (invoiceLines.Count == 0)
            return Result<Guid>.Failure("NothingToInvoice", "No remaining quantity to invoice on this order.");

        var warehouseId = request.WarehouseId ?? order.WarehouseId;
        if (request.Nature == BackOfficeSalesInvoiceNature.Inventory && warehouseId is null
            && invoiceLines.Any(l => l.OrderLine.LineNature == BackOfficeSalesLineNature.Inventory))
            return Result<Guid>.Failure(ErrorCodes.RequiredField,
                "Warehouse is required for inventory sales invoices.");

        var number = string.IsNullOrWhiteSpace(request.InvoiceNumber)
            ? $"SI-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : request.InvoiceNumber.Trim();
        var invoiceDate = request.InvoiceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        try
        {
            var invoice = BackOfficeSalesInvoice.CreateDraft(
                order.TenantId, number, order.CustomerId, invoiceDate, request.PaymentMode,
                request.Nature, order.Currency, companyId: null, order.BranchId, warehouseId,
                request.CostCenterId, order.SalesPersonId, backOfficeSalesOrderId: order.Id,
                request.DueDate, order.ExchangeRate,
                externalReference: order.OrderNumber, notes: order.Notes,
                discountAmount: 0);

            foreach (var pair in invoiceLines)
            {
                var line = pair.OrderLine;
                invoice.AddLine(
                    line.InventoryItemId,
                    productId: null,
                    line.Description,
                    pair.Quantity,
                    line.UnitPrice,
                    MapLineNature(request.Nature, line.LineNature),
                    line.UnitId,
                    lineWarehouseId: warehouseId,
                    discountPercent: 0,
                    discountAmount: line.Quantity > 0 ? Math.Round(line.DiscountAmount * (pair.Quantity / line.Quantity), 4) : 0,
                    taxPercent: line.TaxPercent,
                    taxAmount: 0,
                    costCenterId: request.CostCenterId,
                    unitCost: line.UnitCost,
                    salesOrderLineId: line.Id);
            }

            context.BackOfficeSalesInvoices.Add(invoice);
            await context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(invoice.Id);
        }
        catch (BusinessException ex)
        {
            return Result<Guid>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    private static List<(BackOfficeSalesOrderLine OrderLine, decimal Quantity)> ResolveInvoiceLines(
        BackOfficeSalesOrder order,
        IReadOnlyList<ConvertOrderToInvoiceLineDto>? selection)
    {
        var result = new List<(BackOfficeSalesOrderLine, decimal)>();
        if (selection is null || selection.Count == 0)
        {
            foreach (var line in order.Lines)
            {
                if (line.RemainingToInvoice > 0)
                    result.Add((line, line.RemainingToInvoice));
            }
            return result;
        }

        foreach (var sel in selection)
        {
            var line = order.Lines.FirstOrDefault(l => l.Id == sel.OrderLineId);
            if (line is null || sel.Quantity <= 0) continue;
            var qty = Math.Min(sel.Quantity, line.RemainingToInvoice);
            if (qty > 0)
                result.Add((line, qty));
        }
        return result;
    }

    private static BackOfficeSalesLineNature MapLineNature(
        BackOfficeSalesInvoiceNature invoiceNature,
        BackOfficeSalesLineNature orderLineNature)
    {
        return invoiceNature switch
        {
            BackOfficeSalesInvoiceNature.Services => BackOfficeSalesLineNature.Service,
            BackOfficeSalesInvoiceNature.Project => BackOfficeSalesLineNature.Project,
            BackOfficeSalesInvoiceNature.Assets => BackOfficeSalesLineNature.Asset,
            BackOfficeSalesInvoiceNature.Inventory => BackOfficeSalesLineNature.Inventory,
            _ => orderLineNature
        };
    }
}
