using FluentValidation;
using GastroErp.Application.Features.Inventory.Commands;

namespace GastroErp.Application.Features.Inventory.Validators;

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Dto.TenantId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).MaximumLength(200);
        RuleFor(x => x.Dto.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Dto.ApAccountId).NotEmpty();
        RuleFor(x => x.Dto.CreditLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.PaymentDueDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Email));
        RuleFor(x => x.Dto.TaxNumber).MaximumLength(50);
    }
}

public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).MaximumLength(200);
        RuleFor(x => x.Dto.Currency).NotEmpty().Length(3);
    }
}

public class UpsertSupplierMasterCommandValidator : AbstractValidator<UpsertSupplierMasterCommand>
{
    public UpsertSupplierMasterCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Dto.ApAccountId).NotEmpty();
        RuleFor(x => x.Dto.CreditLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.PaymentDueDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.DefaultTaxPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.Dto.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Email));
        RuleFor(x => x.Dto.Rating).InclusiveBetween(0, 5);
    }
}

public class UpdateSupplierFinancialCommandValidator : AbstractValidator<UpdateSupplierFinancialCommand>
{
    public UpdateSupplierFinancialCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.CreditLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.LeadTimeDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.PaymentDueDays).GreaterThanOrEqualTo(0);
    }
}

public class AddSupplierContactCommandValidator : AbstractValidator<AddSupplierContactCommand>
{
    public AddSupplierContactCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.PhoneNumber).MaximumLength(50);
        RuleFor(x => x.Dto)
            .Must(d => !string.IsNullOrWhiteSpace(d.PhoneNumber) || !string.IsNullOrWhiteSpace(d.Mobile))
            .WithMessage("Phone or mobile is required.");
    }
}
