using FluentValidation;
using GastroErp.Application.Features.Crm.Commands;

namespace GastroErp.Application.Features.Crm.Validators;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(v => v.Dto.FullName).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Dto.Mobile).NotEmpty().MaximumLength(20);
        RuleFor(v => v.Dto.Email).EmailAddress().When(v => !string.IsNullOrEmpty(v.Dto.Email));
    }
}

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(v => v.Dto.FullName).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Dto.Mobile).NotEmpty().MaximumLength(20);
        RuleFor(v => v.Dto.Email).EmailAddress().When(v => !string.IsNullOrEmpty(v.Dto.Email));
    }
}

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(v => v.Dto.Code).NotEmpty().MaximumLength(50);
        RuleFor(v => v.Dto.Value).GreaterThan(0);
        RuleFor(v => v.Dto.UsageLimit).GreaterThan(0);
        RuleFor(v => v.Dto.ValidFrom).LessThan(v => v.Dto.ValidTo);
    }
}

public class CreatePromotionCampaignCommandValidator : AbstractValidator<CreatePromotionCampaignCommand>
{
    public CreatePromotionCampaignCommandValidator()
    {
        RuleFor(v => v.Dto.Name).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Dto.Value).GreaterThan(0);
        RuleFor(v => v.Dto.StartDate).LessThan(v => v.Dto.EndDate);
    }
}

public class IssueGiftCardCommandValidator : AbstractValidator<IssueGiftCardCommand>
{
    public IssueGiftCardCommandValidator()
    {
        RuleFor(v => v.Dto.CardNumber).NotEmpty().MaximumLength(50);
        RuleFor(v => v.Dto.InitialValue).GreaterThan(0);
    }
}
