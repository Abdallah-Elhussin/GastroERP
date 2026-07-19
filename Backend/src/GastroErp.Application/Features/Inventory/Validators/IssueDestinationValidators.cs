using FluentValidation;
using GastroErp.Application.Features.Inventory.Commands;

namespace GastroErp.Application.Features.Inventory.Validators;

public sealed class CreateIssueDestinationCommandValidator : AbstractValidator<CreateIssueDestinationCommand>
{
    public CreateIssueDestinationCommandValidator()
    {
        RuleFor(x => x.Dto.TenantId).NotEmpty();
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).MaximumLength(200);
        RuleFor(x => x.Dto.Description).MaximumLength(500);
        RuleFor(x => x.Dto.DestinationType).NotEqual((byte)0);
    }
}

public sealed class UpdateIssueDestinationCommandValidator : AbstractValidator<UpdateIssueDestinationCommand>
{
    public UpdateIssueDestinationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.TenantId).NotEmpty();
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).MaximumLength(200);
        RuleFor(x => x.Dto.Description).MaximumLength(500);
        RuleFor(x => x.Dto.DestinationType).NotEqual((byte)0);
    }
}
