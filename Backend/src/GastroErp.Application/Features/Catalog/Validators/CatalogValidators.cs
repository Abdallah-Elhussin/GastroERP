using FluentValidation;
using GastroErp.Application.Features.Catalog.Commands;
using GastroErp.Application.Features.Catalog.DTOs;

namespace GastroErp.Application.Features.Catalog.Validators;

public sealed class CreateCatalogDraftDtoValidator : AbstractValidator<CreateCatalogDraftDto>
{
    public CreateCatalogDraftDtoValidator()
    {
        RuleFor(x => x.CatalogType).IsInEnum();
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameEn).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.NameEn));
    }
}

public sealed class UpdateCatalogGeneralInfoDtoValidator : AbstractValidator<UpdateCatalogGeneralInfoDto>
{
    public UpdateCatalogGeneralInfoDtoValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameEn).MaximumLength(200);
        RuleFor(x => x.Sku).MaximumLength(50);
        RuleFor(x => x.Barcode).MaximumLength(50);
        RuleFor(x => x.Keywords).MaximumLength(500);
        RuleFor(x => x.Brand).MaximumLength(100);
    }
}

public sealed class CreateCatalogDraftCommandValidator : AbstractValidator<CreateCatalogDraftCommand>
{
    public CreateCatalogDraftCommandValidator() => RuleFor(x => x.Dto).SetValidator(new CreateCatalogDraftDtoValidator());
}

public sealed class UpdateCatalogGeneralInfoCommandValidator : AbstractValidator<UpdateCatalogGeneralInfoCommand>
{
    public UpdateCatalogGeneralInfoCommandValidator() => RuleFor(x => x.Dto).SetValidator(new UpdateCatalogGeneralInfoDtoValidator());
}

public sealed class SaveCatalogInventoryDtoValidator : AbstractValidator<SaveCatalogInventoryDto>
{
    public SaveCatalogInventoryDtoValidator()
    {
        RuleFor(x => x.BaseUnitId).NotEmpty();
        RuleFor(x => x.MinStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SafetyStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReorderQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CostingMethod).IsInEnum();
    }
}

public sealed class SaveCatalogInventoryCommandValidator : AbstractValidator<SaveCatalogInventoryCommand>
{
    public SaveCatalogInventoryCommandValidator() => RuleFor(x => x.Dto).SetValidator(new SaveCatalogInventoryDtoValidator());
}

public sealed class SaveCatalogRecipeDtoValidator : AbstractValidator<SaveCatalogRecipeDto>
{
    public SaveCatalogRecipeDtoValidator()
    {
        RuleFor(x => x.Yield).GreaterThan(0);
        RuleFor(x => x.WastePercentage).InclusiveBetween(0, 99);
        RuleFor(x => x.PreparationTime).GreaterThanOrEqualTo(0);
    }
}

public sealed class SaveCatalogPosDtoValidator : AbstractValidator<SaveCatalogPosDto>
{
    public SaveCatalogPosDtoValidator()
    {
        RuleFor(x => x.MenuCategoryId).NotEmpty();
        RuleFor(x => x.PrepTimeMinutes).GreaterThanOrEqualTo(0);
    }
}

public sealed class SaveCatalogPricingDtoValidator : AbstractValidator<SaveCatalogPricingDto>
{
    public SaveCatalogPricingDtoValidator()
    {
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);
    }
}
