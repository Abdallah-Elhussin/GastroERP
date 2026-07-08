namespace GastroErp.Presentation.Resolution;

public interface IBranchResolver
{
    Guid? ResolveBranchId();
}

public class BranchResolver : IBranchResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BranchResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? ResolveBranchId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        // 1. JWT Claims
        var branchClaim = context.User.Claims.FirstOrDefault(c => c.Type == "BranchId")?.Value;
        if (Guid.TryParse(branchClaim, out var branchId))
        {
            return branchId;
        }

        // 2. X-Branch Header
        if (context.Request.Headers.TryGetValue("X-Branch", out var branchHeader))
        {
            if (Guid.TryParse(branchHeader.ToString(), out branchId))
            {
                return branchId;
            }
        }

        return null;
    }
}
