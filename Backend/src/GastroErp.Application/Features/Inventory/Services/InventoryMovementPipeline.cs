using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Inventory;
using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Entities.Inventory.Transactions;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Services;

/// <summary>
/// Sole writer of InventoryBalance, StockMovement, and InventoryTransaction.
/// </summary>
public sealed class InventoryMovementPipeline(
    IApplicationDbContext context,
    IInventoryCostingService costing,
    ILogger<InventoryMovementPipeline> logger) : IInventoryMovementPipeline
{
    public async Task<Result<Guid>> ApplyMovementAsync(
        InventoryMovementRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.TenantId == Guid.Empty)
            return Result<Guid>.Failure("InvalidTenant", "TenantId is required.");
        if (request.ReferenceDocumentId == Guid.Empty)
            return Result<Guid>.Failure("InvalidReference", "ReferenceDocumentId is required.");
        if (request.Lines is null || request.Lines.Count == 0)
            return Result<Guid>.Failure("NoLines", "At least one movement line is required.");
        if (string.IsNullOrWhiteSpace(request.ReferenceDocumentNumber))
            return Result<Guid>.Failure("InvalidReference", "ReferenceDocumentNumber is required.");

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
                return Result<Guid>.Failure("InvalidQuantity", "Quantity must always be positive.");
        }

        var alreadyPosted = await context.InventoryTransactions.AnyAsync(
            t => t.TenantId == request.TenantId
                 && t.TransactionType == request.TransactionType
                 && t.ReferenceDocumentId == request.ReferenceDocumentId,
            cancellationToken);
        if (alreadyPosted)
        {
            var existingId = await context.InventoryTransactions
                .Where(t => t.TenantId == request.TenantId
                            && t.TransactionType == request.TransactionType
                            && t.ReferenceDocumentId == request.ReferenceDocumentId)
                .Select(t => t.Id)
                .FirstAsync(cancellationToken);
            logger.LogInformation(
                "Idempotent inventory post for {Type} / {RefId} → {TxId}",
                request.TransactionType,
                request.ReferenceDocumentId,
                existingId);
            return Result<Guid>.Success(existingId);
        }

        var allowNegative = await context.InventorySettings.AsNoTracking()
            .Where(s => s.TenantId == request.TenantId)
            .Select(s => (bool?)s.AllowNegativeStock)
            .FirstOrDefaultAsync(cancellationToken) ?? false;

        var transaction = new InventoryTransaction(
            request.TenantId,
            request.TransactionType,
            request.ReferenceDocumentNumber.Trim(),
            request.ReferenceDocumentId,
            request.Notes,
            request.TransactionDate);

        context.InventoryTransactions.Add(transaction);

        foreach (var line in request.Lines)
        {
            var unitCost = await costing.ResolveUnitCostAsync(
                request.TenantId,
                line.InventoryItemId,
                line.WarehouseId,
                request.MovementType,
                line.Quantity,
                line.ExplicitUnitCost,
                line.AdjIncreasesOnHand,
                cancellationToken);

            var movement = transaction.AddMovement(
                line.InventoryItemId,
                line.WarehouseId,
                line.WarehouseBinId,
                line.Quantity,
                request.MovementType,
                unitCost,
                line.InventoryBatchId,
                line.AdjIncreasesOnHand);

            var balance = await GetOrCreateBalanceAsync(
                request.TenantId,
                line.InventoryItemId,
                line.WarehouseId,
                cancellationToken);

            ApplyToBalance(balance, request.MovementType, line.Quantity, unitCost, line.AdjIncreasesOnHand, allowNegative);
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Inventory posted {MovementType}/{TransactionType} for document {RefId} as tx {TxId}",
            request.MovementType,
            request.TransactionType,
            request.ReferenceDocumentId,
            transaction.Id);

        return Result<Guid>.Success(transaction.Id);
    }

    public async Task ReserveAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid inventoryItemId,
        decimal quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0) return;

        var allowNegative = await context.InventorySettings.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .Select(s => (bool?)s.AllowNegativeStock)
            .FirstOrDefaultAsync(cancellationToken) ?? false;

        var balance = await GetOrCreateBalanceAsync(tenantId, inventoryItemId, warehouseId, cancellationToken);
        balance.Reserve(quantity, allowNegative);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReleaseReservationAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid inventoryItemId,
        decimal quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0) return;

        var balance = await context.InventoryBalances
            .FirstOrDefaultAsync(
                b => b.TenantId == tenantId
                     && b.InventoryItemId == inventoryItemId
                     && b.WarehouseId == warehouseId,
                cancellationToken);
        if (balance is null) return;

        balance.ReleaseReservation(quantity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task FulfillReservationAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid inventoryItemId,
        decimal quantity,
        Guid referenceDocumentId,
        string referenceDocumentNumber,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0) return;

        // Release soft hold then post OUT through the same pipeline (single balance writer).
        var balance = await GetOrCreateBalanceAsync(tenantId, inventoryItemId, warehouseId, cancellationToken);
        balance.ReleaseReservation(quantity);
        await context.SaveChangesAsync(cancellationToken);

        var baseUnitId = await context.InventoryItems.AsNoTracking()
            .Where(i => i.Id == inventoryItemId)
            .Select(i => i.BaseUnitId)
            .FirstOrDefaultAsync(cancellationToken);

        await ApplyMovementAsync(new InventoryMovementRequest(
            tenantId,
            InventoryMovementType.OUT,
            TransactionType.SalesConsumption,
            referenceDocumentId,
            referenceDocumentNumber,
            [
                new InventoryMovementLine(
                    inventoryItemId,
                    warehouseId,
                    baseUnitId,
                    quantity)
            ],
            Notes: "Reservation fulfillment"), cancellationToken);
    }

    private async Task<InventoryBalance> GetOrCreateBalanceAsync(
        Guid tenantId,
        Guid inventoryItemId,
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        var balance = await context.InventoryBalances
            .FirstOrDefaultAsync(
                b => b.TenantId == tenantId
                     && b.InventoryItemId == inventoryItemId
                     && b.WarehouseId == warehouseId,
                cancellationToken);

        if (balance is not null) return balance;

        balance = new InventoryBalance(tenantId, inventoryItemId, warehouseId);
        context.InventoryBalances.Add(balance);
        return balance;
    }

    private static void ApplyToBalance(
        InventoryBalance balance,
        InventoryMovementType movementType,
        decimal quantity,
        decimal unitCost,
        bool adjIncreasesOnHand,
        bool allowNegative)
    {
        switch (movementType)
        {
            case InventoryMovementType.IN:
            case InventoryMovementType.TRI:
                balance.ApplyInbound(quantity, unitCost);
                break;
            case InventoryMovementType.OUT:
            case InventoryMovementType.TRO:
                balance.ApplyOutbound(quantity, allowNegative);
                break;
            case InventoryMovementType.ADJ:
            case InventoryMovementType.REV:
                if (adjIncreasesOnHand)
                    balance.ApplyInbound(quantity, unitCost);
                else
                    balance.ApplyOutbound(quantity, allowNegative);
                break;
            default:
                throw new InvalidOperationException($"Unsupported movement type: {movementType}");
        }
    }
}
