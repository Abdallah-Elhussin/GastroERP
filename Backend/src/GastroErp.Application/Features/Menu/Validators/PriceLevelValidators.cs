using FluentValidation;
using GastroErp.Application.Features.Menu.Commands;

namespace GastroErp.Application.Features.Menu.Validators;

public class CreatePriceLevelCommandValidator : AbstractValidator<CreatePriceLevelCommand>
{
    public CreatePriceLevelCommandValidator()
    {
        RuleFor(x => x.Dto.TenantId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.SalesChannel).IsInEnum();
    }
}

public class UpdatePriceLevelCommandValidator : AbstractValidator<UpdatePriceLevelCommand>
{
    public UpdatePriceLevelCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.SalesChannel).IsInEnum();
    }
}
