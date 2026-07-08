using FluentValidation;
using GastroErp.Application.Features.Invoicing.Commands;

namespace GastroErp.Application.Features.Invoicing.Validators;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
    }
}

public class FinalizeInvoiceCommandValidator : AbstractValidator<FinalizeInvoiceCommand>
{
    public FinalizeInvoiceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class CancelInvoiceCommandValidator : AbstractValidator<CancelInvoiceCommand>
{
    public CancelInvoiceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.Reason).NotEmpty().MinimumLength(3);
    }
}

public class CreateTaxRateCommandValidator : AbstractValidator<CreateTaxRateCommand>
{
    public CreateTaxRateCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Rate).GreaterThanOrEqualTo(0);
    }
}

public class UpdateTaxGroupCommandValidator : AbstractValidator<UpdateTaxGroupCommand>
{
    public UpdateTaxGroupCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class CreateCreditNoteCommandValidator : AbstractValidator<CreateCreditNoteCommand>
{
    public CreateCreditNoteCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.OriginalInvoiceId).NotEmpty();
        RuleFor(x => x.Dto.Reason).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Dto.Lines).NotEmpty();
    }
}

public class CreateDebitNoteCommandValidator : AbstractValidator<CreateDebitNoteCommand>
{
    public CreateDebitNoteCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.OriginalInvoiceId).NotEmpty();
        RuleFor(x => x.Dto.Reason).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Dto.Lines).NotEmpty();
    }
}

public class IssueCreditNoteCommandValidator : AbstractValidator<IssueCreditNoteCommand>
{
    public IssueCreditNoteCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class IssueDebitNoteCommandValidator : AbstractValidator<IssueDebitNoteCommand>
{
    public IssueDebitNoteCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}
