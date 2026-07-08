using Asp.Versioning;
using GastroErp.Application.Features.Sales.Commands;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales;

[ApiVersion("1.0")]
public class FloorPlanController : BaseApiController
{
    [HttpGet(ApiRoutes.Sales.FloorPlans)]
    [HasPermission(Permissions.FloorPlan.View)]
    public async Task<IActionResult> GetFloorPlans([FromQuery] Guid? branchId)
        => HandleResult(await Mediator.Send(new GetFloorPlansQuery(TenantId, branchId)));

    [HttpPost(ApiRoutes.Sales.FloorPlans)]
    [HasPermission(Permissions.FloorPlan.Manage)]
    public async Task<IActionResult> CreateFloorPlan([FromBody] CreateFloorPlanDto dto)
        => HandleResult(await Mediator.Send(new CreateFloorPlanCommand(TenantId, dto)));

    [HttpGet($"{ApiRoutes.Sales.FloorPlans}/{{id:guid}}")]
    [HasPermission(Permissions.FloorPlan.View)]
    public async Task<IActionResult> GetFloorPlanById(Guid id)
        => HandleResult(await Mediator.Send(new GetFloorPlanByIdQuery(id)));

    [HttpGet($"{ApiRoutes.Sales.FloorPlans}/{{id:guid}}/tables")]
    [HasPermission(Permissions.FloorPlan.View)]
    public async Task<IActionResult> GetTablesByFloorPlan(Guid id)
        => HandleResult(await Mediator.Send(new GetTablesByFloorPlanQuery(id)));

    [HttpPost($"{ApiRoutes.Sales.FloorPlans}/{{floorPlanId:guid}}/areas")]
    [HasPermission(Permissions.FloorPlan.Manage)]
    public async Task<IActionResult> AddDiningArea(Guid floorPlanId, [FromBody] AddDiningAreaDto dto)
        => HandleResult(await Mediator.Send(new AddDiningAreaCommand(floorPlanId, dto)));

    [HttpPost($"{ApiRoutes.Sales.FloorPlans}/areas/{{diningAreaId:guid}}/tables")]
    [HasPermission(Permissions.FloorPlan.Manage)]
    public async Task<IActionResult> AddTable(Guid diningAreaId, [FromBody] AddRestaurantTableDto dto)
        => HandleResult(await Mediator.Send(new AddRestaurantTableCommand(diningAreaId, dto)));
}

[ApiVersion("1.0")]
public class TableController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Sales.Tables}/{{id:guid}}")]
    [HasPermission(Permissions.Table.View)]
    public async Task<IActionResult> GetTableById(Guid id)
        => HandleResult(await Mediator.Send(new GetTableByIdQuery(id)));

    [HttpPost($"{ApiRoutes.Sales.Tables}/{{id:guid}}/occupy")]
    [HasPermission(Permissions.Table.Manage)]
    public async Task<IActionResult> OccupyTable(Guid id, [FromBody] OccupyTableDto dto)
        => HandleResult(await Mediator.Send(new OccupyTableCommand(id, dto.OrderId)));

    [HttpPost($"{ApiRoutes.Sales.Tables}/{{id:guid}}/release")]
    [HasPermission(Permissions.Table.Manage)]
    public async Task<IActionResult> ReleaseTable(Guid id)
        => HandleResult(await Mediator.Send(new ReleaseTableCommand(id)));

    [HttpPatch($"{ApiRoutes.Sales.Tables}/{{id:guid}}/status")]
    [HasPermission(Permissions.Table.Manage)]
    public async Task<IActionResult> UpdateTableStatus(Guid id, [FromBody] UpdateTableStatusDto dto)
        => HandleResult(await Mediator.Send(new UpdateTableStatusCommand(id, dto)));
}
