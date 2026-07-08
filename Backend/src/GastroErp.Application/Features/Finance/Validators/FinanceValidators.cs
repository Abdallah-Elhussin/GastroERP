using FluentValidation;
using GastroErp.Application.Features.Finance.Commands;

namespace GastroErp.Application.Features.Finance.Validators;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Dto.AccountNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class CreateJournalCommandValidator : AbstractValidator<CreateJournalCommand>
{
    public CreateJournalCommandValidator()
    {
        RuleFor(x => x.Dto.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Dto.Lines).NotEmpty();
    }
}

public class CreateFiscalPeriodCommandValidator : AbstractValidator<CreateFiscalPeriodCommand>
{
    public CreateFiscalPeriodCommandValidator()
    {
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.EndDate).GreaterThan(x => x.Dto.StartDate);
    }
}

public class CreateCostCenterCommandValidator : AbstractValidator<CreateCostCenterCommand>
{
    public CreateCostCenterCommandValidator()
    {
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.BranchId).NotEmpty();
    }
}
