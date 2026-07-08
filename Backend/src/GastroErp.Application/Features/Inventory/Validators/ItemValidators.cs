using FluentValidation;
using GastroErp.Application.Features.Inventory.Commands;

namespace GastroErp.Application.Features.Inventory.Validators;

public class CreateInventoryItemCommandValidator : AbstractValidator<CreateInventoryItemCommand>
{
    public CreateInventoryItemCommandValidator()
    {
        RuleFor(x => x.Dto.TenantId).NotEmpty();
        RuleFor(x => x.Dto.CategoryId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.BaseUnitId).NotEmpty();
    }
}

public class UpdateInventoryItemCommandValidator : AbstractValidator<UpdateInventoryItemCommand>
{
    public UpdateInventoryItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
    }
}

public class SetInventoryItemUnitsCommandValidator : AbstractValidator<SetInventoryItemUnitsCommand>
{
    public SetInventoryItemUnitsCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
