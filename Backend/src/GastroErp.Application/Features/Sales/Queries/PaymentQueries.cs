using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Sales.Queries;

public record GetPaymentByIdQuery(Guid Id) : IRequest<Result<PaymentDto>>;
public record GetPaymentsQuery(Guid TenantId, PaymentFilterDto Filter) : IRequest<PagedResult<PaymentDto>>;
public record GetPaymentsByOrderQuery(Guid OrderId) : IRequest<Result<IReadOnlyList<PaymentDto>>>;

public record GetCashRegisterByIdQuery(Guid Id) : IRequest<Result<CashRegisterDto>>;
public record GetCashRegistersQuery(Guid TenantId, CashRegisterFilterDto Filter) : IRequest<PagedResult<CashRegisterDto>>;
public record GetCurrentCashRegisterQuery(Guid BranchId) : IRequest<Result<CashRegisterDto>>;

public record GetShiftByIdQuery(Guid Id) : IRequest<Result<CashierShiftDto>>;
public record GetShiftsQuery(Guid TenantId, ShiftFilterDto Filter) : IRequest<PagedResult<CashierShiftDto>>;
public record GetCurrentShiftQuery(Guid CashierId, Guid DeviceId) : IRequest<Result<CashierShiftDto>>;

public record GetCashMovementsQuery(Guid TenantId, CashMovementFilterDto Filter) : IRequest<PagedResult<CashMovementDto>>;
