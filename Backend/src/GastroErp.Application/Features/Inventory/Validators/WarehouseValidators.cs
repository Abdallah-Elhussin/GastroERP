using FluentValidation;
using GastroErp.Application.Features.Inventory.Commands;

namespace GastroErp.Application.Features.Inventory.Validators;

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Dto.TenantId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.Code).MaximumLength(50);
        RuleFor(x => x.Dto.Address).MaximumLength(500);
        RuleFor(x => x.Dto.Phone).MaximumLength(50);
        RuleFor(x => x.Dto.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Dto.Email));
        RuleFor(x => x.Dto.ParentWarehouseId)
            .Must((cmd, parentId) => parentId != Guid.Empty)
            .When(x => x.Dto.ParentWarehouseId.HasValue)
            .WithMessage("ParentWarehouseId cannot be empty.");
    }
}

public class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.Code).MaximumLength(50);
        RuleFor(x => x.Dto.Address).MaximumLength(500);
        RuleFor(x => x.Dto.Phone).MaximumLength(50);
        RuleFor(x => x.Dto.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Dto.Email));
        RuleFor(x => x.Dto.ParentWarehouseId)
            .Must((cmd, parentId) => parentId != cmd.Id)
            .When(x => x.Dto.ParentWarehouseId.HasValue)
            .WithMessage("Warehouse cannot be its own parent.");
    }
}

public class AddWarehouseZoneCommandValidator : AbstractValidator<AddWarehouseZoneCommand>
{
    public AddWarehouseZoneCommandValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.NameEn).MaximumLength(100);
    }
}

public class CreateWarehouseTypeDefinitionCommandValidator : AbstractValidator<CreateWarehouseTypeDefinitionCommand>
{
    public CreateWarehouseTypeDefinitionCommandValidator()
    {
        RuleFor(x => x.Dto.TenantId).NotEmpty();
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).MaximumLength(200);
        RuleFor(x => x.Dto.Description).MaximumLength(500);
    }
}

public class UpdateWarehouseTypeDefinitionCommandValidator : AbstractValidator<UpdateWarehouseTypeDefinitionCommand>
{
    public UpdateWarehouseTypeDefinitionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).MaximumLength(200);
        RuleFor(x => x.Dto.Description).MaximumLength(500);
    }
}
