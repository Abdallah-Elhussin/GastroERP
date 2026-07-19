using FluentValidation;
using GastroErp.Application.Features.Inventory.ItemTypes.Commands;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Inventory.ItemTypes.Validators;

public sealed class CreateInventoryItemTypeCommandValidator : AbstractValidator<CreateInventoryItemTypeCommand>
{
    public CreateInventoryItemTypeCommandValidator()
    {
        RuleFor(x => x.Request.TenantId).NotEmpty();
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.NameEn).MaximumLength(200);
        RuleFor(x => x.Request.Description).MaximumLength(500);
        RuleFor(x => x.Request.Color).MaximumLength(30);
        RuleFor(x => x.Request.Category).IsInEnum();
        RuleFor(x => x.Request)
            .Must(r => !r.CodeStart.HasValue || !r.CodeEnd.HasValue || r.CodeStart <= r.CodeEnd)
            .WithMessage("CodeStart cannot be greater than CodeEnd.");
        RuleFor(x => x.Request)
            .Must(r => !r.CanSell || IsSellableCategory(r.Category))
            .WithMessage("Sellable types must be Menu Item, Finished Product, Bundle, or Service.");
        RuleFor(x => x.Request)
            .Must(r => !r.IsRecipe || r.IsInventory || r.IsProduction)
            .WithMessage("Recipe items require Inventory or Production.");
    }

    private static bool IsSellableCategory(InventoryItemTypeCategory category) =>
        category is InventoryItemTypeCategory.MenuItem
            or InventoryItemTypeCategory.FinishedProduct
            or InventoryItemTypeCategory.Bundle
            or InventoryItemTypeCategory.Service;
}

public sealed class UpdateInventoryItemTypeCommandValidator : AbstractValidator<UpdateInventoryItemTypeCommand>
{
    public UpdateInventoryItemTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.TenantId).NotEmpty();
        RuleFor(x => x.Request.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.NameEn).MaximumLength(200);
        RuleFor(x => x.Request.Description).MaximumLength(500);
        RuleFor(x => x.Request.Color).MaximumLength(30);
        RuleFor(x => x.Request.Category).IsInEnum();
        RuleFor(x => x.Request)
            .Must(r => !r.CodeStart.HasValue || !r.CodeEnd.HasValue || r.CodeStart <= r.CodeEnd)
            .WithMessage("CodeStart cannot be greater than CodeEnd.");
        RuleFor(x => x.Request)
            .Must(r => !r.CanSell || IsSellableCategory(r.Category))
            .WithMessage("Sellable types must be Menu Item, Finished Product, Bundle, or Service.");
        RuleFor(x => x.Request)
            .Must(r => !r.IsRecipe || r.IsInventory || r.IsProduction)
            .WithMessage("Recipe items require Inventory or Production.");
    }

    private static bool IsSellableCategory(InventoryItemTypeCategory category) =>
        category is InventoryItemTypeCategory.MenuItem
            or InventoryItemTypeCategory.FinishedProduct
            or InventoryItemTypeCategory.Bundle
            or InventoryItemTypeCategory.Service;
}

public sealed class DeleteInventoryItemTypeCommandValidator : AbstractValidator<DeleteInventoryItemTypeCommand>
{
    public DeleteInventoryItemTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
    }
}
