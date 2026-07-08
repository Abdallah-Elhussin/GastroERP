using FluentValidation;
using GastroErp.Application.Features.Delivery.Commands;

namespace GastroErp.Application.Features.Delivery.Validators;

public class CreateDeliveryZoneCommandValidator : AbstractValidator<CreateDeliveryZoneCommand>
{
    public CreateDeliveryZoneCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.RadiusKm).GreaterThan(0);
    }
}

public class CreateDeliveryDriverCommandValidator : AbstractValidator<CreateDeliveryDriverCommand>
{
    public CreateDeliveryDriverCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty();
        RuleFor(x => x.Dto.Phone).NotEmpty();
    }
}

public class CreateDeliveryOrderCommandValidator : AbstractValidator<CreateDeliveryOrderCommand>
{
    public CreateDeliveryOrderCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.SalesOrderId).NotEmpty();
        RuleFor(x => x.Dto.Address.CustomerName).NotEmpty();
        RuleFor(x => x.Dto.Address.CustomerPhone).NotEmpty();
        RuleFor(x => x.Dto.Address.DeliveryAddress).NotEmpty();
    }
}

public class AssignDeliveryCommandValidator : AbstractValidator<AssignDeliveryCommand>
{
    public AssignDeliveryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.DriverId).NotEmpty();
    }
}

public class CompleteDeliveryCommandValidator : AbstractValidator<CompleteDeliveryCommand>
{
    public CompleteDeliveryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.DriverId).NotEmpty();
    }
}

public class FailDeliveryCommandValidator : AbstractValidator<FailDeliveryCommand>
{
    public FailDeliveryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.Reason).NotEmpty().MinimumLength(3);
    }
}

public class CancelDeliveryCommandValidator : AbstractValidator<CancelDeliveryCommand>
{
    public CancelDeliveryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.Reason).NotEmpty().MinimumLength(3);
    }
}

public class UpdateDeliveryZoneCommandValidator : AbstractValidator<UpdateDeliveryZoneCommand>
{
    public UpdateDeliveryZoneCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty();
    }
}
