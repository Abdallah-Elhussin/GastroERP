using Asp.Versioning;
using GastroErp.Application.Features.Ai.Commands;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Ai;

[ApiVersion("1.0")]
public class RecommendationsController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet($"{ApiRoutes.Ai.Recommendations}/purchase")]
    [HasPermission(Permissions.Ai.RecommendationsView)]
    public async Task<IActionResult> GetPurchase([FromQuery] Guid? branchId)
        => HandleResult(await Mediator.Send(new GetPurchaseRecommendationsQuery(TenantId, branchId)));

    [HttpGet($"{ApiRoutes.Ai.Recommendations}/recipe-cost")]
    [HasPermission(Permissions.Ai.RecommendationsView)]
    public async Task<IActionResult> GetRecipeCost()
        => HandleResult(await Mediator.Send(new GetRecipeCostRecommendationsQuery(TenantId)));

    [HttpGet($"{ApiRoutes.Ai.Recommendations}/staff-scheduling")]
    [HasPermission(Permissions.Ai.RecommendationsView)]
    public async Task<IActionResult> GetStaffScheduling([FromQuery] Guid? branchId)
        => HandleResult(await Mediator.Send(new GetStaffSchedulingRecommendationsQuery(TenantId, branchId)));

    [HttpGet($"{ApiRoutes.Ai.Recommendations}/pricing")]
    [HasPermission(Permissions.Ai.RecommendationsView)]
    public async Task<IActionResult> GetPricing([FromQuery] Guid? branchId)
        => HandleResult(await Mediator.Send(new GetDynamicPricingRecommendationsQuery(TenantId, branchId)));

    [HttpGet(ApiRoutes.Ai.Recommendations)]
    [HasPermission(Permissions.Ai.RecommendationsView)]
    public async Task<IActionResult> GetActions([FromQuery] RecommendationFilterDto filter)
        => HandleResult(await Mediator.Send(new GetRecommendationActionsQuery(TenantId, filter)));

    [HttpPost($"{ApiRoutes.Ai.Recommendations}/refresh")]
    [HasPermission(Permissions.Ai.DataManage)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRecommendationsDto dto)
        => HandleResult(await Mediator.Send(new RefreshRecommendationsCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Ai.Recommendations}/{{id:guid}}/apply")]
    [HasPermission(Permissions.Ai.RecommendationsApply)]
    public async Task<IActionResult> Apply(Guid id, [FromBody] ApplyRecommendationDto? dto)
        => HandleResult(await Mediator.Send(new ApplyRecommendationCommand(TenantId, id, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Ai.Recommendations}/{{id:guid}}/dismiss")]
    [HasPermission(Permissions.Ai.RecommendationsApply)]
    public async Task<IActionResult> Dismiss(Guid id, [FromBody] DismissRecommendationDto? dto)
        => HandleResult(await Mediator.Send(new DismissRecommendationCommand(TenantId, id, CurrentUserId, dto)));
}
