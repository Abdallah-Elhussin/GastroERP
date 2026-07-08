using GastroErp.Presentation.HealthChecks;
using GastroErp.Presentation.Infrastructure.ApiKeys;
using GastroErp.Presentation.Infrastructure.Metrics;
using GastroErp.Presentation.Middlewares;
using GastroErp.Presentation.Swagger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace GastroErp.Presentation.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePresentationPipeline(this IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        if (!env.IsDevelopment())
        {
            app.UseHsts();
        }
        app.UseHttpsRedirection();

        // 1. Exception Handling (Top of pipeline)
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // 2. Correlation ID
        app.UseMiddleware<CorrelationIdMiddleware>();

        // 3. Request Logging & Metrics
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<MetricsMiddleware>();

        // 4. Security Headers
        app.UseMiddleware<SecurityHeadersMiddleware>();

        // 5. Response Compression
        app.UseResponseCompression();

        // 6. Swagger
        app.UseSwaggerDocumentation();

        // 7. Routing
        app.UseRouting();

        // 7.1. CORS
        app.UseCors("DefaultCorsPolicy");

        // 7.2. Rate Limiting
        app.UseRateLimiter();

        // 8. Idempotency & ApiKeys
        app.UseMiddleware<IdempotencyMiddleware>();
        app.UseMiddleware<ApiKeyMiddleware>();

        // 9. Auth
        app.UseAuthentication();
        app.UseTenantResolution();
        app.UseAuthorization();

        // 10. Endpoints
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers().RequireRateLimiting("Fixed");
            
            // Health Checks
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false // Basic check
            });

            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriter.WriteResponse
            });
            
            endpoints.MapHealthChecks("/health/db", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("db"),
                ResponseWriter = HealthCheckResponseWriter.WriteResponse
            });
        });

        return app;
    }
}
