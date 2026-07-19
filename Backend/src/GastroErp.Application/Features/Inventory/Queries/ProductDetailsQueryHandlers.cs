using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Queries;

public class GetInventoryItemStockByWarehouseQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetInventoryItemStockByWarehouseQuery, Result<List<WarehouseStockBalanceDto>>>
{
    public async Task<Result<List<WarehouseStockBalanceDto>>> Handle(
        GetInventoryItemStockByWarehouseQuery request,
        CancellationToken cancellationToken)
    {
        var item = await context.InventoryItems.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.InventoryItemId, cancellationToken);
        if (item == null)
            return Result<List<WarehouseStockBalanceDto>>.Failure("ItemNotFound", "Inventory item not found.");

        var balances = await context.InventoryBalances.AsNoTracking()
            .Where(b => b.InventoryItemId == request.InventoryItemId)
            .Select(b => new { b.WarehouseId, b.QtyOnHand, b.ReservedQty })
            .ToListAsync(cancellationToken);

        var onHand = balances
            .Select(b => new { b.WarehouseId, Qty = b.QtyOnHand })
            .ToList();

        var reserved = balances
            .Where(b => b.ReservedQty != 0)
            .Select(b => new { b.WarehouseId, Qty = b.ReservedQty })
            .ToList();

        if (onHand.Count == 0)
        {
            // Fallback for pre-pipeline ledger data
            onHand = await context.StockMovements.AsNoTracking()
                .Where(m => m.InventoryItemId == request.InventoryItemId)
                .GroupBy(m => m.WarehouseId)
                .Select(g => new { WarehouseId = g.Key, Qty = g.Sum(x => x.QuantityChange) })
                .ToListAsync(cancellationToken);

            reserved = await context.InventoryReservations.AsNoTracking()
                .Where(r => r.InventoryItemId == request.InventoryItemId && r.Status == ReservationStatus.Active)
                .GroupBy(r => r.WarehouseId)
                .Select(g => new { WarehouseId = g.Key, Qty = g.Sum(x => x.ReservedQuantity) })
                .ToListAsync(cancellationToken);
        }

        var openPoStatuses = new[]
        {
            PurchaseOrderStatus.Approved,
            PurchaseOrderStatus.SentToSupplier,
            PurchaseOrderStatus.PartiallyReceived
        };

        var ordered = await context.PurchaseOrders.AsNoTracking()
            .Where(po => po.TenantId == item.TenantId && openPoStatuses.Contains(po.Status))
            .SelectMany(po => po.Lines.Select(line => new
            {
                po.DestinationWarehouseId,
                line.InventoryItemId,
                Remaining = line.Quantity - line.ReceivedQuantity
            }))
            .Where(x => x.InventoryItemId == request.InventoryItemId && x.Remaining > 0)
            .GroupBy(x => x.DestinationWarehouseId)
            .Select(g => new { WarehouseId = g.Key, Qty = g.Sum(x => x.Remaining) })
            .ToListAsync(cancellationToken);

