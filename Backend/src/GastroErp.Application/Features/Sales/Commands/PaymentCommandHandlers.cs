using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.Commands;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Services;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Sales.Commands;

// ─── Payment Handlers ─────────────────────────────────────────────────────────

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<PaymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IReceiptNumberGenerator _receiptGenerator;
    private readonly IAutoPostingService _autoPostingService;
    private readonly ILogger<CreatePaymentCommandHandler> _logger;

    public CreatePaymentCommandHandler(
        IApplicationDbContext context, IMapper mapper,
        IReceiptNumberGenerator receiptGenerator,
        IAutoPostingService autoPostingService,
        ILogger<CreatePaymentCommandHandler> logger)
        => (_context, _mapper, _receiptGenerator, _autoPostingService, _logger)
            = (context, mapper, receiptGenerator, autoPostingService, logger);

    public async Task<Result<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(o => o.Id == request.Dto.SalesOrderId && o.TenantId == request.TenantId, cancellationToken);
        if (order is null) return Result<PaymentDto>.Failure("NotFound", "Order not found.");

        var shift = await _context.CashierShifts
            .FirstOrDefaultAsync(s => s.Id == request.Dto.CashierShiftId, cancellationToken);
        if (shift is null) return Result<PaymentDto>.Failure("NotFound", "Shift not found.");

        shift.EnsureAcceptsPayments();

        var register = await _context.CashRegisters
            .FirstOrDefaultAsync(r => r.Id == shift.CashRegisterId, cancellationToken);
        if (register is null) return Result<PaymentDto>.Failure("NotFound", "Cash register not found.");
        if (register.Status != RegisterStatus.Open)
            return Result<PaymentDto>.Failure("RegisterClosed", "Cash register is not open.");

        order.EnsureCanAcceptPayment();
        if (request.Dto.Amount > order.RemainingBalance)
            return Result<PaymentDto>.Failure("PaymentExceedsBalance", "Payment exceeds remaining balance.");

        var receiptNumber = await _receiptGenerator.GenerateAsync(order.BranchId, cancellationToken);

        var payment = Payment.Create(
            request.TenantId, order.BranchId, shift.Id, receiptNumber,
            request.Dto.PaymentMethod, request.Dto.Amount, order.Currency,
            request.UserId, request.Dto.TipAmount,
            request.Dto.ReferenceNumber, request.Dto.GatewayTransactionId);

        payment.AllocateToOrder(order.Id, request.Dto.Amount);
        payment.Complete(order.Id);

        order.RecordPayment(request.Dto.Amount, shift.Id);

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.SalesOrderId == order.Id && i.Status == InvoiceStatus.Finalized, cancellationToken);
        invoice?.RecordPayment(request.Dto.Amount);

        if (request.Dto.PaymentMethod == PaymentMethodType.Cash)
        {
            shift.RecordMovement(CashMovementType.Sale, request.Dto.Amount, $"Sale {order.OrderNumber}", request.UserId, order.OrderNumber);
            register.RecordMovement(request.Dto.Amount, true);
        }

        _context.Payments.Add(payment);
        _context.SalesOrders.Update(order);
        if (invoice is not null) _context.Invoices.Update(invoice);
        _context.CashierShifts.Update(shift);
        _context.CashRegisters.Update(register);
        await _context.SaveChangesAsync(cancellationToken);

        var postingResult = await _autoPostingService.PostPaymentCompletedAsync(payment.Id, request.UserId, cancellationToken);
        if (!postingResult.IsSuccess)
            return Result<PaymentDto>.Failure(postingResult.ErrorCode!, postingResult.ErrorMessage!);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment {PaymentId} completed for order {OrderId}", payment.Id, order.Id);
        return Result<PaymentDto>.Success(_mapper.Map<PaymentDto>(payment));
    }
}

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result<RefundDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;

    public RefundPaymentCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<RefundPaymentCommandHandler> logger)
        => (_context, _mapper, _logger) = (context, mapper, logger);

    public async Task<Result<RefundDto>> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Include(p => p.Allocations)
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment is null) return Result<RefundDto>.Failure("NotFound", "Payment not found.");
        if (request.Dto.Amount > payment.RemainingRefundable)
            return Result<RefundDto>.Failure("RefundExceedsPaid", "Refund exceeds paid amount.");

        var allocation = payment.Allocations.FirstOrDefault();
        if (allocation is null) return Result<RefundDto>.Failure("NotFound", "No order allocation found.");

        var order = await _context.SalesOrders.FirstOrDefaultAsync(o => o.Id == allocation.SalesOrderId, cancellationToken);
        if (order is null) return Result<RefundDto>.Failure("NotFound", "Order not found.");

        var refund = payment.AddRefund(allocation.SalesOrderId, request.Dto.Amount,
            request.Dto.RefundMethod, request.Dto.Reason, request.UserId);

        if (request.Dto.RequiresApproval)
            refund.Approve(request.UserId);
        else
            refund.Process();

        payment.MarkRefunded(request.Dto.Amount, refund.Id, allocation.SalesOrderId);
        order.ReversePayment(request.Dto.Amount);

        var shift = await _context.CashierShifts.FirstOrDefaultAsync(s => s.Id == payment.CashierShiftId, cancellationToken);
        if (shift is not null && request.Dto.RefundMethod == PaymentMethodType.Cash)
        {
            shift.RecordMovement(CashMovementType.Refund, request.Dto.Amount, request.Dto.Reason, request.UserId, order.OrderNumber);
            _context.CashierShifts.Update(shift);

            var register = await _context.CashRegisters.FirstOrDefaultAsync(r => r.Id == shift.CashRegisterId, cancellationToken);
            if (register is not null)
            {
                register.RecordMovement(request.Dto.Amount, false);
                _context.CashRegisters.Update(register);
            }
        }

        _context.Payments.Update(payment);
        _context.SalesOrders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refund {RefundId} processed for payment {PaymentId}", refund.Id, payment.Id);
        return Result<RefundDto>.Success(_mapper.Map<RefundDto>(refund));
    }
}

