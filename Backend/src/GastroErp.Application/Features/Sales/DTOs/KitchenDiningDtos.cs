using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Sales.DTOs;

// ─── Kitchen DTOs ─────────────────────────────────────────────────────────────

public record CreateKitchenStationDto(
    Guid BranchId,
    string NameAr,
    KitchenStationType StationType,
    string? NameEn = null,
    Guid? DeviceId = null,
    Guid? CategoryId = null,
    int SortOrder = 0
);

public record UpdateKitchenStationDto(
    string NameAr,
    KitchenStationType StationType,
    string? NameEn = null,
    Guid? DeviceId = null,
    Guid? CategoryId = null,
    int SortOrder = 0
);

public record KitchenStationDto(
    Guid Id,
    Guid BranchId,
    string NameAr,
    string? NameEn,
    KitchenStationType StationType,
    Guid? DeviceId,
    Guid? CategoryId,
    bool IsActive,
    int SortOrder
);

public record KitchenTicketDto(
    Guid Id,
    Guid SalesOrderId,
    string TicketNumber,
    Guid KitchenStationId,
    KitchenTicketStatus Status,
    int Priority,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    int? EstimatedPrepMinutes,
    IReadOnlyList<KitchenTicketItemDto> Items
);

public record KitchenTicketItemDto(
    Guid Id,
    Guid OrderItemId,
    string ProductNameAr,
    string? ProductNameEn,
    decimal Quantity,
    string? ModifiersSummary,
    KitchenItemStatus Status
);

public record KitchenTicketFilterDto(
    Guid? BranchId = null,
    Guid? StationId = null,
    KitchenTicketStatus? Status = null,
    int Page = 1,
    int PageSize = 20
);

public record KdsTicketItemViewDto(
    Guid Id,
    string Name,
    decimal Quantity,
    IReadOnlyList<string> Notes
);

public record KdsTicketViewDto(
    Guid Id,
    string TicketNumber,
    string TableLabel,
    string OrderType,
    Guid KitchenStationId,
    KitchenStationType StationType,
    string StationNameAr,
    string? StationNameEn,
    string KdsStatus,
    int TimerSeconds,
    DateTimeOffset CreatedAt,
    IReadOnlyList<KdsTicketItemViewDto> Items
);

public record DispatchPosToKitchenDto(
    string OrderReference,
    string TableLabel,
    string OrderType,
    IReadOnlyList<DispatchPosKitchenItemDto> Items
);

public record DispatchPosKitchenItemDto(
    string Name,
    decimal Quantity,
    string? Notes,
    string? CategoryKey = null
);

// ─── Floor Plan DTOs ──────────────────────────────────────────────────────────

public record CreateFloorPlanDto(Guid BranchId, string NameAr, string? NameEn = null, int SortOrder = 0);

public record AddDiningAreaDto(string NameAr, string? NameEn, int Capacity, int SortOrder = 0);

public record AddRestaurantTableDto(
    string TableNumber,
    int Capacity,
    TableShape Shape,
    string? NameAr = null,
    string? NameEn = null,
    int? PositionX = null,
    int? PositionY = null
);

public record FloorPlanDto(
    Guid Id,
    Guid BranchId,
    string NameAr,
    string? NameEn,
    bool IsActive,
    int SortOrder,
    int AreaCount
);

public record FloorPlanDetailDto(
    Guid Id,
    Guid BranchId,
    string NameAr,
    string? NameEn,
    bool IsActive,
    IReadOnlyList<DiningAreaDto> DiningAreas
);

public record DiningAreaDto(
    Guid Id,
    string NameAr,
    string? NameEn,
    int Capacity,
    int SortOrder,
    IReadOnlyList<RestaurantTableDto> Tables
);

public record RestaurantTableDto(
    Guid Id,
    Guid DiningAreaId,
    string TableNumber,
    string? NameAr,
    string? NameEn,
    int Capacity,
    TableStatus Status,
    Guid? CurrentOrderId,
    int? PositionX,
    int? PositionY,
    TableShape Shape
);

public record UpdateTableStatusDto(TableStatus Status);

public record OccupyTableDto(Guid OrderId);

// ─── Reservation DTOs ─────────────────────────────────────────────────────────

public record CreateTableReservationDto(
    Guid BranchId,
    string CustomerName,
    string CustomerPhone,
    int GuestCount,
    DateTimeOffset ReservationDate,
    int DurationMinutes = 120,
    Guid? TableId = null,
    string? Notes = null
);

public record CancelTableReservationDto(string Reason);

public record TableReservationDto(
    Guid Id,
    Guid BranchId,
    Guid? TableId,
    string CustomerName,
    string CustomerPhone,
    int GuestCount,
    DateTimeOffset ReservationDate,
    int DurationMinutes,
    TableReservationStatus Status,
    string? Notes,
    Guid? SalesOrderId,
    DateTimeOffset? ConfirmedAt
);

public record TableReservationFilterDto(
    Guid? BranchId = null,
    TableReservationStatus? Status = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = 1,
    int PageSize = 20
);

public record SeatReservationDto(
    Guid DeviceId,
    Guid? WaiterId = null,
    int? GuestCount = null
);
