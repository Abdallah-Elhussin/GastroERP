using Asp.Versioning;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.EnterpriseDashboard.DTOs;
using GastroErp.Application.Features.EnterpriseDashboard.Queries;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Dashboard;

/// <summary>
/// Enterprise operations dashboard — available to any authenticated user.
/// Widget-level authorization can be tightened later per module.
/// </summary>
[ApiVersion("1.0")]
[Authorize]
public sealed class EnterpriseDashboardController(ICurrentUser currentUser) : BaseApiController
{
    [HttpGet(ApiRoutes.EnterpriseDashboard.Overview)]
    public async Task<IActionResult> Overview([FromQuery] EnterpriseDashboardFilterDto filter)
        => HandleResult(await Mediator.Send(new GetEnterpriseDashboardOverviewQuery(
            TenantId, currentUser.Name ?? currentUser.Email, filter)));

    [HttpGet(ApiRoutes.EnterpriseDashboard.Sales)]
    public async Task<IActionResult> Sales([FromQuery] EnterpriseDashboardFilterDto filter)
        => HandleResult(await Mediator.Send(new GetEnterpriseDashboardSalesQuery(TenantId, filter)));

    [HttpGet(ApiRoutes.EnterpriseDashboard.Products)]
    public async Task<IActionResult> Products([FromQuery] EnterpriseDashboardFilterDto filter)
        => HandleResult(await Mediator.Send(new GetEnterpriseDashboardProductsQuery(TenantId, filter)));

    [HttpGet(ApiRoutes.EnterpriseDashboard.Customers)]
    public async Task<IActionResult> Customers([FromQuery] EnterpriseDashboardFilterDto filter)
        => HandleResult(await Mediator.Send(new GetEnterpriseDashboardCustomersQuery(TenantId, filter)));

    [HttpGet(ApiRoutes.EnterpriseDashboard.Inventory)]
    public async Task<IActionResult> Inventory([FromQuery] EnterpriseDashboardFilterDto filter)
        => HandleResult(await Mediator.Send(new GetEnterpriseDashboardInventoryQuery(TenantId, filter)));

    [HttpGet(ApiRoutes.EnterpriseDashboard.Finance)]
    public async Task<IActionResult> Finance([FromQuery] EnterpriseDashboardFilterDto filter)
        => HandleResult(await Mediator.Send(new GetEnterpriseDashboardFinanceQuery(TenantId, filter)));

    [HttpGet(ApiRoutes.EnterpriseDashboard.Kitchen)]
    public async Task<IActionResult> Kitchen([FromQuery] EnterpriseDashboardFilterDto filter)
        => HandleResult(await Mediator.Send(new GetEnterpriseDashboardKitchenQuery(TenantId, filter)));

    [HttpGet(ApiRoutes.EnterpriseDashboard.Delivery)]
    public async Task<IActionResult> Delivery([FromQuery] EnterpriseDashboardFilterDto filter)
        => HandleResult(await Mediator.Send(new GetEnterpriseDashboardDeliveryQuery(TenantId, filter)));

    [HttpGet(ApiRoutes.EnterpriseDashboard.Hr)]
    public async Task<IActionResult> Hr([FromQuery] EnterpriseDashboardFilterDto filter)
        => HandleResult(await Mediator.Send(new GetEnterpriseDashboardHrQuery(TenantId, filter)));

    [HttpGet(ApiRoutes.EnterpriseDashboard.Activities)]
    public async Task<IActionResult> Activities([FromQuery] EnterpriseDashboardFilterDto filter)
        => HandleResult(await Mediator.Send(new GetEnterpriseDashboardActivitiesQuery(TenantId, filter)));
}
