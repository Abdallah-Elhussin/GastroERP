using Asp.Versioning;
using GastroErp.Application.Features.Inventory.ProductInquiry.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>Product Information Center — read-only aggregated item inquiry.</summary>
[ApiVersion("1.0")]
public class ProductInquiryController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.ProductInquiry)]
    [HasPermission(Permissions.Inventory.ProductInquiry.View)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? search = null,
        [FromQuery] bool activeOnly = true,
        [FromQuery] bool inventoryOnly = false,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? itemTypeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => HandlePagedResult(await Mediator.Send(new GetProductInquiryListQuery(
            TenantId, search, activeOnly, inventoryOnly, categoryId, itemTypeId, sortBy, sortDesc, page, pageSize)));

    [HttpGet($"{ApiRoutes.Inventory.ProductInquiry}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.ProductInquiry.View)]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var permissions = User.Claims
            .Where(c => c.Type is "Permission" or "permissions")
            .Select(c => c.Value)
            .ToHashSet(StringComparer.Ordinal);

        bool Can(string p) =>
            permissions.Contains(p)
            || permissions.Contains("ALL")
            || permissions.Contains("Inventory.Manage")
            || User.IsInRole("Administrator")
            || (p.EndsWith(".ViewCost", StringComparison.Ordinal) && permissions.Contains(Permissions.Inventory.ProductInquiry.View))
            || (p.EndsWith(".ViewPrices", StringComparison.Ordinal) && permissions.Contains(Permissions.Inventory.ProductInquiry.View))
            || (p.EndsWith(".ViewMovements", StringComparison.Ordinal) && permissions.Contains(Permissions.Inventory.ProductInquiry.View))
            || (p.EndsWith(".ViewSuppliers", StringComparison.Ordinal) && permissions.Contains(Permissions.Inventory.ProductInquiry.View))
            || permissions.Contains("Inventory.View");

        return HandleResult(await Mediator.Send(new GetProductInquiryDetailQuery(
            TenantId,
            id,
            CanViewCost: Can(Permissions.Inventory.ProductInquiry.ViewCost),
            CanViewPrices: Can(Permissions.Inventory.ProductInquiry.ViewPrices),
            CanViewMovements: Can(Permissions.Inventory.ProductInquiry.ViewMovements),
            CanViewSuppliers: Can(Permissions.Inventory.ProductInquiry.ViewSuppliers))));
    }
}
