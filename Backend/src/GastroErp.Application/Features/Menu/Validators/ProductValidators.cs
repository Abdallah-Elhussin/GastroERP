using FluentValidation;
using GastroErp.Application.Features.Menu.Commands;

namespace GastroErp.Application.Features.Menu.Validators;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Dto.TenantId).NotEmpty();
        RuleFor(x => x.Dto.CategoryId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.BasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.Currency).NotEmpty().Length(3);
    }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.PrepTimeMinutes).GreaterThanOrEqualTo(0);
    }
}

public class AddModifierGroupCommandValidator : AbstractValidator<AddModifierGroupCommand>
{
    public AddModifierGroupCommandValidator()
    {
        RuleFor(x => x.Dto.ProductId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.MinSelection).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.MaxSelection).GreaterThanOrEqualTo(x => x.Dto.MinSelection)
            .WithMessage("MaxSelection must be greater than or equal to MinSelection.");
    }
}

public class AddOptionGroupCommandValidator : AbstractValidator<AddOptionGroupCommand>
{
    public AddOptionGroupCommandValidator()
    {
        RuleFor(x => x.Dto.ProductId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
    }
}
