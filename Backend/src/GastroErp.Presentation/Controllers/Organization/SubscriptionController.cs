using Asp.Versioning;
using GastroErp.Application.Features.Organization.Commands;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Application.Features.Organization.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GastroErp.Presentation.Controllers.Organization;

/// <summary>
/// Subscription management
/// </summary>
[ApiVersion("1.0")]
public class SubscriptionController : BaseApiController
{
    /// <summary>
    /// Get tenant subscription details
    /// </summary>
    [HttpGet(ApiRoutes.Organization.Subscriptions)]
    [HasPermission(Permissions.Tenant.View)] // Assuming viewing subscription is part of tenant management
    public async Task<IActionResult> GetSubscription()
    {
        return HandleResult(await Mediator.Send(new GetSubscriptionQuery(TenantId)));
    }

    /// <summary>
    /// Create a new subscription
    /// </summary>
    [HttpPost(ApiRoutes.Organization.Subscriptions)]
    [HasPermission(Permissions.Tenant.Manage)]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionDto dto)
    {
        // Enforce the tenant ID to be the current tenant
        var commandDto = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateSubscriptionCommand(commandDto)));
    }

    /// <summary>
    /// Renew subscription
    /// </summary>
    [HttpPost($"{ApiRoutes.Organization.Subscriptions}/{{id:guid}}/renew")]
    [HasPermission(Permissions.Tenant.Manage)]
    public async Task<IActionResult> RenewSubscription(Guid id, [FromBody] RenewSubscriptionDto dto)
    {
        return HandleResult(await Mediator.Send(new RenewSubscriptionCommand(id, dto.NewEndDate, dto.Price)));
    }

    /// <summary>
    /// Suspend subscription
    /// </summary>
    [HttpPost($"{ApiRoutes.Organization.Subscriptions}/{{id:guid}}/suspend")]
    [HasPermission(Permissions.Tenant.Manage)] // This could be an admin-only permission in a real app
    public async Task<IActionResult> SuspendSubscription(Guid id)
    {
        return HandleResult(await Mediator.Send(new SuspendSubscriptionCommand(id)));
    }

    /// <summary>
    /// Resume subscription
    /// </summary>
    [HttpPost($"{ApiRoutes.Organization.Subscriptions}/{{id:guid}}/resume")]
    [HasPermission(Permissions.Tenant.Manage)]
    public async Task<IActionResult> ResumeSubscription(Guid id)
    {
        return HandleResult(await Mediator.Send(new ResumeSubscriptionCommand(id)));
    }

    /// <summary>
    /// Cancel subscription
    /// </summary>
    [HttpPost($"{ApiRoutes.Organization.Subscriptions}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Tenant.Manage)]
    public async Task<IActionResult> CancelSubscription(Guid id)
    {
        return HandleResult(await Mediator.Send(new CancelSubscriptionCommand(id)));
    }

    /// <summary>
    /// Get available subscription plans
    /// </summary>
    [HttpGet($"{ApiRoutes.Organization.Subscriptions}/plans")]
    public async Task<IActionResult> GetPlans()
    {
        return HandleResult(await Mediator.Send(new GetPlansQuery()));
    }
}

public record RenewSubscriptionDto(DateTimeOffset NewEndDate, decimal Price);
