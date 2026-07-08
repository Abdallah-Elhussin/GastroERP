using FluentValidation;
using GastroErp.Application.Features.ReportingPlatform.Commands;
using GastroErp.Application.Features.ReportingPlatform.DTOs;

namespace GastroErp.Application.Features.ReportingPlatform.Validators;

public sealed class CreateDashboardValidator : AbstractValidator<CreateDashboardCommand>
{
    public CreateDashboardValidator() => RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(200);
}

public sealed class UpdateDashboardValidator : AbstractValidator<UpdateDashboardCommand>
{
    public UpdateDashboardValidator() => RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(200);
}

public sealed class CreateReportDefinitionValidator : AbstractValidator<CreateReportDefinitionCommand>
{
    public CreateReportDefinitionValidator()
    {
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Dto.DataSource).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateReportDefinitionValidator : AbstractValidator<UpdateReportDefinitionCommand>
{
    public UpdateReportDefinitionValidator()
    {
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.DataSource).NotEmpty().MaximumLength(200);
    }
}

public sealed class ExecuteReportValidator : AbstractValidator<ExecuteReportCommand>
{
    public ExecuteReportValidator()
    {
        RuleFor(x => x.Dto).Must(d => d.ReportDefinitionId.HasValue || !string.IsNullOrWhiteSpace(d.DataSource))
            .WithMessage("ReportDefinitionId or DataSource is required.");
    }
}

public sealed class CreateScheduledReportValidator : AbstractValidator<CreateScheduledReportCommand>
{
    public CreateScheduledReportValidator() => RuleFor(x => x.Dto.ReportDefinitionId).NotEmpty();
}

public sealed class CreateKpiDefinitionValidator : AbstractValidator<CreateKpiDefinitionCommand>
{
    public CreateKpiDefinitionValidator()
    {
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Dto.Formula).NotEmpty().MaximumLength(500);
    }
}

public sealed class ExportReportPlatformValidator : AbstractValidator<ExportReportPlatformCommand>
{
    public ExportReportPlatformValidator()
    {
        RuleFor(x => x.Dto).Must(d => d.ReportDefinitionId.HasValue || !string.IsNullOrWhiteSpace(d.ReportKey))
            .WithMessage("ReportDefinitionId or ReportKey is required.");
    }
}
