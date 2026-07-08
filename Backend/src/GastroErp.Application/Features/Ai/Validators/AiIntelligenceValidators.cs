using FluentValidation;
using GastroErp.Application.Features.Ai.Commands;
using GastroErp.Application.Features.Ai.DTOs;

namespace GastroErp.Application.Features.Ai.Validators;

public sealed class FraudAnalysisValidator : AbstractValidator<RefreshFraudAnalysisCommand>
{
    public FraudAnalysisValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.LookbackDays).InclusiveBetween(1, 365);
    }
}

public sealed class SegmentValidator : AbstractValidator<RefreshSegmentsCommand>
{
    public SegmentValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.LookbackDays).InclusiveBetween(7, 365);
    }
}

public sealed class ChurnValidator : AbstractValidator<RefreshChurnCommand>
{
    public ChurnValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.LookbackDays).InclusiveBetween(7, 365);
    }
}

public sealed class IntelligenceRecommendationValidator : AbstractValidator<RefreshIntelligenceRecommendationsCommand>
{
    public IntelligenceRecommendationValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.LookbackDays).InclusiveBetween(7, 180);
    }
}

public sealed class IntelligenceFilterValidator : AbstractValidator<IntelligenceFilterDto>
{
    public IntelligenceFilterValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 500);
    }
}
