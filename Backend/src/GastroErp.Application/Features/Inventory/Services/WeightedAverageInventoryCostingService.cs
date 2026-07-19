using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Inventory;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Services;

/// <summary>
/// Weighted Average is the only active costing engine.
/// FIFO / StandardCost enums remain but are not executed.
/// </summary>
public sealed class WeightedAverageInventoryCostingService(IApplicationDbContext context) : IInventoryCostingService
{
    public async Task<decimal> ResolveUnitCostAsync(
        Guid tenantId,
        Guid inventoryItemId,
        Guid warehouseId,
        InventoryMovementType movementType,
        decimal quantity,
        decimal? explicitUnitCost,
        bool adjIncreasesOnHand,
        CancellationToken cancellationToken = default)
    {
        var balance = await context.InventoryBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.TenantId == tenantId
                     && b.InventoryItemId == inventoryItemId
                     && b.WarehouseId == warehouseId,
                cancellationToken);

        var increases = movementType is InventoryMovementType.IN or InventoryMovementType.TRI
            || (movementType == InventoryMovementType.ADJ && adjIncreasesOnHand)
            || (movementType == InventoryMovementType.REV && adjIncreasesOnHand);

        if (increases)
        {
            if (explicitUnitCost.HasValue && explicitUnitCost.Value >= 0)
                return explicitUnitCost.Value;

            return balance?.AvgCost ?? 0m;
        }

        // Outbound / TRO / ADJ decrease / REV decrease: stamp current avg cost
        return balance?.AvgCost ?? explicitUnitCost ?? 0m;
    }
}
