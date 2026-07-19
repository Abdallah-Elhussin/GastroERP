using FluentValidation;
using GastroErp.Application.Features.Inventory.ValuationGroups.Commands;

namespace GastroErp.Application.Features.Inventory.ValuationGroups;

public sealed class CreateInventoryValuationGroupCommandValidator
    : AbstractValidator<CreateInventoryValuationGroupCommand>
{
    public CreateInventoryValuationGroupCommandValidator()
    {
        RuleFor(x => x.Request.TenantId).NotEmpty();
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.NameEn).MaximumLength(200);
        RuleFor(x => x.Request.Description).MaximumLength(500);
    }
}

public sealed class UpdateInventoryValuationGroupCommandValidator
    : AbstractValidator<UpdateInventoryValuationGroupCommand>
{
    public UpdateInventoryValuationGroupCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.TenantId).NotEmpty();
        RuleFor(x => x.Request.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.NameEn).MaximumLength(200);
        RuleFor(x => x.Request.Description).MaximumLength(500);
    }
}
