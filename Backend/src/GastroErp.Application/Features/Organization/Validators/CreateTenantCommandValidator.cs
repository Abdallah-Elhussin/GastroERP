using FluentValidation;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Organization.Commands;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Application.Features.Organization.Validators;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Dto.NameAr)
            .NotEmpty().WithMessage(localizationService.GetMessage(ErrorCodes.RequiredField, "NameAr"))
            .MaximumLength(100).WithMessage(localizationService.GetMessage(ErrorCodes.MaxLengthExceeded, 100));

        RuleFor(x => x.Dto.Slug)
            .NotEmpty().WithMessage(localizationService.GetMessage(ErrorCodes.RequiredField, "Slug"))
            .Matches("^[a-z0-9-]+$").WithMessage(localizationService.GetMessage("InvalidSlugFormat"))
            .MaximumLength(50).WithMessage(localizationService.GetMessage(ErrorCodes.MaxLengthExceeded, 50));

        RuleFor(x => x.Dto.DefaultCurrency)
            .NotEmpty().WithMessage(localizationService.GetMessage(ErrorCodes.RequiredField, "DefaultCurrency"));

        RuleFor(x => x.Dto.DefaultLanguage)
            .NotEmpty().WithMessage(localizationService.GetMessage(ErrorCodes.RequiredField, "DefaultLanguage"));

        RuleFor(x => x.Dto.DefaultTimezone)
            .NotEmpty().WithMessage(localizationService.GetMessage(ErrorCodes.RequiredField, "DefaultTimezone"));
    }
}
