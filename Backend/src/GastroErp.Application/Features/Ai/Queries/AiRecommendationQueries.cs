using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Ai.Queries;

public record GetPurchaseRecommendationsQuery(Guid TenantId, Guid? BranchId = null)
    : IRequest<Result<PurchaseRecommendationsResultDto>>;

public record GetRecipeCostRecommendationsQuery(Guid TenantId)
    : IRequest<Result<RecipeCostRecommendationsResultDto>>;

public record GetStaffSchedulingRecommendationsQuery(Guid TenantId, Guid? BranchId = null)
    : IRequest<Result<StaffSchedulingRecommendationsResultDto>>;

public record GetDynamicPricingRecommendationsQuery(Guid TenantId, Guid? BranchId = null)
    : IRequest<Result<DynamicPricingRecommendationsResultDto>>;

public record GetRecommendationActionsQuery(Guid TenantId, RecommendationFilterDto Filter)
    : IRequest<Result<IReadOnlyList<RecommendationActionDto>>>;
