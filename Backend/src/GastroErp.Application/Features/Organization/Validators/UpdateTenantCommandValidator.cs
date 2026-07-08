using FluentValidation;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Organization.Commands;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Application.Features.Organization.Validators;

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(localizationService.GetMessage(ErrorCodes.RequiredField, "Id"));

        RuleFor(x => x.Dto.DefaultCurrency)
            .NotEmpty().WithMessage(localizationService.GetMessage(ErrorCodes.RequiredField, "DefaultCurrency"));

        RuleFor(x => x.Dto.DefaultLanguage)
            .NotEmpty().WithMessage(localizationService.GetMessage(ErrorCodes.RequiredField, "DefaultLanguage"));

        RuleFor(x => x.Dto.DefaultTimezone)
            .NotEmpty().WithMessage(localizationService.GetMessage(ErrorCodes.RequiredField, "DefaultTimezone"));
    }
}
