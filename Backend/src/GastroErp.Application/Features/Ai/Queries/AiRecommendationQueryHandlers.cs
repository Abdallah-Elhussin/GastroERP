using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Queries;
using GastroErp.Application.Features.Ai.Services;
using MediatR;

namespace GastroErp.Application.Features.Ai.Queries;

public sealed class GetPurchaseRecommendationsQueryHandler
    : IRequestHandler<GetPurchaseRecommendationsQuery, Result<PurchaseRecommendationsResultDto>>
{
    private readonly IPurchaseRecommendationService _service;
    public GetPurchaseRecommendationsQueryHandler(IPurchaseRecommendationService service) => _service = service;
    public async Task<Result<PurchaseRecommendationsResultDto>> Handle(GetPurchaseRecommendationsQuery request, CancellationToken ct)
        => Result<PurchaseRecommendationsResultDto>.Success(await _service.GetRecommendationsAsync(request.TenantId, request.BranchId, ct));
}

public sealed class GetRecipeCostRecommendationsQueryHandler
    : IRequestHandler<GetRecipeCostRecommendationsQuery, Result<RecipeCostRecommendationsResultDto>>
{
    private readonly IRecipeCostOptimizationService _service;
    public GetRecipeCostRecommendationsQueryHandler(IRecipeCostOptimizationService service) => _service = service;
    public async Task<Result<RecipeCostRecommendationsResultDto>> Handle(GetRecipeCostRecommendationsQuery request, CancellationToken ct)
        => Result<RecipeCostRecommendationsResultDto>.Success(await _service.GetRecommendationsAsync(request.TenantId, ct));
}

public sealed class GetStaffSchedulingRecommendationsQueryHandler
    : IRequestHandler<GetStaffSchedulingRecommendationsQuery, Result<StaffSchedulingRecommendationsResultDto>>
{
    private readonly IStaffSchedulingAdvisorService _service;
    public GetStaffSchedulingRecommendationsQueryHandler(IStaffSchedulingAdvisorService service) => _service = service;
    public async Task<Result<StaffSchedulingRecommendationsResultDto>> Handle(GetStaffSchedulingRecommendationsQuery request, CancellationToken ct)
        => Result<StaffSchedulingRecommendationsResultDto>.Success(await _service.GetRecommendationsAsync(request.TenantId, request.BranchId, ct));
}

public sealed class GetDynamicPricingRecommendationsQueryHandler
    : IRequestHandler<GetDynamicPricingRecommendationsQuery, Result<DynamicPricingRecommendationsResultDto>>
{
    private readonly IDynamicPricingService _service;
    public GetDynamicPricingRecommendationsQueryHandler(IDynamicPricingService service) => _service = service;
    public async Task<Result<DynamicPricingRecommendationsResultDto>> Handle(GetDynamicPricingRecommendationsQuery request, CancellationToken ct)
        => Result<DynamicPricingRecommendationsResultDto>.Success(await _service.GetRecommendationsAsync(request.TenantId, request.BranchId, ct));
}

public sealed class GetRecommendationActionsQueryHandler
    : IRequestHandler<GetRecommendationActionsQuery, Result<IReadOnlyList<RecommendationActionDto>>>
{
    private readonly IRecommendationActionService _service;
    public GetRecommendationActionsQueryHandler(IRecommendationActionService service) => _service = service;
    public async Task<Result<IReadOnlyList<RecommendationActionDto>>> Handle(GetRecommendationActionsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<RecommendationActionDto>>.Success(await _service.GetActionsAsync(request.TenantId, request.Filter, ct));
}
