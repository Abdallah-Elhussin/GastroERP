using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Sales.Commands;

// ─── Payment Commands ─────────────────────────────────────────────────────────
public record CreatePaymentCommand(Guid TenantId, Guid UserId, CreatePaymentDto Dto) : IRequest<Result<PaymentDto>>;
public record RefundPaymentCommand(Guid PaymentId, Guid UserId, RefundPaymentDto Dto) : IRequest<Result<RefundDto>>;
public record VoidPaymentCommand(Guid PaymentId, Guid UserId, VoidPaymentDto Dto) : IRequest<Result>;

// ─── Cash Register Commands ───────────────────────────────────────────────────
public record CreateCashRegisterCommand(Guid TenantId, CreateCashRegisterDto Dto) : IRequest<Result<CashRegisterDto>>;
public record OpenCashRegisterCommand(Guid RegisterId, Guid UserId, OpenCashRegisterDto Dto) : IRequest<Result>;
public record CloseCashRegisterCommand(Guid RegisterId, Guid UserId, CloseCashRegisterDto Dto) : IRequest<Result>;
public record SuspendCashRegisterCommand(Guid RegisterId) : IRequest<Result>;
public record ResumeCashRegisterCommand(Guid RegisterId) : IRequest<Result>;

// ─── Shift Commands ───────────────────────────────────────────────────────────
public record OpenShiftCommand(Guid TenantId, Guid CashierId, OpenShiftDto Dto) : IRequest<Result<CashierShiftDto>>;
public record CloseShiftCommand(Guid ShiftId, Guid UserId, CloseShiftDto Dto) : IRequest<Result>;
public record SuspendShiftCommand(Guid ShiftId) : IRequest<Result>;
public record ResumeShiftCommand(Guid ShiftId) : IRequest<Result>;
public record ReconcileShiftCommand(Guid ShiftId, Guid UserId, ReconcileShiftDto Dto) : IRequest<Result>;

// ─── Cash Movement Commands ───────────────────────────────────────────────────
public record CreateCashMovementCommand(Guid UserId, CreateCashMovementDto Dto) : IRequest<Result<CashMovementDto>>;
