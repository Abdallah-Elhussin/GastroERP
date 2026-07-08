using FluentValidation;
using GastroErp.Application.Features.Inventory.Commands;

namespace GastroErp.Application.Features.Inventory.Validators;

public class CreateRecipeCommandValidator : AbstractValidator<CreateRecipeCommand>
{
    public CreateRecipeCommandValidator()
    {
        RuleFor(x => x.Dto.ProductId).NotEmpty();
        RuleFor(x => x.Dto.Yield).GreaterThan(0);
    }
}

public class AddRecipeIngredientCommandValidator : AbstractValidator<AddRecipeIngredientCommand>
{
    public AddRecipeIngredientCommandValidator()
    {
        RuleFor(x => x.RecipeId).NotEmpty();
        RuleFor(x => x.Dto.InventoryItemId).NotEmpty();
        RuleFor(x => x.Dto.UnitId).NotEmpty();
        RuleFor(x => x.Dto.Quantity).GreaterThan(0);
    }
}