public class VoidPaymentCommandHandler : IRequestHandler<VoidPaymentCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<VoidPaymentCommandHandler> _logger;

    public VoidPaymentCommandHandler(IApplicationDbContext context, ILogger<VoidPaymentCommandHandler> logger)
        => (_context, _logger) = (context, logger);

    public async Task<Result> Handle(VoidPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Include(p => p.Allocations)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment is null) return Result.Failure("NotFound", "Payment not found.");

        payment.Void(request.Dto.Reason, request.UserId);

        foreach (var allocation in payment.Allocations)
        {
            var order = await _context.SalesOrders.FirstOrDefaultAsync(o => o.Id == allocation.SalesOrderId, cancellationToken);
            if (order is not null)
            {
                order.ReversePayment(allocation.AllocatedAmount);
                _context.SalesOrders.Update(order);
            }
        }

        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Payment {PaymentId} voided", payment.Id);
        return Result.Success();
    }
}

// ─── Cash Register Handlers ─────────────────────────────────────────────────────

public class CreateCashRegisterCommandHandler : IRequestHandler<CreateCashRegisterCommand, Result<CashRegisterDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateCashRegisterCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<CashRegisterDto>> Handle(CreateCashRegisterCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.CashRegisters
            .AnyAsync(r => r.BranchId == request.Dto.BranchId && r.Code == request.Dto.Code.ToUpperInvariant(), cancellationToken);
        if (exists) return Result<CashRegisterDto>.Failure("RegisterAlreadyExists", "Register code already exists.");

        var register = CashRegister.Create(
            request.TenantId, request.Dto.BranchId, request.Dto.NameAr,
            request.Dto.Code, request.Dto.NameEn, request.Dto.DefaultOpeningFloat);

        _context.CashRegisters.Add(register);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<CashRegisterDto>.Success(_mapper.Map<CashRegisterDto>(register));
    }
}

