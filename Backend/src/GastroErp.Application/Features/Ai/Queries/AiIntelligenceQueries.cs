using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Ai.Queries;

public record GetFraudAlertsQuery(Guid TenantId, IntelligenceFilterDto Filter)
    : IRequest<Result<IReadOnlyList<FraudAlertDto>>>;

public record GetCustomerSegmentsQuery(Guid TenantId, IntelligenceFilterDto Filter)
    : IRequest<Result<IReadOnlyList<CustomerSegmentDto>>>;

public record GetChurnPredictionsQuery(Guid TenantId, IntelligenceFilterDto Filter)
    : IRequest<Result<IReadOnlyList<ChurnPredictionDto>>>;

public record GetProductRecommendationsQuery(Guid TenantId, IntelligenceFilterDto Filter)
    : IRequest<Result<IReadOnlyList<ProductRecommendationDto>>>;

public record GetIntelligenceDashboardQuery(Guid TenantId, Guid? BranchId = null)
    : IRequest<Result<IntelligenceDashboardDto>>;

public record GetIntelligenceMonitoringQuery()
    : IRequest<Result<IntelligenceMonitoringDto>>;
