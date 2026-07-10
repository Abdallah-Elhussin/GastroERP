using FluentValidation;
using GastroErp.Application.Features.Onboarding.DTOs;

namespace GastroErp.Application.Features.Onboarding.Validators;

public sealed class RegisterCompanyValidator : AbstractValidator<RegisterCompanyDto>
{
    private static readonly string[] SupportedCurrencies =
        ["SAR", "USD", "AED", "KWD", "QAR", "OMR", "BHD", "EGP", "SDG"];

    private static readonly string[] SupportedLanguages = ["ar", "en"];

    private static readonly string[] SupportedCalendarTypes = ["Gregorian", "Hijri"];

    public RegisterCompanyValidator()
    {
        RuleFor(x => x.Admin).NotNull().SetValidator(new AdminAccountStepValidator());
        RuleFor(x => x.Company).NotNull().SetValidator(new CompanyDataStepValidator());
        RuleFor(x => x.Settings).NotNull().SetValidator(new GeneralSettingsStepValidator());
        RuleFor(x => x.Branch).NotNull().SetValidator(new MainBranchStepValidator());
    }

    private sealed class AdminAccountStepValidator : AbstractValidator<AdminAccountStepDto>
    {
        public AdminAccountStepValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Mobile).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(128);
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage("Password and confirmation must match.");
        }
    }

    private sealed class CompanyDataStepValidator : AbstractValidator<CompanyDataStepDto>
    {
        public CompanyDataStepValidator()
        {
            RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
            RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
            RuleFor(x => x.TradeName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.CommercialRegister).NotEmpty().MaximumLength(50);
            RuleFor(x => x.TaxNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Website).MaximumLength(200);
            RuleFor(x => x.LogoUrl).MaximumLength(500);
            RuleFor(x => x.TaxCertificateUrl).MaximumLength(500);
            RuleFor(x => x.Address).NotNull().SetValidator(new CompanyAddressValidator());
        }
    }

    private sealed class CompanyAddressValidator : AbstractValidator<CompanyAddressDto>
    {
        public CompanyAddressValidator()
        {
            RuleFor(x => x.Country).NotEmpty().Length(2, 3);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Region).MaximumLength(100);
            RuleFor(x => x.District).MaximumLength(100);
            RuleFor(x => x.Street).MaximumLength(200);
            RuleFor(x => x.PostalCode).MaximumLength(20);
        }
    }

    private sealed class GeneralSettingsStepValidator : AbstractValidator<GeneralSettingsStepDto>
    {
        public GeneralSettingsStepValidator()
        {
            RuleFor(x => x.Language)
                .NotEmpty()
                .Must(l => SupportedLanguages.Contains(l, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Language must be 'ar' or 'en'.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .Must(c => SupportedCurrencies.Contains(c.ToUpperInvariant()))
                .WithMessage("Unsupported currency code.");

            RuleFor(x => x.Timezone).NotEmpty().MaximumLength(100);

            RuleFor(x => x.FiscalYearStartMonth).InclusiveBetween(1, 12);

            RuleFor(x => x.CalendarType)
                .NotEmpty()
                .Must(c => SupportedCalendarTypes.Contains(c, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Calendar type must be Gregorian or Hijri.");

            RuleForEach(x => x.AdditionalCurrencies)
                .Must(c => SupportedCurrencies.Contains(c.ToUpperInvariant()))
                .When(x => x.MultiCurrencyEnabled && x.AdditionalCurrencies is not null);
        }
    }

    private sealed class MainBranchStepValidator : AbstractValidator<MainBranchStepDto>
    {
        public MainBranchStepValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        }
    }
}
