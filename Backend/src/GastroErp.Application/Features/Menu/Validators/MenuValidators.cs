using FluentValidation;
using GastroErp.Application.Features.Menu.Commands;

namespace GastroErp.Application.Features.Menu.Validators;

public class CreateMenuCommandValidator : AbstractValidator<CreateMenuCommand>
{
    public CreateMenuCommandValidator()
    {
        RuleFor(x => x.Dto.TenantId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.MenuType).IsInEnum();
        RuleFor(x => x.Dto.SalesChannel).IsInEnum();
        RuleFor(x => x.Dto.EndDate).GreaterThanOrEqualTo(x => x.Dto.StartDate)
            .When(x => x.Dto.StartDate.HasValue && x.Dto.EndDate.HasValue)
            .WithMessage("EndDate must be after StartDate.");
    }
}

public class UpdateMenuCommandValidator : AbstractValidator<UpdateMenuCommand>
{
    public UpdateMenuCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.EndDate).GreaterThanOrEqualTo(x => x.Dto.StartDate)
            .When(x => x.Dto.StartDate.HasValue && x.Dto.EndDate.HasValue)
            .WithMessage("EndDate must be after StartDate.");
    }
}

public class AddMenuSectionCommandValidator : AbstractValidator<AddMenuSectionCommand>
{
    public AddMenuSectionCommandValidator()
    {
        RuleFor(x => x.MenuId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.SortOrder).GreaterThanOrEqualTo(0);
    }
}