        var incoming = await context.StockTransfers.AsNoTracking()
            .Where(t => t.TenantId == item.TenantId && t.Status == StockTransferStatus.InTransit)
            .SelectMany(t => t.Lines.Select(line => new
            {
                t.DestinationWarehouseId,
                line.InventoryItemId,
                line.Quantity
            }))
            .Where(x => x.InventoryItemId == request.InventoryItemId)
            .GroupBy(x => x.DestinationWarehouseId)
            .Select(g => new { WarehouseId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToListAsync(cancellationToken);

        var warehouseIds = onHand.Select(x => x.WarehouseId)
            .Concat(reserved.Select(x => x.WarehouseId))
            .Concat(ordered.Select(x => x.WarehouseId))
            .Concat(incoming.Select(x => x.WarehouseId))
            .Distinct()
            .ToList();

        var warehouses = await context.Warehouses.AsNoTracking()
            .Where(w => warehouseIds.Contains(w.Id) || (w.TenantId == item.TenantId && w.IsActive))
            .ToDictionaryAsync(w => w.Id, cancellationToken);

        // Include active warehouses even with zero stock so the matrix is complete for small tenants.
        foreach (var wh in warehouses.Values.Where(w => w.TenantId == item.TenantId && w.IsActive))
        {
            if (!warehouseIds.Contains(wh.Id))
                warehouseIds.Add(wh.Id);
        }

        var onHandMap = onHand.ToDictionary(x => x.WarehouseId, x => x.Qty);
        var reservedMap = reserved.ToDictionary(x => x.WarehouseId, x => x.Qty);
        var orderedMap = ordered.ToDictionary(x => x.WarehouseId, x => x.Qty);
        var incomingMap = incoming.ToDictionary(x => x.WarehouseId, x => x.Qty);

        var rows = warehouseIds
            .Select(id =>
            {
                warehouses.TryGetValue(id, out var wh);
                var oh = onHandMap.GetValueOrDefault(id);
                var res = reservedMap.GetValueOrDefault(id);
                return new WarehouseStockBalanceDto(
                    id,
                    wh?.NameAr ?? "Unknown",
                    wh?.Code,
                    oh,
                    res,
                    oh - res,
                    orderedMap.GetValueOrDefault(id),
                    incomingMap.GetValueOrDefault(id));
            })
            .OrderBy(r => r.WarehouseNameAr)
            .ToList();

        return Result<List<WarehouseStockBalanceDto>>.Success(rows);
    }
}

public class GetInventoryItemMovementsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetInventoryItemMovementsQuery, PagedResult<ItemStockMovementDto>>
{
    public async Task<PagedResult<ItemStockMovementDto>> Handle(
        GetInventoryItemMovementsQuery request,
        CancellationToken cancellationToken)
    {
        var query =
            from m in context.StockMovements.AsNoTracking()
            join t in context.InventoryTransactions.AsNoTracking() on m.InventoryTransactionId equals t.Id
            join wh in context.Warehouses.AsNoTracking() on m.WarehouseId equals wh.Id
            where m.InventoryItemId == request.InventoryItemId
            orderby t.TransactionDate descending, m.CreatedAt descending
            select new ItemStockMovementDto(
                m.Id,
                t.Id,
                t.TransactionDate.UtcDateTime,
                t.TransactionType.ToString(),
                t.ReferenceDocumentNumber,
                m.WarehouseId,
                wh.NameAr,
                m.QuantityChange,
                m.UnitCost,
                m.QuantityChange * m.UnitCost);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<ItemStockMovementDto>.Success(items, total, request.PageNumber, request.PageSize);
    }
}

public class GetInventoryItemPurchaseHistoryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetInventoryItemPurchaseHistoryQuery, PagedResult<ItemPurchaseHistoryDto>>
{
    public async Task<PagedResult<ItemPurchaseHistoryDto>> Handle(
        GetInventoryItemPurchaseHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var query =
            from po in context.PurchaseOrders.AsNoTracking()
            from line in po.Lines
            join supplier in context.Suppliers.AsNoTracking() on po.SupplierId equals supplier.Id
            where line.InventoryItemId == request.InventoryItemId
            orderby po.OrderDate descending
            select new ItemPurchaseHistoryDto(
                po.Id,
                po.PoNumber,
                po.SupplierId,
                supplier.NameAr,
                line.Quantity,
                line.UnitPrice,
                line.LineTotal,
                po.OrderDate.UtcDateTime,
                po.Status.ToString());

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<ItemPurchaseHistoryDto>.Success(items, total, request.PageNumber, request.PageSize);
    }
}

public class GetInventoryItemSalesHistoryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetInventoryItemSalesHistoryQuery, PagedResult<ItemSalesHistoryDto>>
{
    public async Task<PagedResult<ItemSalesHistoryDto>> Handle(
        GetInventoryItemSalesHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var catalog = await context.ProductCatalogDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(c => c.InventoryItemId == request.InventoryItemId, cancellationToken);

        if (catalog?.ProductId is null)
            return PagedResult<ItemSalesHistoryDto>.Success([], 0, request.PageNumber, request.PageSize);

        var productId = catalog.ProductId.Value;

        var query =
            from oi in context.OrderItems.AsNoTracking()
            join so in context.SalesOrders.AsNoTracking() on oi.SalesOrderId equals so.Id
            join customer in context.Customers.AsNoTracking() on so.CustomerId equals customer.Id into customers
            from customer in customers.DefaultIfEmpty()
            where oi.ProductId == productId && !oi.IsVoided
            orderby so.CreatedAt descending
            select new ItemSalesHistoryDto(
                so.Id,
                so.OrderNumber,
                so.CustomerId,
                customer != null ? customer.FullName : null,
                oi.Quantity,
                oi.UnitPrice,
                oi.LineTotal,
                so.CreatedAt.UtcDateTime,
                so.Status.ToString());

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<ItemSalesHistoryDto>.Success(items, total, request.PageNumber, request.PageSize);
    }
}
