using GastroErp.Application.Features.Onboarding.DTOs;
using FluentValidation;

namespace GastroErp.Application.Features.Onboarding.Validators;

public sealed class RegisterCompanyValidator : AbstractValidator<RegisterCompanyDto>
{
    public RegisterCompanyValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(128);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().Length(2, 3);
        RuleFor(x => x.Subscription).NotEmpty();
    }
}
