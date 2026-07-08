using GastroErp.Domain.Enums;
using GastroErp.Application.Features.Delivery.DTOs;

namespace GastroErp.Application.Features.Sales.DTOs;

// ─── Request DTOs ─────────────────────────────────────────────────────────────

public record CreateOrderDto(
    Guid BranchId,
    Guid DeviceId,
    OrderType OrderType,
    SalesChannel SalesChannel,
    Guid? TableId = null,
    int? GuestCount = null,
    Guid? WaiterId = null,
    Guid? PriceLevelId = null,
    string? Notes = null,
    CreateOrderDeliveryDto? Delivery = null
);

public record AddOrderItemDto(
    Guid ProductId,
    decimal Quantity,
    string? Notes = null,
    List<AddOrderLineModifierDto>? Modifiers = null
);

public record AddOrderLineModifierDto(Guid ModifierId, int Quantity = 1);

public record ApplyOrderDiscountDto(
    DiscountType DiscountType,
    decimal Value,
    string? Description = null
);

public record CancelOrderDto(string Reason);

public record ReopenOrderDto(string Reason);

public record VoidOrderItemDto(string Reason);

public record UpdateOrderStatusDto(OrderStatus TargetStatus);

public record OrderFilterDto(
    Guid? BranchId = null,
    OrderStatus? Status = null,
    OrderType? OrderType = null,
    SalesChannel? SalesChannel = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20
);

// ─── Response DTOs ────────────────────────────────────────────────────────────

public record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid BranchId,
    OrderType OrderType,
    SalesChannel SalesChannel,
    OrderStatus Status,
    decimal SubTotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal GrandTotal,
    string Currency,
    decimal PaidAmount,
    decimal RemainingBalance,
    OrderPaymentStatus PaymentStatus,
    int ItemCount,
    DateTimeOffset CreatedAt
);

public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    Guid BranchId,
    OrderType OrderType,
    SalesChannel SalesChannel,
    OrderStatus Status,
    decimal GrandTotal,
    string Currency,
    decimal PaidAmount,
    decimal RemainingBalance,
    OrderPaymentStatus PaymentStatus,
    int ItemCount,
    DateTimeOffset CreatedAt
);

public record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    Guid TenantId,
    Guid CompanyId,
    Guid BranchId,
    OrderType OrderType,
    SalesChannel SalesChannel,
    OrderStatus Status,
    Guid? TableId,
    Guid CashierId,
    Guid? WaiterId,
    Guid DeviceId,
    int? GuestCount,
    string? Notes,
    decimal SubTotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal ServiceChargeTotal,
    decimal GrandTotal,
    string Currency,
    decimal PaidAmount,
    decimal RemainingBalance,
    OrderPaymentStatus PaymentStatus,
    Guid? PriceLevelId,
    Guid? CashierShiftId,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    SyncStatus SyncStatus,
    DateTimeOffset CreatedAt,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<OrderDiscountDto> Discounts,
    IReadOnlyList<OrderTaxDto> Taxes,
    IReadOnlyList<OrderStatusHistoryDto> StatusHistory
);

public record OrderItemDto(
    Guid Id,
    int LineNumber,
    Guid ProductId,
    Guid? ComboMealId,
    string ProductNameAr,
    string? ProductNameEn,
    string? Sku,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string Currency,
    string? Notes,
    KitchenItemStatus KitchenStatus,
    bool IsVoided,
    string? VoidReason,
    IReadOnlyList<OrderLineModifierDto> Modifiers
);

public record OrderLineModifierDto(
    Guid Id,
    Guid ModifierId,
    string ModifierNameAr,
    string? ModifierNameEn,
    decimal ExtraPrice,
    int Quantity
);

public record OrderDiscountDto(
    Guid Id,
    DiscountType DiscountType,
    string? Description,
    decimal Amount,
    string Currency
);

public record OrderTaxDto(
    Guid Id,
    string TaxNameAr,
    string? TaxNameEn,
    decimal TaxRate,
    decimal TaxableAmount,
    decimal TaxAmount,
    string Currency,
    bool IsInclusive
);

public record OrderStatusHistoryDto(
    Guid Id,
    OrderStatus FromStatus,
    OrderStatus ToStatus,
    DateTimeOffset ChangedAt,
    Guid ChangedBy,
    string? Reason,
    Guid DeviceId
);

// ─── ACL Snapshots ────────────────────────────────────────────────────────────

public record ProductPriceSnapshot(
    Guid ProductId,
    string NameAr,
    string? NameEn,
    string? Sku,
    decimal UnitPrice,
    string Currency,
    bool IsAvailable
);

public record ModifierPriceSnapshot(
    Guid ModifierId,
    string NameAr,
    string? NameEn,
    decimal ExtraPrice
);
