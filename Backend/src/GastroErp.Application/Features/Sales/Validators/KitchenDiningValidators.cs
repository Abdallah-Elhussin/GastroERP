using FluentValidation;
using GastroErp.Application.Features.Sales.Commands;

namespace GastroErp.Application.Features.Sales.Validators;

public class CreateKitchenStationCommandValidator : AbstractValidator<CreateKitchenStationCommand>
{
    public CreateKitchenStationCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class CreateFloorPlanCommandValidator : AbstractValidator<CreateFloorPlanCommand>
{
    public CreateFloorPlanCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class CreateTableReservationCommandValidator : AbstractValidator<CreateTableReservationCommand>
{
    public CreateTableReservationCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.CustomerPhone).NotEmpty();
        RuleFor(x => x.Dto.GuestCount).GreaterThan(0);
    }
}

public class CancelTableReservationCommandValidator : AbstractValidator<CancelTableReservationCommand>
{
    public CancelTableReservationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.Reason).NotEmpty().MinimumLength(3);
    }
}
