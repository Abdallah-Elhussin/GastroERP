using FluentValidation;
using GastroErp.Application.Features.Sales.Commands;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Sales.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CashierId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.DeviceId).NotEmpty();
        RuleFor(x => x.Dto.TableId).NotEmpty()
            .When(x => x.Dto.OrderType == OrderType.DineIn)
            .WithMessage("TableId is required for DineIn orders.");
    }
}

public class AddOrderItemCommandValidator : AbstractValidator<AddOrderItemCommand>
{
    public AddOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Dto.ProductId).NotEmpty();
        RuleFor(x => x.Dto.Quantity).GreaterThan(0);
    }
}

public class ApplyOrderDiscountCommandValidator : AbstractValidator<ApplyOrderDiscountCommand>
{
    public ApplyOrderDiscountCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Dto.Value).GreaterThan(0);
        RuleFor(x => x.Dto.Value).LessThanOrEqualTo(100)
            .When(x => x.Dto.DiscountType == DiscountType.Percentage);
    }
}

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Dto.Reason).NotEmpty().MinimumLength(3);
    }
}

public class VoidOrderItemCommandValidator : AbstractValidator<VoidOrderItemCommand>
{
    public VoidOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Dto.Reason).NotEmpty().MinimumLength(3);
    }
}

public class ReopenOrderCommandValidator : AbstractValidator<ReopenOrderCommand>
{
    public ReopenOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Dto.Reason).NotEmpty().MinimumLength(3);
    }
}
