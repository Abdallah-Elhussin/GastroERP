using FluentValidation;
using GastroErp.Application.Features.Inventory.Commands;

namespace GastroErp.Application.Features.Inventory.Validators;

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Dto.TenantId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.Currency).NotEmpty().Length(3);
    }
}

public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.Currency).NotEmpty().Length(3);
    }
}

public class UpdateSupplierFinancialCommandValidator : AbstractValidator<UpdateSupplierFinancialCommand>
{
    public UpdateSupplierFinancialCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.CreditLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.LeadTimeDays).GreaterThanOrEqualTo(0);
    }
}

public class AddSupplierContactCommandValidator : AbstractValidator<AddSupplierContactCommand>
{
    public AddSupplierContactCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.PhoneNumber).NotEmpty().MaximumLength(20);
    }
}
