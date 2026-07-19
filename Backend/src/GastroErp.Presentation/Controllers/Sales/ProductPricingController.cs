using Asp.Versioning;
using GastroErp.Application.Features.Sales.ProductPricing.Commands;
using GastroErp.Application.Features.Sales.ProductPricing.Dtos;
using GastroErp.Application.Features.Sales.ProductPricing.Queries;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales;

/// <summary>Product pricing — sales prices separated from product master data.</summary>
[ApiVersion("1.0")]
public class ProductPricingController : BaseApiController
{
    [HttpGet(ApiRoutes.Sales.ProductPrices)]
    [HasPermission(Permissions.Sales.ProductPricing.View)]
    public async Task<IActionResult> Get(
        [FromQuery] string? search = null,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? priceListId = null,
        [FromQuery] Guid? unitId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] DateTimeOffset? asOfDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => HandlePagedResult(await Mediator.Send(new GetProductPricesQuery(
            TenantId, search, productId, branchId, priceListId, unitId, isActive, asOfDate,
            sortBy, sortDesc, page, pageSize)));

    [HttpGet($"{ApiRoutes.Sales.ProductPrices}/{{id:guid}}")]
    [HasPermission(Permissions.Sales.ProductPricing.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetProductPriceByIdQuery(id, TenantId)));

    [HttpGet($"{ApiRoutes.Sales.ProductPrices}/product-units/{{productId:guid}}")]
    [HasPermission(Permissions.Sales.ProductPricing.View)]
    public async Task<IActionResult> GetProductUnits(
        Guid productId,
        [FromQuery] ProductCostType costType = ProductCostType.Average)
        => HandleResult(await Mediator.Send(new GetProductUnitsForPricingQuery(TenantId, productId, costType)));

    [HttpPost(ApiRoutes.Sales.ProductPrices)]
    [HasPermission(Permissions.Sales.ProductPricing.Create)]
    public async Task<IActionResult> Create([FromBody] CreateProductPriceRequest request)
        => HandleResult(await Mediator.Send(new CreateProductPriceCommand(
            request with { TenantId = TenantId })));

    [HttpPost($"{ApiRoutes.Sales.ProductPrices}/batch")]
    [HasPermission(Permissions.Sales.ProductPricing.Create)]
    public async Task<IActionResult> CreateBatch([FromBody] CreateProductPricesBatchRequest request)
        => HandleResult(await Mediator.Send(new CreateProductPricesBatchCommand(
            request with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Sales.ProductPrices}/{{id:guid}}")]
    [HasPermission(Permissions.Sales.ProductPricing.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductPriceRequest request)
        => HandleResult(await Mediator.Send(new UpdateProductPriceCommand(
            id, request with { TenantId = TenantId })));

    [HttpDelete($"{ApiRoutes.Sales.ProductPrices}/{{id:guid}}")]
    [HasPermission(Permissions.Sales.ProductPricing.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteProductPriceCommand(id, TenantId)));

    [HttpPost($"{ApiRoutes.Sales.ProductPrices}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Sales.ProductPricing.Edit)]
    public async Task<IActionResult> Activate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateProductPriceCommand(id, TenantId, true)));

    [HttpPost($"{ApiRoutes.Sales.ProductPrices}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Sales.ProductPricing.Edit)]
    public async Task<IActionResult> Deactivate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateProductPriceCommand(id, TenantId, false)));

    [HttpPost($"{ApiRoutes.Sales.ProductPrices}/copy-list")]
    [HasPermission(Permissions.Sales.ProductPricing.Copy)]
    public async Task<IActionResult> CopyList([FromBody] CopyPriceListRequest request)
        => HandleResult(await Mediator.Send(new CopyPriceListCommand(
            request with { TenantId = TenantId })));
}

/// <summary>Sales price lists (Retail, Delivery, VIP, …).</summary>
[ApiVersion("1.0")]
public class SalesPriceListsController : BaseApiController
{
    [HttpGet(ApiRoutes.Sales.PriceLists)]
    [HasPermission(Permissions.Sales.ProductPricing.View)]
    public async Task<IActionResult> Get(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool activeOnly = false)
        => HandleResult(await Mediator.Send(new GetPriceListsQuery(TenantId, search, isActive, activeOnly)));

    [HttpPost(ApiRoutes.Sales.PriceLists)]
    [HasPermission(Permissions.Sales.ProductPricing.Create)]
    public async Task<IActionResult> Create([FromBody] CreateSalesPriceListRequest request)
        => HandleResult(await Mediator.Send(new CreateSalesPriceListCommand(
            request with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Sales.PriceLists}/{{id:guid}}")]
    [HasPermission(Permissions.Sales.ProductPricing.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalesPriceListRequest request)
        => HandleResult(await Mediator.Send(new UpdateSalesPriceListCommand(
            id, request with { TenantId = TenantId })));

    [HttpDelete($"{ApiRoutes.Sales.PriceLists}/{{id:guid}}")]
    [HasPermission(Permissions.Sales.ProductPricing.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteSalesPriceListCommand(id, TenantId)));
}
