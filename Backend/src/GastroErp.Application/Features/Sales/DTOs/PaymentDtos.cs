using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Sales.DTOs;

// ─── Payment DTOs ─────────────────────────────────────────────────────────────

public record CreatePaymentDto(
    Guid SalesOrderId,
    Guid CashierShiftId,
    PaymentMethodType PaymentMethod,
    decimal Amount,
    decimal TipAmount = 0,
    string? ReferenceNumber = null,
    string? GatewayTransactionId = null
);

public record RefundPaymentDto(
    decimal Amount,
    PaymentMethodType RefundMethod,
    string Reason,
    bool RequiresApproval = false
);

public record VoidPaymentDto(string Reason);

public record PaymentDto(
    Guid Id,
    string ReceiptNumber,
    Guid BranchId,
    Guid CashierShiftId,
    PaymentMethodType PaymentMethod,
    PaymentStatus Status,
    decimal Amount,
    decimal TipAmount,
    string Currency,
    string? ReferenceNumber,
    Guid ProcessedBy,
    DateTimeOffset ProcessedAt,
    IReadOnlyList<PaymentAllocationDto> Allocations
);

public record PaymentAllocationDto(
    Guid Id,
    Guid SalesOrderId,
    decimal AllocatedAmount,
    string Currency
);

public record RefundDto(
    Guid Id,
    Guid PaymentId,
    Guid SalesOrderId,
    decimal RefundAmount,
    PaymentMethodType RefundMethod,
    RefundStatus Status,
    string Reason,
    DateTimeOffset? ProcessedAt
);

public record PaymentFilterDto(
    Guid? BranchId = null,
    Guid? CashierShiftId = null,
    Guid? SalesOrderId = null,
    PaymentStatus? Status = null,
    PaymentMethodType? PaymentMethod = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = 1,
    int PageSize = 20
);

// ─── Cash Register DTOs ─────────────────────────────────────────────────────────

public record CreateCashRegisterDto(
    Guid BranchId,
    string NameAr,
    string Code,
    string? NameEn = null,
    decimal DefaultOpeningFloat = 0
);

public record OpenCashRegisterDto(decimal OpeningBalance);

public record CloseCashRegisterDto(decimal ActualBalance);

public record CashRegisterDto(
    Guid Id,
    Guid BranchId,
    string NameAr,
    string? NameEn,
    string Code,
    bool IsActive,
    RegisterStatus Status,
    decimal CurrentBalance,
    decimal OpeningBalance,
    decimal ExpectedBalance,
    DateTimeOffset? OpenedAt
);

public record CashRegisterFilterDto(
    Guid? BranchId = null,
    RegisterStatus? Status = null,
    int Page = 1,
    int PageSize = 20
);

// ─── Shift DTOs ───────────────────────────────────────────────────────────────

public record OpenShiftDto(
    Guid BranchId,
    Guid CashRegisterId,
    Guid DeviceId,
    decimal OpeningFloat,
    string? Notes = null
);

public record CloseShiftDto(decimal ActualCash, string? Notes = null);

public record ReconcileShiftDto(decimal VarianceThreshold = 100);

public record CashierShiftDto(
    Guid Id,
    Guid BranchId,
    Guid CashRegisterId,
    Guid DeviceId,
    Guid CashierId,
    string ShiftNumber,
    ShiftStatus Status,
    decimal OpeningFloat,
    decimal ExpectedCash,
    decimal ActualCash,
    decimal Variance,
    ReconciliationStatus ReconciliationStatus,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt
);

public record ShiftFilterDto(
    Guid? BranchId = null,
    Guid? CashierId = null,
    ShiftStatus? Status = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = 1,
    int PageSize = 20
);

// ─── Cash Movement DTOs ───────────────────────────────────────────────────────

public record CreateCashMovementDto(
    Guid CashierShiftId,
    CashMovementType MovementType,
    decimal Amount,
    string Reason,
    string? ReferenceDocument = null
);

public record CashMovementDto(
    Guid Id,
    Guid CashierShiftId,
    Guid CashRegisterId,
    CashMovementType MovementType,
    decimal Amount,
    string Reason,
    string? ReferenceDocument,
    Guid CreatedByUser,
    DateTimeOffset CreatedAtMovement
);

public record CashMovementFilterDto(
    Guid? CashierShiftId = null,
    Guid? CashRegisterId = null,
    CashMovementType? MovementType = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = 1,
    int PageSize = 20
);
