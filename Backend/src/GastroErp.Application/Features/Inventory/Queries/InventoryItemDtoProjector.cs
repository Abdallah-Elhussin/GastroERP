using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Entities.Inventory.Catalog;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Queries;

internal static class InventoryItemDtoProjector
{
    public static async Task<List<InventoryItemDto>> ProjectAsync(
        IApplicationDbContext context,
        IReadOnlyList<InventoryItem> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            return [];

        var itemIds = items.Select(i => i.Id).ToList();
        var categoryIds = items.Select(i => i.CategoryId).Distinct().ToList();
        var unitIds = items.Select(i => i.BaseUnitId).Distinct().ToList();

        var categories = await context.InventoryCategories.AsNoTracking()
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var units = await context.InventoryUnits.AsNoTracking()
            .Where(u => unitIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var receiptMovements = await context.StockMovements.AsNoTracking()
            .Where(m => itemIds.Contains(m.InventoryItemId) && m.QuantityChange > 0)
            .Select(m => new { m.InventoryItemId, m.QuantityChange, m.UnitCost, m.CreatedAt })
            .ToListAsync(cancellationToken);

        var costByItem = receiptMovements
            .GroupBy(m => m.InventoryItemId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var totalQty = g.Sum(x => x.QuantityChange);
                    var weighted = totalQty > 0
                        ? g.Sum(x => x.QuantityChange * x.UnitCost) / totalQty
                        : (decimal?)null;
                    var last = g.OrderByDescending(x => x.CreatedAt).First().UnitCost;
                    return (Average: weighted, Last: (decimal?)last);
                });

        return items.Select(item =>
        {
            decimal? average = null;
            decimal? last = null;
            if (costByItem.TryGetValue(item.Id, out var cost))
            {
                average = cost.Average;
                last = cost.Last;
            }

            categories.TryGetValue(item.CategoryId, out var category);
            units.TryGetValue(item.BaseUnitId, out var unit);

            return new InventoryItemDto(
                item.Id,
                item.TenantId,
                item.CategoryId,
                category?.NameAr ?? string.Empty,
                item.NameAr,
                item.NameEn,
                item.DescriptionAr,
                item.DescriptionEn,
                item.Sku,
                item.Barcode,
                item.ImageUrl,
                item.ItemKind,
                item.BaseUnitId,
                unit?.NameAr ?? string.Empty,
                item.DefaultPurchaseUnitId,
                item.DefaultRecipeUnitId,
                item.ReorderLevel,
                item.ReorderQuantity,
                average,
                last,
                item.IsActive,
                item.CreatedAt.UtcDateTime);
        }).ToList();
    }
}
