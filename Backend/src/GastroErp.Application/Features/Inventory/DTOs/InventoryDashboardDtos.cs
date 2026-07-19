using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Inventory.DTOs;

/// <summary>
/// Phase F — Inventory dashboard summary (single round-trip for KPI widgets).
/// </summary>
public record InventoryDashboardDto(
    int TotalItems,
    int ActiveItems,
    int InactiveItems,
    int CategoryCount,
    int LowStockWatchlist,
    int WarehouseCount,
    int ActiveWarehouses,
    int OpenTransfers,
    int OpenStockCounts,
    int ActiveReservations,
    int DraftGoodsReceipts,
    int UncompletedWaste,
    IReadOnlyList<InventoryDashboardWarehouseDto> Warehouses,
    IReadOnlyList<InventoryDashboardActivityDto> RecentActivities,
    IReadOnlyList<InventoryDashboardAlertDto> Alerts,
    IReadOnlyList<InventoryDashboardTopMoverDto> TopMovers,
    IReadOnlyList<InventoryDashboardCategorySliceDto> CategoryDistribution
);

public record InventoryDashboardWarehouseDto(
    Guid Id,
    string NameAr,
    string? NameEn,
    string? Code,
    WarehouseType WarehouseType,
    bool IsActive
);

public record InventoryDashboardActivityDto(
    Guid Id,
    string Kind,
    string Reference,
    DateTimeOffset OccurredAt,
    string? Notes
);

public record InventoryDashboardAlertDto(
    string Code,
    string Severity,
    string MessageEn,
    string MessageAr,
    string? Path
);

public record InventoryDashboardTopMoverDto(
    Guid InventoryItemId,
    string NameAr,
    string? NameEn,
    decimal InQuantity,
    decimal OutQuantity
);

public record InventoryDashboardCategorySliceDto(
    Guid? CategoryId,
    string NameAr,
    string? NameEn,
    int ItemCount
);
