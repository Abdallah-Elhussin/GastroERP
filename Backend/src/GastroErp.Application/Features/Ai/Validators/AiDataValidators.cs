using FluentValidation;
using GastroErp.Application.Features.Ai.Commands;

namespace GastroErp.Application.Features.Ai.Validators;

public sealed class TriggerWarehouseSyncValidator : AbstractValidator<TriggerWarehouseSyncCommand>
{
    public TriggerWarehouseSyncValidator()
    {
        RuleFor(x => x.Dto.LookbackDays).InclusiveBetween(1, 365);
    }
}

public sealed class CreateDatasetDefinitionValidator : AbstractValidator<CreateDatasetDefinitionCommand>
{
    public CreateDatasetDefinitionValidator()
    {
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.SpecJson).NotEmpty();
    }
}

public sealed class BuildDatasetValidator : AbstractValidator<BuildDatasetCommand>
{
    public BuildDatasetValidator()
    {
        RuleFor(x => x.Dto.DefinitionId).NotEmpty();
        RuleFor(x => x.Dto.TrainRatio).InclusiveBetween(0.1, 0.9);
        RuleFor(x => x.Dto.ValidationRatio).InclusiveBetween(0.05, 0.5);
    }
}
