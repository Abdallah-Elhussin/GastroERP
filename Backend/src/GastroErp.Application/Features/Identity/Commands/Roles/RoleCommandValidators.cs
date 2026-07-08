using FluentValidation;

namespace GastroErp.Application.Features.Identity.Commands.Roles;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.NameAr).MaximumLength(100);
        RuleFor(x => x.Dto.Description).MaximumLength(500);
    }
}

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.NameAr).MaximumLength(100);
        RuleFor(x => x.Dto.Description).MaximumLength(500);
    }
}
