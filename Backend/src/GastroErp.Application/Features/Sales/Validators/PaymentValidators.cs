using FluentValidation;
using GastroErp.Application.Features.Sales.Commands;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Sales.Validators;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Dto.SalesOrderId).NotEmpty();
        RuleFor(x => x.Dto.CashierShiftId).NotEmpty();
        RuleFor(x => x.Dto.Amount).GreaterThan(0);
        RuleFor(x => x.Dto.TipAmount).GreaterThanOrEqualTo(0);
    }
}

public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.Dto.Amount).GreaterThan(0);
        RuleFor(x => x.Dto.Reason).NotEmpty().MinimumLength(3);
    }
}

public class VoidPaymentCommandValidator : AbstractValidator<VoidPaymentCommand>
{
    public VoidPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.Dto.Reason).NotEmpty().MinimumLength(3);
    }
}

public class CreateCashRegisterCommandValidator : AbstractValidator<CreateCashRegisterCommand>
{
    public CreateCashRegisterCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(50);
    }
}

public class OpenCashRegisterCommandValidator : AbstractValidator<OpenCashRegisterCommand>
{
    public OpenCashRegisterCommandValidator()
    {
        RuleFor(x => x.RegisterId).NotEmpty();
        RuleFor(x => x.Dto.OpeningBalance).GreaterThanOrEqualTo(0);
    }
}

public class CloseCashRegisterCommandValidator : AbstractValidator<CloseCashRegisterCommand>
{
    public CloseCashRegisterCommandValidator()
    {
        RuleFor(x => x.RegisterId).NotEmpty();
        RuleFor(x => x.Dto.ActualBalance).GreaterThanOrEqualTo(0);
    }
}

public class OpenShiftCommandValidator : AbstractValidator<OpenShiftCommand>
{
    public OpenShiftCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CashierId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.CashRegisterId).NotEmpty();
        RuleFor(x => x.Dto.DeviceId).NotEmpty();
        RuleFor(x => x.Dto.OpeningFloat).GreaterThanOrEqualTo(0);
    }
}

public class CloseShiftCommandValidator : AbstractValidator<CloseShiftCommand>
{
    public CloseShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.Dto.ActualCash).GreaterThanOrEqualTo(0);
    }
}

public class CreateCashMovementCommandValidator : AbstractValidator<CreateCashMovementCommand>
{
    public CreateCashMovementCommandValidator()
    {
        RuleFor(x => x.Dto.CashierShiftId).NotEmpty();
        RuleFor(x => x.Dto.Amount).GreaterThan(0);
        RuleFor(x => x.Dto.Reason).NotEmpty().MinimumLength(3);
    }
}
