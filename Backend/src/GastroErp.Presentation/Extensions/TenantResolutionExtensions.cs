using GastroErp.Application.Common.Interfaces.Platform;
using GastroErp.Presentation.Middlewares;

namespace GastroErp.Presentation.Extensions;

public static class TenantResolutionExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }
}
