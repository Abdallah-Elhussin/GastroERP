using FluentValidation;
using GastroErp.Application.Features.Hr.Commands;
using GastroErp.Domain.Entities.HR;

namespace GastroErp.Application.Features.Hr.Validators;

public sealed class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().MaximumLength(300)
            .Must(EmployeeNameRules.IsTripleName)
            .WithMessage("Employee name must contain at least three parts.");
        RuleFor(x => x.Dto.NameAr)
            .MaximumLength(300)
            .Must(name => name is null || EmployeeNameRules.IsTripleName(name))
            .WithMessage("Arabic name must contain at least three parts when provided.");
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
    }
}

public sealed class UpdateSelfProfileValidator : AbstractValidator<UpdateSelfProfileCommand>
{
    public UpdateSelfProfileValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().MaximumLength(300)
            .Must(EmployeeNameRules.IsTripleName)
            .WithMessage("Employee name must contain at least three parts.");
        RuleFor(x => x.Dto.NameAr)
            .MaximumLength(300)
            .Must(name => name is null || EmployeeNameRules.IsTripleName(name))
            .WithMessage("Arabic name must contain at least three parts when provided.");
    }
}

public sealed class CheckInValidator : AbstractValidator<CheckInCommand>
{
    public CheckInValidator()
    {
        RuleFor(x => x.Dto.EmployeeId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
    }
}

public sealed class SubmitLeaveValidator : AbstractValidator<SubmitLeaveCommand>
{
    public SubmitLeaveValidator()
    {
        RuleFor(x => x.Dto.EmployeeId).NotEmpty();
        RuleFor(x => x.Dto.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Dto.ToDate).GreaterThanOrEqualTo(x => x.Dto.FromDate);
    }
}

public sealed class CreatePayrollRunValidator : AbstractValidator<CreatePayrollRunCommand>
{
    public CreatePayrollRunValidator()
    {
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.Year).InclusiveBetween(2020, 2100);
        RuleFor(x => x.Dto.Month).InclusiveBetween(1, 12);
    }
}

public sealed class UpsertSalaryStructureValidator : AbstractValidator<UpsertSalaryStructureCommand>
{
    public UpsertSalaryStructureValidator()
    {
        RuleFor(x => x.Dto.EmployeeId).NotEmpty();
        RuleFor(x => x.Dto.BaseSalary).GreaterThanOrEqualTo(0);
    }
}

public sealed class AddEmergencyContactValidator : AbstractValidator<AddEmergencyContactCommand>
{
    public AddEmergencyContactValidator()
    {
        RuleFor(x => x.Dto.EmployeeId).NotEmpty();
        RuleFor(x => x.Dto.Name)
            .NotEmpty().MaximumLength(300)
            .Must(EmployeeNameRules.IsTripleName)
            .WithMessage("Contact name must contain at least three parts.");
        RuleFor(x => x.Dto.Phone).NotEmpty().MaximumLength(30);
    }
}

public sealed class ApplyApplicantValidator : AbstractValidator<ApplyApplicantCommand>
{
    public ApplyApplicantValidator()
    {
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.Name)
            .NotEmpty().MaximumLength(300)
            .Must(EmployeeNameRules.IsTripleName)
            .WithMessage("Applicant name must contain at least three parts.");
        RuleFor(x => x.Dto.NameAr)
            .MaximumLength(300)
            .Must(name => name is null || EmployeeNameRules.IsTripleName(name))
            .WithMessage("Arabic name must contain at least three parts when provided.");
        RuleFor(x => x.Dto.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}
