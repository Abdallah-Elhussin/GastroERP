using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.ProductInquiry.Dtos;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.ProductInquiry.Queries;

public sealed record GetProductInquiryListQuery(
    Guid TenantId,
    string? Search = null,
    bool ActiveOnly = true,
    bool InventoryOnly = false,
    Guid? CategoryId = null,
    Guid? ItemTypeId = null,
    string? SortBy = null,
    bool SortDesc = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PagedResult<ProductInquiryListItemDto>>;

public sealed class GetProductInquiryListQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductInquiryListQuery, PagedResult<ProductInquiryListItemDto>>
{
    public async Task<PagedResult<ProductInquiryListItemDto>> Handle(
        GetProductInquiryListQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 50 : request.PageSize;

        var query = db.InventoryItems.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId);

        if (request.ActiveOnly)
            query = query.Where(x => x.IsActive);
        if (request.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        if (request.ItemTypeId.HasValue)
            query = query.Where(x => x.ItemTypeId == request.ItemTypeId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x =>
                x.NameAr.Contains(term)
                || (x.NameEn != null && x.NameEn.Contains(term))
                || (x.Sku != null && x.Sku.Contains(term))
                || (x.Barcode != null && x.Barcode.Contains(term)));
        }

        if (request.InventoryOnly)
        {
            var inventoryTypeIds = await db.InventoryItemTypes.AsNoTracking()
                .Where(t => t.TenantId == request.TenantId && t.IsInventory)
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);
            query = query.Where(x => x.ItemTypeId != null && inventoryTypeIds.Contains(x.ItemTypeId.Value));
        }

        query = (request.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "sku" => request.SortDesc ? query.OrderByDescending(x => x.Sku) : query.OrderBy(x => x.Sku),
            "nameen" => request.SortDesc ? query.OrderByDescending(x => x.NameEn) : query.OrderBy(x => x.NameEn),
            "isactive" => request.SortDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => request.SortDesc ? query.OrderByDescending(x => x.NameAr) : query.OrderBy(x => x.NameAr)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        if (items.Count == 0)
            return PagedResult<ProductInquiryListItemDto>.Success([], page, pageSize, total);

        var itemIds = items.Select(i => i.Id).ToList();
        var categoryIds = items.Select(i => i.CategoryId).Distinct().ToList();
        var typeIds = items.Where(i => i.ItemTypeId.HasValue).Select(i => i.ItemTypeId!.Value).Distinct().ToList();
        var unitIds = items.Select(i => i.BaseUnitId).Distinct().ToList();

        var categories = await db.InventoryCategories.AsNoTracking()
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.NameAr, cancellationToken);

        var types = typeIds.Count == 0
            ? new Dictionary<Guid, (string NameAr, bool IsInventory)>()
            : await db.InventoryItemTypes.AsNoTracking()
                .Where(t => typeIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => (t.NameAr, t.IsInventory), cancellationToken);

        var units = await db.InventoryUnits.AsNoTracking()
            .Where(u => unitIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.NameAr, cancellationToken);

        var balances = await db.InventoryBalances.AsNoTracking()
            .Where(b => itemIds.Contains(b.InventoryItemId))
            .GroupBy(b => b.InventoryItemId)
            .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.QtyOnHand) })
            .ToDictionaryAsync(x => x.ItemId, x => x.Qty, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var prices = await db.ProductPrices.AsNoTracking()
            .Where(p => itemIds.Contains(p.ProductId)
                        && p.TenantId == request.TenantId
                        && p.IsActive
                        && p.StartDate <= now
                        && (p.EndDate == null || p.EndDate >= now))
            .OrderByDescending(p => p.Priority)
            .ThenByDescending(p => p.IsDefault)
            .ThenByDescending(p => p.StartDate)
            .Select(p => new { p.ProductId, p.SellingPrice })
            .ToListAsync(cancellationToken);

        var priceMap = prices
            .GroupBy(p => p.ProductId)
            .ToDictionary(g => g.Key, g => (decimal?)g.First().SellingPrice);

        var lastPurchases = await db.StockMovements.AsNoTracking()
            .Where(m => itemIds.Contains(m.InventoryItemId) && m.UnitCost > 0 && m.QuantityChange > 0)
            .GroupBy(m => m.InventoryItemId)
            .Select(g => new
            {
                ItemId = g.Key,
                Cost = g.OrderByDescending(x => x.CreatedAt).Select(x => x.UnitCost).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.ItemId, x => (decimal?)x.Cost, cancellationToken);

        var dtos = items.Select(i =>
        {
            types.TryGetValue(i.ItemTypeId ?? Guid.Empty, out var type);
            return new ProductInquiryListItemDto(
                i.Id,
                i.Sku,
                i.Barcode,
                i.NameAr,
                i.NameEn,
                categories.GetValueOrDefault(i.CategoryId),
                i.ItemTypeId.HasValue ? type.NameAr : null,
                units.GetValueOrDefault(i.BaseUnitId),
                priceMap.GetValueOrDefault(i.Id),
                lastPurchases.GetValueOrDefault(i.Id),
                balances.GetValueOrDefault(i.Id),
                i.IsActive,
                i.ItemTypeId.HasValue && type.IsInventory,
                i.ItemKind == InventoryItemKind.Manufactured
                    ? InventoryItemKindDto.Manufactured
                    : InventoryItemKindDto.Raw);
        }).ToList();

        return PagedResult<ProductInquiryListItemDto>.Success(dtos, page, pageSize, total);
    }
}

