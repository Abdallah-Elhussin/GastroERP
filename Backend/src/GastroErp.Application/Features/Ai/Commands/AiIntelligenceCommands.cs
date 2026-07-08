using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Ai.Commands;

public record RefreshFraudAnalysisCommand(Guid TenantId, RefreshIntelligenceDto Dto)
    : IRequest<Result<FraudAnalysisResultDto>>;

public record RefreshSegmentsCommand(Guid TenantId, RefreshIntelligenceDto Dto)
    : IRequest<Result<SegmentationResultDto>>;

public record RefreshChurnCommand(Guid TenantId, RefreshIntelligenceDto Dto)
    : IRequest<Result<ChurnAnalysisResultDto>>;

public record RefreshIntelligenceRecommendationsCommand(Guid TenantId, RefreshIntelligenceDto Dto)
    : IRequest<Result<ProductRecommendationResultDto>>;
