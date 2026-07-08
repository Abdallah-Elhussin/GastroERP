using FluentValidation;
using GastroErp.Application.Features.Menu.Commands;

namespace GastroErp.Application.Features.Menu.Validators;

public class CreateComboCommandValidator : AbstractValidator<CreateComboCommand>
{
    public CreateComboCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.ComboPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Dto.EndDate).GreaterThanOrEqualTo(x => x.Dto.StartDate)
            .When(x => x.Dto.StartDate.HasValue && x.Dto.EndDate.HasValue)
            .WithMessage("EndDate must be after StartDate.");
    }
}

public class UpdateComboCommandValidator : AbstractValidator<UpdateComboCommand>
{
    public UpdateComboCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.ComboPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Dto.EndDate).GreaterThanOrEqualTo(x => x.Dto.StartDate)
            .When(x => x.Dto.StartDate.HasValue && x.Dto.EndDate.HasValue)
            .WithMessage("EndDate must be after StartDate.");
    }
}
