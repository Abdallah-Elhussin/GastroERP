using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Delivery.DTOs;

// ─── Shared ───────────────────────────────────────────────────────────────────

public record DeliveryAddressDto(
    string CustomerName,
    string CustomerPhone,
    string DeliveryAddress,
    string? AddressLine2 = null,
    string? City = null,
    string? Notes = null,
    decimal? Latitude = null,
    decimal? Longitude = null
);

// ─── Zone DTOs ────────────────────────────────────────────────────────────────

public record CreateDeliveryZoneDto(
    Guid BranchId, string NameAr, decimal CenterLatitude, decimal CenterLongitude,
    decimal RadiusKm, DeliveryZoneFeeType FeeType, decimal FixedFee, decimal FeePerKm,
    int EstimatedMinutes, string? NameEn = null
);

public record UpdateDeliveryZoneDto(
    string NameAr, decimal CenterLatitude, decimal CenterLongitude,
    decimal RadiusKm, DeliveryZoneFeeType FeeType, decimal FixedFee, decimal FeePerKm,
    int EstimatedMinutes, string? NameEn = null
);

public record DeliveryZoneDto(
    Guid Id, Guid BranchId, string NameAr, string? NameEn,
    decimal CenterLatitude, decimal CenterLongitude, decimal RadiusKm,
    DeliveryZoneFeeType FeeType, decimal FixedFee, decimal FeePerKm,
    int EstimatedMinutes, bool IsActive
);

public record DeliveryFeeQuoteDto(
    Guid ZoneId, string ZoneName, decimal DeliveryFee, int EstimatedMinutes
);

// ─── Driver DTOs ──────────────────────────────────────────────────────────────

public record CreateDeliveryDriverDto(
    Guid BranchId, string NameAr, string Phone,
    Guid? UserId = null, string? NameEn = null, string? VehiclePlate = null
);

public record UpdateDeliveryDriverDto(
    string NameAr, string Phone, Guid? UserId, string? NameEn, string? VehiclePlate
);

public record UpdateDriverStatusDto(DriverStatus Status);

public record DeliveryDriverDto(
    Guid Id, Guid BranchId, Guid? UserId, string NameAr, string? NameEn,
    string Phone, DriverStatus Status, string? VehiclePlate, bool IsActive
);

// ─── Delivery Order DTOs ──────────────────────────────────────────────────────

public record CreateDeliveryOrderDto(
    Guid SalesOrderId,
    DeliveryAddressDto Address,
    DeliveryPaymentMode PaymentMode,
    DeliveryPriority Priority = DeliveryPriority.Normal,
    DeliveryProviderType ProviderType = DeliveryProviderType.Internal,
    Guid? DeliveryZoneId = null,
    DateTimeOffset? ScheduledAt = null,
    string? ExternalProviderCode = null,
    string? ExternalOrderReference = null
);

public record CreateOrderDeliveryDto(
    DeliveryAddressDto Address,
    DeliveryPaymentMode PaymentMode,
    DeliveryPriority Priority = DeliveryPriority.Normal,
    Guid? DeliveryZoneId = null,
    DateTimeOffset? ScheduledAt = null
);

public record AssignDeliveryDto(Guid DriverId);

public record PickUpDeliveryDto(Guid DriverId, decimal? Latitude = null, decimal? Longitude = null);

public record CompleteDeliveryDto(
    Guid DriverId, Guid? CashierShiftId = null,
    decimal? Latitude = null, decimal? Longitude = null);

public record FailDeliveryDto(string Reason);

public record CancelDeliveryDto(string Reason);

public record DeliveryOrderDto(
    Guid Id, Guid SalesOrderId, string DeliveryNumber, DeliveryStatus Status,
    DeliveryPriority Priority, DeliveryPaymentMode PaymentMode, DeliveryProviderType ProviderType,
    string CustomerName, string CustomerPhone, string DeliveryAddress,
    decimal DeliveryFee, int EstimatedMinutes, bool IsReadyForPickup,
    Guid? CurrentDriverId, Guid? DeliveryZoneId,
    DateTimeOffset? AssignedAt, DateTimeOffset? PickedUpAt, DateTimeOffset? DeliveredAt,
    string? ExternalProviderCode, string? ExternalOrderReference
);

public record DeliveryOrderDetailDto(
    Guid Id, Guid SalesOrderId, string DeliveryNumber, DeliveryStatus Status,
    DeliveryPriority Priority, DeliveryPaymentMode PaymentMode, DeliveryProviderType ProviderType,
    string CustomerName, string CustomerPhone, string DeliveryAddress,
    string? AddressLine2, string? City, string? DeliveryNotes,
    decimal? Latitude, decimal? Longitude,
    decimal DeliveryFee, int EstimatedMinutes, bool IsReadyForPickup,
    Guid? CurrentDriverId, Guid? DeliveryZoneId,
    DateTimeOffset? AssignedAt, DateTimeOffset? PickedUpAt, DateTimeOffset? DeliveredAt,
    string? FailureReason,
    IReadOnlyList<DeliveryTrackingEventDto> TrackingEvents
);

public record DeliveryTrackingEventDto(
    DeliveryStatus Status, string Notes, DateTimeOffset OccurredAt
);

public record DeliveryOrderFilterDto(
    Guid? BranchId = null, DeliveryStatus? Status = null, Guid? DriverId = null,
    bool? ReadyForPickup = null, int Page = 1, int PageSize = 20
);
