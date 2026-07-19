using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Inventory;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Domain.Entities.Inventory.Reservation;
using GastroErp.Domain.Entities.Inventory.Recipe;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.Services;

public sealed class MenuPricingService : IMenuPricingService
{
    private readonly IApplicationDbContext _context;

    public MenuPricingService(IApplicationDbContext context) => _context = context;

    public async Task<ProductPriceSnapshot?> GetProductPriceAsync(
        Guid productId, Guid tenantId, Guid? priceLevelId, CancellationToken ct = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.PriceLevels)
            .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId, ct);

        if (product is null) return null;

        var price = product.BasePrice;

        if (priceLevelId.HasValue)
        {
            var priceLevel = product.PriceLevels.FirstOrDefault(pl => pl.PriceLevelId == priceLevelId.Value);
            if (priceLevel is not null)
                price = priceLevel.Price;
        }

        return new ProductPriceSnapshot(
            product.Id, product.NameAr, product.NameEn, product.SKU,
            price, product.Currency, product.IsAvailable);
    }

    public async Task<ModifierPriceSnapshot?> GetModifierPriceAsync(
        Guid modifierId, Guid tenantId, CancellationToken ct = default)
    {
        var modifier = await _context.Modifiers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == modifierId && m.TenantId == tenantId, ct);

        return modifier is null
            ? null
            : new ModifierPriceSnapshot(modifier.Id, modifier.NameAr, modifier.NameEn, modifier.ExtraPrice);
    }
}

public sealed class OrderNumberGenerator : IOrderNumberGenerator
{
    private readonly IApplicationDbContext _context;

    public OrderNumberGenerator(IApplicationDbContext context) => _context = context;

    public async Task<string> GenerateAsync(Guid branchId, string? branchCode, CancellationToken ct = default)
    {
        var prefix = string.IsNullOrWhiteSpace(branchCode) ? "BR" : branchCode.ToUpperInvariant();
        var year = DateTime.UtcNow.Year;

        var count = await _context.SalesOrders
            .CountAsync(o => o.BranchId == branchId && o.CreatedAt.Year == year, ct);

        return $"{prefix}-{year}-{(count + 1):D5}";
    }
}

public sealed class OrderInventoryService : IOrderInventoryService
{
    private readonly IApplicationDbContext _context;
    private readonly IInventoryMovementPipeline _pipeline;

    public OrderInventoryService(IApplicationDbContext context, IInventoryMovementPipeline pipeline)
        => (_context, _pipeline) = (context, pipeline);

    public async Task ReserveStockForOrderAsync(SalesOrderContext order, CancellationToken ct = default)
    {
        var setting = await _context.InventorySettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == order.TenantId && (s.BranchId == null || s.BranchId == order.BranchId), ct);

        if (setting is not { EnableReservation: true }) return;

        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.BranchId == order.BranchId && w.IsActive, ct)
            ?? await _context.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.TenantId == order.TenantId && w.BranchId == null && w.IsActive, ct);

        if (warehouse is null) return;

        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
        var recipes = await _context.Recipes
            .AsNoTracking()
            .Include(r => r.Items)
            .Where(r => productIds.Contains(r.ProductId) && r.Status == RecipeStatus.Active)
            .ToListAsync(ct);

        foreach (var orderItem in order.Items)
        {
            var recipe = recipes.FirstOrDefault(r => r.ProductId == orderItem.ProductId);
            if (recipe is null) continue;

            foreach (var recipeItem in recipe.Items)
            {
                var qty = recipeItem.Quantity * orderItem.Quantity / (recipe.Yield > 0 ? recipe.Yield : 1);
                if (qty <= 0) continue;

                var reservation = new InventoryReservation(
                    order.TenantId,
                    warehouse.Id,
                    recipeItem.InventoryItemId,
                    qty,
                    order.OrderNumber,
                    DateTimeOffset.UtcNow.AddHours(4));

                _context.InventoryReservations.Add(reservation);
                await _pipeline.ReserveAsync(order.TenantId, warehouse.Id, recipeItem.InventoryItemId, qty, ct);
            }
        }
    }

    public async Task ReleaseStockForOrderAsync(string orderNumber, CancellationToken ct = default)
    {
        var reservations = await _context.InventoryReservations
            .Where(r => r.SourceDocument == orderNumber && r.Status == ReservationStatus.Active)
            .ToListAsync(ct);

        foreach (var reservation in reservations)
        {
            await _pipeline.ReleaseReservationAsync(
                reservation.TenantId,
                reservation.WarehouseId,
                reservation.InventoryItemId,
                reservation.ReservedQuantity,
                ct);
            reservation.Cancel();
            _context.InventoryReservations.Update(reservation);
        }
    }
}

public sealed class ReceiptNumberGenerator : IReceiptNumberGenerator
{
    private readonly IApplicationDbContext _context;

    public ReceiptNumberGenerator(IApplicationDbContext context) => _context = context;

    public async Task<string> GenerateAsync(Guid branchId, CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Payments
            .CountAsync(p => p.BranchId == branchId && p.ProcessedAt.Year == year, ct);
        return $"RCP-{year}-{(count + 1):D6}";
    }
}

public sealed class ShiftNumberGenerator : IShiftNumberGenerator
{
    private readonly IApplicationDbContext _context;

    public ShiftNumberGenerator(IApplicationDbContext context) => _context = context;

    public async Task<string> GenerateAsync(Guid branchId, CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.CashierShifts
            .CountAsync(s => s.BranchId == branchId && s.OpenedAt.Year == year, ct);
        return $"SHF-{year}-{(count + 1):D5}";
    }
}
