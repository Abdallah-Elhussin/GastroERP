using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Common.Interfaces.Inventory;

/// <summary>
/// نقطة الترحيل الوحيدة للمخزون — لا يُحدَّث الرصيد من خارج هذا المسار.
/// </summary>
public interface IInventoryMovementPipeline
{
    Task<Result<Guid>> ApplyMovementAsync(InventoryMovementRequest request, CancellationToken cancellationToken = default);

    Task ReserveAsync(Guid tenantId, Guid warehouseId, Guid inventoryItemId, decimal quantity, CancellationToken cancellationToken = default);

    Task ReleaseReservationAsync(Guid tenantId, Guid warehouseId, Guid inventoryItemId, decimal quantity, CancellationToken cancellationToken = default);

    Task FulfillReservationAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid inventoryItemId,
        decimal quantity,
        Guid referenceDocumentId,
        string referenceDocumentNumber,
        CancellationToken cancellationToken = default);
}

public sealed record InventoryMovementRequest(
    Guid TenantId,
    InventoryMovementType MovementType,
    TransactionType TransactionType,
    Guid ReferenceDocumentId,
    string ReferenceDocumentNumber,
    IReadOnlyList<InventoryMovementLine> Lines,
    string? Notes = null,
    DateTimeOffset? TransactionDate = null);

public sealed record InventoryMovementLine(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid UnitId,
    decimal Quantity,
    decimal? ExplicitUnitCost = null,
    Guid? WarehouseBinId = null,
    Guid? InventoryBatchId = null,
    /// <summary>Used only when MovementType is ADJ or REV.</summary>
    bool AdjIncreasesOnHand = true);

public interface IInventoryCostingService
{
    /// <summary>Returns unit cost to stamp on the movement (Weighted Average only).</summary>
    Task<decimal> ResolveUnitCostAsync(
        Guid tenantId,
        Guid inventoryItemId,
        Guid warehouseId,
        InventoryMovementType movementType,
        decimal quantity,
        decimal? explicitUnitCost,
        bool adjIncreasesOnHand,
        CancellationToken cancellationToken = default);
}
