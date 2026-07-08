using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Domain.Entities.Sales;

namespace GastroErp.Application.Features.Sales.Services;

public interface IMenuPricingService
{
    Task<ProductPriceSnapshot?> GetProductPriceAsync(
        Guid productId, Guid tenantId, Guid? priceLevelId, CancellationToken ct = default);

    Task<ModifierPriceSnapshot?> GetModifierPriceAsync(
        Guid modifierId, Guid tenantId, CancellationToken ct = default);
}

public interface IOrderNumberGenerator
{
    Task<string> GenerateAsync(Guid branchId, string? branchCode, CancellationToken ct = default);
}

public interface IOrderInventoryService
{
    Task ReserveStockForOrderAsync(SalesOrderContext order, CancellationToken ct = default);
    Task ReleaseStockForOrderAsync(string orderNumber, CancellationToken ct = default);
}

public interface IReceiptNumberGenerator
{
    Task<string> GenerateAsync(Guid branchId, CancellationToken ct = default);
}

public interface IShiftNumberGenerator
{
    Task<string> GenerateAsync(Guid branchId, CancellationToken ct = default);
}

public interface IKitchenRoutingService
{
    Task RouteOrderAsync(Guid orderId, CancellationToken ct = default);
}

public interface ITableService
{
    Task<RestaurantTable?> GetTableByIdAsync(Guid tableId, CancellationToken ct = default);
    Task OccupyTableAsync(Guid tableId, Guid orderId, CancellationToken ct = default);
    Task ReleaseTableForOrderAsync(Guid? tableId, CancellationToken ct = default);
}

public record SalesOrderContext(
    Guid TenantId,
    Guid BranchId,
    string OrderNumber,
    IReadOnlyList<SalesOrderItemContext> Items
);

public record SalesOrderItemContext(Guid ProductId, decimal Quantity);
