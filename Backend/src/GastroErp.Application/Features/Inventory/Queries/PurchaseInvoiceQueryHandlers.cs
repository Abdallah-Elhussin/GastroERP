using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Queries;

public class GetPurchaseInvoicesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPurchaseInvoicesQuery, PagedResult<PurchaseInvoiceDto>>
{
    public async Task<PagedResult<PurchaseInvoiceDto>> Handle(
        GetPurchaseInvoicesQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = context.PurchaseInvoices.AsNoTracking()
            .Include(i => i.Lines)
            .Where(i => i.TenantId == request.TenantId);

        if (request.Kind.HasValue)
            query = query.Where(i => i.Kind == request.Kind.Value);
        if (request.Status.HasValue)
            query = query.Where(i => i.Status == request.Status.Value);
        if (request.SupplierId.HasValue)
            query = query.Where(i => i.SupplierId == request.SupplierId.Value);
        if (request.WarehouseId.HasValue)
            query = query.Where(i => i.WarehouseId == request.WarehouseId.Value);
        if (request.PaymentMode.HasValue)
            query = query.Where(i => i.PaymentMode == request.PaymentMode.Value);
        if (request.Nature.HasValue)
            query = query.Where(i => i.Nature == request.Nature.Value);
        if (request.From.HasValue)
            query = query.Where(i => i.InvoiceDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(i => i.InvoiceDate <= request.To.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(i =>
                i.InvoiceNumber.Contains(term)
                || (i.SupplierInvoiceNumber != null && i.SupplierInvoiceNumber.Contains(term))
                || (i.ExternalReference != null && i.ExternalReference.Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(i => i.InvoiceDate)
            .ThenByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = rows.Select(CreatePurchaseInvoiceCommandHandler.Map).ToList();
        return PagedResult<PurchaseInvoiceDto>.Success(items, page, pageSize, total);
    }
}

public class GetPurchaseInvoiceByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPurchaseInvoiceByIdQuery, Result<PurchaseInvoiceDto>>
{
    public async Task<Result<PurchaseInvoiceDto>> Handle(
        GetPurchaseInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var inv = await context.PurchaseInvoices.AsNoTracking()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (inv is null)
            return Result<PurchaseInvoiceDto>.Failure("PurchaseInvoiceNotFound", "Purchase invoice not found.");

        return Result<PurchaseInvoiceDto>.Success(CreatePurchaseInvoiceCommandHandler.Map(inv));
    }
}