public sealed record GetProductInquiryDetailQuery(
    Guid TenantId,
    Guid ProductId,
    bool CanViewCost = true,
    bool CanViewPrices = true,
    bool CanViewMovements = true,
    bool CanViewSuppliers = true) : IRequest<Result<ProductInquiryDetailDto>>;

public sealed class GetProductInquiryDetailQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductInquiryDetailQuery, Result<ProductInquiryDetailDto>>
{
    public async Task<Result<ProductInquiryDetailDto>> Handle(
        GetProductInquiryDetailQuery request,
        CancellationToken cancellationToken)
    {
        var item = await db.InventoryItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ProductId && x.TenantId == request.TenantId, cancellationToken);
        if (item is null)
            return Result<ProductInquiryDetailDto>.Failure("ProductNotFound", "Inventory item not found.");

        var categoryName = await db.InventoryCategories.AsNoTracking()
            .Where(c => c.Id == item.CategoryId)
            .Select(c => c.NameAr)
            .FirstOrDefaultAsync(cancellationToken);

        string? typeName = null;
        var isInventory = true;
        if (item.ItemTypeId.HasValue)
        {
            var type = await db.InventoryItemTypes.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == item.ItemTypeId.Value, cancellationToken);
            typeName = type?.NameAr;
            isInventory = type?.IsInventory ?? true;
        }

        var unitName = await db.InventoryUnits.AsNoTracking()
            .Where(u => u.Id == item.BaseUnitId)
            .Select(u => u.NameAr)
            .FirstOrDefaultAsync(cancellationToken);

        var general = new ProductInquiryGeneralDto(
            item.Sku,
            item.NameAr,
            item.NameEn,
            item.Barcode,
            categoryName,
            typeName,
            unitName,
            item.IsActive,
            isInventory,
            item.ItemKind.ToString());

        var balanceRows = await db.InventoryBalances.AsNoTracking()
            .Where(b => b.InventoryItemId == item.Id)
            .ToListAsync(cancellationToken);

        var warehouseIds = balanceRows.Select(b => b.WarehouseId).Distinct().ToList();
        var activeWarehouses = await db.Warehouses.AsNoTracking()
            .Where(w => w.TenantId == request.TenantId && w.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var wh in activeWarehouses)
        {
            if (!warehouseIds.Contains(wh.Id))
                warehouseIds.Add(wh.Id);
        }

        var warehousesDict = activeWarehouses
            .Concat(await db.Warehouses.AsNoTracking()
                .Where(w => warehouseIds.Contains(w.Id) && !activeWarehouses.Select(a => a.Id).Contains(w.Id))
                .ToListAsync(cancellationToken))
            .GroupBy(w => w.Id)
            .ToDictionary(g => g.Key, g => g.First());

        var balMap = balanceRows.ToDictionary(b => b.WarehouseId);
        var warehouseDtos = warehouseIds
            .Select(id =>
            {
                warehousesDict.TryGetValue(id, out var wh);
                balMap.TryGetValue(id, out var bal);
                var onHand = bal?.QtyOnHand ?? 0;
                var reserved = bal?.ReservedQty ?? 0;
                return new ProductInquiryWarehouseStockDto(
                    id,
                    wh?.NameAr ?? "—",
                    wh?.Code,
                    wh?.BranchId,
                    onHand,
                    reserved,
                    onHand - reserved);
            })
            .OrderBy(w => w.WarehouseNameAr)
            .ToList();

        var totalOnHand = warehouseDtos.Sum(w => w.OnHand);
        var avgCostFromBalances = balanceRows.Where(b => b.QtyOnHand > 0).Select(b => b.AvgCost).DefaultIfEmpty(0).Average();

        var lastMovementCost = await db.StockMovements.AsNoTracking()
            .Where(m => m.InventoryItemId == item.Id && m.UnitCost > 0 && m.QuantityChange > 0)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => (decimal?)m.UnitCost)
            .FirstOrDefaultAsync(cancellationToken) ?? 0m;

        var costingMethod = await db.InventorySettings.AsNoTracking()
            .Where(s => s.TenantId == request.TenantId)
            .Select(s => s.CostingMethod)
            .FirstOrDefaultAsync(cancellationToken);

        var cost = new ProductInquiryCostDto(
            request.CanViewCost ? decimal.Round(avgCostFromBalances, 4) : 0,
            request.CanViewCost ? decimal.Round(lastMovementCost, 4) : 0,
            null,
            costingMethod == 0 ? InventoryCostingMethod.WeightedAverage.ToString() : costingMethod.ToString(),
            request.CanViewCost);

        // Purchase (Goods Receipt preferred, PO fallback)
        var lastGr = await (
            from gr in db.GoodsReceipts.AsNoTracking()
            from line in gr.Lines
            join supplier in db.Suppliers.AsNoTracking() on gr.SupplierId equals supplier.Id
            where line.InventoryItemId == item.Id && gr.TenantId == request.TenantId
            orderby gr.ReceiptDate descending
            select new
            {
                line.UnitCost,
                SupplierName = supplier.NameAr,
                gr.ReceiptNumber,
                gr.ReceiptDate,
                supplier.LeadTimeDays,
                SupplierId = supplier.Id
            }).FirstOrDefaultAsync(cancellationToken);

        ProductInquiryPurchaseDto purchase;
        if (lastGr is not null)
        {
            purchase = new ProductInquiryPurchaseDto(
                request.CanViewCost || request.CanViewSuppliers ? lastGr.UnitCost : null,
                request.CanViewSuppliers ? lastGr.SupplierName : null,
                request.CanViewSuppliers ? lastGr.ReceiptNumber : null,
                lastGr.ReceiptDate);
        }
        else
        {
            var lastPo = await (
                from po in db.PurchaseOrders.AsNoTracking()
                from line in po.Lines
                join supplier in db.Suppliers.AsNoTracking() on po.SupplierId equals supplier.Id
                where line.InventoryItemId == item.Id && po.TenantId == request.TenantId
                orderby po.OrderDate descending
                select new
                {
                    line.UnitPrice,
                    SupplierName = supplier.NameAr,
                    po.PoNumber,
                    po.OrderDate,
                    supplier.LeadTimeDays
                }).FirstOrDefaultAsync(cancellationToken);

            purchase = new ProductInquiryPurchaseDto(
                lastPo is null || !(request.CanViewCost || request.CanViewSuppliers) ? null : lastPo.UnitPrice,
                request.CanViewSuppliers ? lastPo?.SupplierName : null,
                request.CanViewSuppliers ? lastPo?.PoNumber : null,
                lastPo?.OrderDate);
        }

        var supplierInfo = new ProductInquirySupplierDto(
            request.CanViewSuppliers ? lastGr?.SupplierName ?? purchase.LastSupplierName : null,
            request.CanViewSuppliers ? purchase.LastSupplierName : null,
            request.CanViewSuppliers ? lastGr?.LeadTimeDays : null,
            request.CanViewSuppliers ? purchase.LastPurchasePrice : null,
            request.CanViewSuppliers);

        // Sales via catalog bridge
        var catalog = await db.ProductCatalogDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(c => c.InventoryItemId == item.Id, cancellationToken);

        decimal? defaultPrice = null;
        decimal? lastSalePrice = null;
        DateTimeOffset? lastSaleAt = null;
        string? lastCustomer = null;
        string? lastOrderNumber = null;

        if (catalog?.ProductId is Guid menuProductId)
        {
            defaultPrice = await db.Products.AsNoTracking()
                .Where(p => p.Id == menuProductId)
                .Select(p => (decimal?)p.BasePrice)
                .FirstOrDefaultAsync(cancellationToken);

            var lastSale = await (
                from oi in db.OrderItems.AsNoTracking()
                join so in db.SalesOrders.AsNoTracking() on oi.SalesOrderId equals so.Id
                join customer in db.Customers.AsNoTracking() on so.CustomerId equals customer.Id into customers
                from customer in customers.DefaultIfEmpty()
                where oi.ProductId == menuProductId && !oi.IsVoided
                orderby so.CreatedAt descending
                select new
                {
                    oi.UnitPrice,
                    so.CreatedAt,
                    CustomerName = customer != null ? customer.FullName : null,
                    so.OrderNumber
                }).FirstOrDefaultAsync(cancellationToken);

            if (lastSale is not null)
            {
                lastSalePrice = lastSale.UnitPrice;
                lastSaleAt = lastSale.CreatedAt;
                lastCustomer = lastSale.CustomerName;
                lastOrderNumber = lastSale.OrderNumber;
            }
        }

        if (defaultPrice is null)
        {
            var now = DateTimeOffset.UtcNow;
            defaultPrice = await db.ProductPrices.AsNoTracking()
                .Where(p => p.ProductId == item.Id && p.TenantId == request.TenantId && p.IsActive
                            && p.StartDate <= now && (p.EndDate == null || p.EndDate >= now))
                .OrderByDescending(p => p.IsDefault)
                .ThenByDescending(p => p.Priority)
                .Select(p => (decimal?)p.SellingPrice)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var sales = new ProductInquirySalesDto(lastSalePrice, defaultPrice, lastSaleAt, lastCustomer, lastOrderNumber);

        IReadOnlyList<ProductInquiryPriceRowDto> prices = [];
        if (request.CanViewPrices)
        {
            var now = DateTimeOffset.UtcNow;
            var priceEntities = await db.ProductPrices.AsNoTracking()
                .Where(p => p.ProductId == item.Id && p.TenantId == request.TenantId)
                .OrderByDescending(p => p.IsActive)
                .ThenByDescending(p => p.Priority)
                .ThenByDescending(p => p.StartDate)
                .Take(50)
                .ToListAsync(cancellationToken);

            var listIds = priceEntities.Select(p => p.PriceListId).Distinct().ToList();
            var unitPriceIds = priceEntities.Select(p => p.UnitId).Distinct().ToList();
            var lists = listIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await db.SalesPriceLists.AsNoTracking()
                    .Where(l => listIds.Contains(l.Id))
                    .ToDictionaryAsync(l => l.Id, l => l.NameAr, cancellationToken);
            var priceUnits = unitPriceIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await db.InventoryUnits.AsNoTracking()
                    .Where(u => unitPriceIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.NameAr, cancellationToken);

            prices = priceEntities.Select(p => new ProductInquiryPriceRowDto(
                lists.GetValueOrDefault(p.PriceListId) ?? "—",
                priceUnits.GetValueOrDefault(p.UnitId),
                p.SellingPrice,
                p.StartDate,
                p.EndDate,
                p.IsActive && p.StartDate <= now && (p.EndDate == null || p.EndDate >= now))).ToList();
        }

        var reservations = await (
            from r in db.InventoryReservations.AsNoTracking()
            join wh in db.Warehouses.AsNoTracking() on r.WarehouseId equals wh.Id
            where r.InventoryItemId == item.Id && r.Status == ReservationStatus.Active
            orderby r.CreatedAt descending
            select new ProductInquiryReservationDto(
                r.Id,
                wh.NameAr,
                r.ReservedQuantity,
                r.SourceDocument,
                r.Status.ToString(),
                r.ExpirationDate)).Take(20).ToListAsync(cancellationToken);

        var batchQty = await db.StockMovements.AsNoTracking()
            .Where(m => m.InventoryItemId == item.Id && m.InventoryBatchId != null)
            .GroupBy(m => m.InventoryBatchId!.Value)
            .Select(g => new { BatchId = g.Key, Qty = g.Sum(x => x.QuantityChange) })
            .ToDictionaryAsync(x => x.BatchId, x => x.Qty, cancellationToken);

        var batches = await db.InventoryBatches.AsNoTracking()
            .Where(b => b.InventoryItemId == item.Id && b.Status == BatchStatus.Active)
            .OrderBy(b => b.ExpirationDate)
            .Take(30)
            .ToListAsync(cancellationToken);

        var batchDtos = batches.Select(b => new ProductInquiryBatchDto(
            b.Id,
            b.BatchNumber,
            b.LotNumber,
            b.ExpirationDate,
            b.Status.ToString(),
            batchQty.GetValueOrDefault(b.Id))).ToList();

        IReadOnlyList<ProductInquiryMovementDto> movements = [];
        if (request.CanViewMovements)
        {
            movements = await (
                from m in db.StockMovements.AsNoTracking()
                join t in db.InventoryTransactions.AsNoTracking() on m.InventoryTransactionId equals t.Id
                join wh in db.Warehouses.AsNoTracking() on m.WarehouseId equals wh.Id
                where m.InventoryItemId == item.Id
                orderby t.TransactionDate descending
                select new ProductInquiryMovementDto(
                    m.Id,
                    t.TransactionDate,
                    t.TransactionType.ToString(),
                    m.QuantityChange,
                    t.ReferenceDocumentNumber,
                    wh.NameAr)).Take(10).ToListAsync(cancellationToken);
        }

        // Recipe via catalog → menu product
        ProductInquiryRecipeDto recipeDto = new(false, 0, null, null);
        if (catalog?.ProductId is Guid recipeProductId)
        {
            var recipe = await db.Recipes.AsNoTracking()
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.ProductId == recipeProductId && r.TenantId == request.TenantId, cancellationToken);
            if (recipe is not null)
            {
                decimal? recipeCost = null;
                if (request.CanViewCost && recipe.Items.Count > 0)
                {
                    var ingredientIds = recipe.Items.Select(i => i.InventoryItemId).Distinct().ToList();
                    var ingredientCosts = await db.InventoryBalances.AsNoTracking()
                        .Where(b => ingredientIds.Contains(b.InventoryItemId) && b.QtyOnHand > 0)
                        .GroupBy(b => b.InventoryItemId)
                        .Select(g => new { ItemId = g.Key, Avg = g.Average(x => x.AvgCost) })
                        .ToDictionaryAsync(x => x.ItemId, x => x.Avg, cancellationToken);

                    recipeCost = recipe.Items.Sum(i =>
                        i.Quantity * ingredientCosts.GetValueOrDefault(i.InventoryItemId));
                }

                recipeDto = new ProductInquiryRecipeDto(true, recipe.Items.Count, recipeCost, recipe.NameAr);
            }
        }

        var branchGroups = warehouseDtos
            .Where(w => w.BranchId.HasValue)
            .GroupBy(w => w.BranchId!.Value)
            .Select(g => new { BranchId = g.Key, Qty = g.Sum(x => x.OnHand) })
            .ToList();

        var branchIds = branchGroups.Select(b => b.BranchId).ToList();
        var branches = branchIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.Branches.AsNoTracking()
                .Where(b => branchIds.Contains(b.Id))
                .ToDictionaryAsync(b => b.Id, b => b.NameAr, cancellationToken);

        var branchDtos = branchGroups
            .Select(b => new ProductInquiryBranchStockDto(
                b.BranchId,
                branches.GetValueOrDefault(b.BranchId) ?? "—",
                b.Qty))
            .OrderBy(b => b.BranchNameAr)
            .ToList();

        // Analytics
        var since = DateTimeOffset.UtcNow.AddDays(-90);
        var salesQty90 = 0m;
        if (catalog?.ProductId is Guid pid)
        {
            salesQty90 = await db.OrderItems.AsNoTracking()
                .Where(oi => oi.ProductId == pid && !oi.IsVoided)
                .Join(db.SalesOrders.AsNoTracking(), oi => oi.SalesOrderId, so => so.Id, (oi, so) => new { oi, so })
                .Where(x => x.so.CreatedAt >= since)
                .SumAsync(x => (decimal?)x.oi.Quantity, cancellationToken) ?? 0m;
        }

        var consumption90 = await db.StockMovements.AsNoTracking()
            .Where(m => m.InventoryItemId == item.Id && m.CreatedAt >= since && m.QuantityChange < 0)
            .SumAsync(m => (decimal?)(-m.QuantityChange), cancellationToken) ?? 0m;

        var avgMonthlySales = salesQty90 / 3m;
        var avgMonthlyConsumption = consumption90 / 3m;
        var dailyUse = avgMonthlyConsumption / 30m;
        decimal? daysOfCover = dailyUse > 0 ? totalOnHand / dailyUse : null;
        var inventoryValue = request.CanViewCost ? totalOnHand * (avgCostFromBalances > 0 ? avgCostFromBalances : lastMovementCost) : 0;

        var stockStatus = totalOnHand <= 0
            ? "OutOfStock"
            : totalOnHand <= item.ReorderLevel
                ? "BelowReorder"
                : "Ok";

        var analytics = new ProductInquiryAnalyticsDto(
            totalOnHand,
            decimal.Round(inventoryValue, 4),
            decimal.Round(avgMonthlySales, 4),
            decimal.Round(avgMonthlyConsumption, 4),
            daysOfCover.HasValue ? decimal.Round(daysOfCover.Value, 1) : null,
            item.ReorderLevel,
            stockStatus);

        return Result<ProductInquiryDetailDto>.Success(new ProductInquiryDetailDto(
            item.Id,
            general,
            warehouseDtos,
            cost,
            sales,
            purchase,
            prices,
            reservations,
            batchDtos,
            movements,
            recipeDto,
            supplierInfo,
            branchDtos,
            analytics));
    }
}
