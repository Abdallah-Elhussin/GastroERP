using FluentValidation;
using GastroErp.Application.Features.Menu.Commands;

namespace GastroErp.Application.Features.Menu.Validators;

public class CreateBranchMenuCommandValidator : AbstractValidator<CreateBranchMenuCommand>
{
    public CreateBranchMenuCommandValidator()
    {
        RuleFor(x => x.Dto.TenantId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.MenuId).NotEmpty();
    }
}

public class SetMenuAvailabilityCommandValidator : AbstractValidator<SetMenuAvailabilityCommand>
{
    public SetMenuAvailabilityCommandValidator()
    {
        RuleFor(x => x.BranchMenuId).NotEmpty();
        RuleFor(x => x.Dto.DayOfWeek).IsInEnum();
        RuleFor(x => x.Dto.EndTime).GreaterThan(x => x.Dto.StartTime)
            .WithMessage("EndTime must be after StartTime.");
    }
}
