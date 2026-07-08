using GastroErp.Application.Common.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Common;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    private GastroErp.Presentation.Resolution.ITenantResolver? _tenantResolver;
    private GastroErp.Presentation.Resolution.IBranchResolver? _branchResolver;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    
    protected Guid TenantId => (_tenantResolver ??= HttpContext.RequestServices.GetRequiredService<GastroErp.Presentation.Resolution.ITenantResolver>()).ResolveTenantId() ?? Guid.Empty;
    
    protected Guid? BranchId => (_branchResolver ??= HttpContext.RequestServices.GetRequiredService<GastroErp.Presentation.Resolution.IBranchResolver>()).ResolveBranchId();

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return result.ErrorCode switch
        {
            _ when result.ErrorCode != null && result.ErrorCode.StartsWith("NotFound") => NotFound(new { error = result.ErrorMessage }),
            _ when result.ErrorCode != null && result.ErrorCode.StartsWith("Unauthorized") => Unauthorized(new { error = result.ErrorMessage }),
            _ => UnprocessableEntity(new { error = result.ErrorMessage, code = result.ErrorCode })
        };
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return result.ErrorCode switch
        {
            _ when result.ErrorCode != null && result.ErrorCode.StartsWith("NotFound") => NotFound(new { error = result.ErrorMessage }),
            _ when result.ErrorCode != null && result.ErrorCode.StartsWith("Unauthorized") => Unauthorized(new { error = result.ErrorMessage }),
            _ => UnprocessableEntity(new { error = result.ErrorMessage, code = result.ErrorCode })
        };
    }

    protected IActionResult HandlePagedResult<T>(PagedResult<T> result)
    {
        if (result.IsSuccess)
        {
            Response.Headers.Append("X-Pagination-TotalCount", result.TotalCount.ToString());
            Response.Headers.Append("X-Pagination-PageNumber", result.PageNumber.ToString());
            Response.Headers.Append("X-Pagination-PageSize", result.PageSize.ToString());
            Response.Headers.Append("X-Pagination-TotalPages", result.TotalPages.ToString());

            return Ok(result.Data);
        }

        return UnprocessableEntity(new { error = result.ErrorMessage, code = result.ErrorCode });
    }
}