public class OpenCashRegisterCommandHandler : IRequestHandler<OpenCashRegisterCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public OpenCashRegisterCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(OpenCashRegisterCommand request, CancellationToken cancellationToken)
    {
        var register = await _context.CashRegisters.FirstOrDefaultAsync(r => r.Id == request.RegisterId, cancellationToken);
        if (register is null) return Result.Failure("NotFound", "Cash register not found.");

        register.Open(request.Dto.OpeningBalance, request.UserId);
        _context.CashRegisters.Update(register);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CloseCashRegisterCommandHandler : IRequestHandler<CloseCashRegisterCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CloseCashRegisterCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(CloseCashRegisterCommand request, CancellationToken cancellationToken)
    {
        var register = await _context.CashRegisters.FirstOrDefaultAsync(r => r.Id == request.RegisterId, cancellationToken);
        if (register is null) return Result.Failure("NotFound", "Cash register not found.");

        register.Close(request.Dto.ActualBalance, request.UserId);
        _context.CashRegisters.Update(register);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SuspendCashRegisterCommandHandler : IRequestHandler<SuspendCashRegisterCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SuspendCashRegisterCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SuspendCashRegisterCommand request, CancellationToken cancellationToken)
    {
        var register = await _context.CashRegisters.FirstOrDefaultAsync(r => r.Id == request.RegisterId, cancellationToken);
        if (register is null) return Result.Failure("NotFound", "Cash register not found.");
        register.Suspend();
        _context.CashRegisters.Update(register);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ResumeCashRegisterCommandHandler : IRequestHandler<ResumeCashRegisterCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ResumeCashRegisterCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ResumeCashRegisterCommand request, CancellationToken cancellationToken)
    {
        var register = await _context.CashRegisters.FirstOrDefaultAsync(r => r.Id == request.RegisterId, cancellationToken);
        if (register is null) return Result.Failure("NotFound", "Cash register not found.");
        register.Resume();
        _context.CashRegisters.Update(register);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── Shift Handlers ───────────────────────────────────────────────────────────

public class OpenShiftCommandHandler : IRequestHandler<OpenShiftCommand, Result<CashierShiftDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IShiftNumberGenerator _shiftNumberGenerator;

    public OpenShiftCommandHandler(IApplicationDbContext context, IMapper mapper, IShiftNumberGenerator shiftNumberGenerator)
        => (_context, _mapper, _shiftNumberGenerator) = (context, mapper, shiftNumberGenerator);

    public async Task<Result<CashierShiftDto>> Handle(OpenShiftCommand request, CancellationToken cancellationToken)
    {
        var activeShift = await _context.CashierShifts
            .AnyAsync(s => s.CashierId == request.CashierId && s.DeviceId == request.Dto.DeviceId
                && (s.Status == ShiftStatus.Open || s.Status == ShiftStatus.Active || s.Status == ShiftStatus.Suspended), cancellationToken);
        if (activeShift) return Result<CashierShiftDto>.Failure("ShiftAlreadyOpen", "An active shift already exists.");

        var register = await _context.CashRegisters
            .FirstOrDefaultAsync(r => r.Id == request.Dto.CashRegisterId && r.TenantId == request.TenantId, cancellationToken);
        if (register is null) return Result<CashierShiftDto>.Failure("NotFound", "Cash register not found.");
        if (register.Status != RegisterStatus.Open)
            return Result<CashierShiftDto>.Failure("RegisterClosed", "Cash register must be open.");

        var shiftNumber = await _shiftNumberGenerator.GenerateAsync(request.Dto.BranchId, cancellationToken);
        var shift = CashierShift.Open(
            request.TenantId, request.Dto.BranchId, request.Dto.CashRegisterId,
            request.Dto.DeviceId, request.CashierId, shiftNumber, request.Dto.OpeningFloat, request.Dto.Notes);

        shift.Activate();
        _context.CashierShifts.Add(shift);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<CashierShiftDto>.Success(_mapper.Map<CashierShiftDto>(shift));
    }
}

public class CloseShiftCommandHandler : IRequestHandler<CloseShiftCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CloseShiftCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(CloseShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.CashierShifts.FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);
        if (shift is null) return Result.Failure("NotFound", "Shift not found.");

        if (shift.Status == ShiftStatus.Active || shift.Status == ShiftStatus.Suspended)
            shift.StartClosing();

        shift.Close(request.Dto.ActualCash, request.UserId);
        _context.CashierShifts.Update(shift);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class SuspendShiftCommandHandler : IRequestHandler<SuspendShiftCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SuspendShiftCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(SuspendShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.CashierShifts.FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);
        if (shift is null) return Result.Failure("NotFound", "Shift not found.");
        shift.Suspend();
        _context.CashierShifts.Update(shift);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ResumeShiftCommandHandler : IRequestHandler<ResumeShiftCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ResumeShiftCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ResumeShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.CashierShifts.FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);
        if (shift is null) return Result.Failure("NotFound", "Shift not found.");
        shift.Resume();
        _context.CashierShifts.Update(shift);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class ReconcileShiftCommandHandler : IRequestHandler<ReconcileShiftCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ReconcileShiftCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(ReconcileShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.CashierShifts.FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);
        if (shift is null) return Result.Failure("NotFound", "Shift not found.");

        shift.Reconcile(request.UserId, request.Dto.VarianceThreshold);
        _context.CashierShifts.Update(shift);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ─── Cash Movement Handlers ───────────────────────────────────────────────────

public class CreateCashMovementCommandHandler : IRequestHandler<CreateCashMovementCommand, Result<CashMovementDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateCashMovementCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<CashMovementDto>> Handle(CreateCashMovementCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.CashierShifts.FirstOrDefaultAsync(s => s.Id == request.Dto.CashierShiftId, cancellationToken);
        if (shift is null) return Result<CashMovementDto>.Failure("NotFound", "Shift not found.");

        var movement = shift.RecordMovement(
            request.Dto.MovementType, request.Dto.Amount,
            request.Dto.Reason, request.UserId, request.Dto.ReferenceDocument);

        var register = await _context.CashRegisters.FirstOrDefaultAsync(r => r.Id == shift.CashRegisterId, cancellationToken);
        if (register is not null)
        {
            var isInflow = request.Dto.MovementType is CashMovementType.CashIn or CashMovementType.SafeWithdrawal or CashMovementType.Float;
            register.RecordMovement(request.Dto.Amount, isInflow);
            _context.CashRegisters.Update(register);
        }

        _context.CashierShifts.Update(shift);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<CashMovementDto>.Success(_mapper.Map<CashMovementDto>(movement));
    }
}
