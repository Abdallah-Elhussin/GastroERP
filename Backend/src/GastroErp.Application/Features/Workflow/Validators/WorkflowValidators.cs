using FluentValidation;
using GastroErp.Application.Features.Workflow.Commands;

namespace GastroErp.Application.Features.Workflow.Validators;

public sealed class CreateWorkflowDefinitionValidator : AbstractValidator<CreateWorkflowDefinitionCommand>
{
    public CreateWorkflowDefinitionValidator()
    {
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(50);
    }
}

public sealed class StartWorkflowValidator : AbstractValidator<StartWorkflowCommand>
{
    public StartWorkflowValidator()
    {
        RuleFor(x => x.Dto.WorkflowCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Dto.ReferenceType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.ReferenceId).NotEmpty();
    }
}

public sealed class RejectWorkflowValidator : AbstractValidator<RejectWorkflowCommand>
{
    public RejectWorkflowValidator()
    {
        RuleFor(x => x.Dto.InstanceId).NotEmpty();
        RuleFor(x => x.Dto.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class CreateApprovalDelegateValidator : AbstractValidator<CreateApprovalDelegateCommand>
{
    public CreateApprovalDelegateValidator()
    {
        RuleFor(x => x.Dto.DelegateUserId).NotEmpty();
        RuleFor(x => x.Dto.ToDate).GreaterThanOrEqualTo(x => x.Dto.FromDate);
    }
}
