using Asp.Versioning;
using GastroErp.Application.Common.Interfaces.Realtime;
using GastroErp.Application.Common.Options;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Filters;
using GastroErp.Presentation.Infrastructure.FeatureFlags;
using GastroErp.Presentation.Infrastructure.FileUpload;
using GastroErp.Presentation.Infrastructure.Webhooks;
using GastroErp.Presentation.Realtime;
using GastroErp.Presentation.Resolution;
using GastroErp.Presentation.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GastroErp.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Controllers & API Behavior
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidateModelFilter>();
            options.Filters.Add<AuditLogFilter>();
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            // Suppress default validation as we use ValidateModelFilter
            options.SuppressModelStateInvalidFilter = true;
        });

        // 2. API Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("x-api-version"),
                new MediaTypeApiVersionReader("x-api-version"));
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.Configure<AuthJwtSettings>(configuration.GetSection(AuthJwtSettings.SectionName));

        // 3. Authentication (JWT)
        var jwtSigningKey = configuration["Jwt:Secret"]
            ?? configuration["Jwt:Key"]
            ?? "super-secret-key-that-should-be-very-long-for-hmac-sha256";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"] ?? "GastroErp",
                ValidAudience = configuration["Jwt:Audience"] ?? "GastroErpClient",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        context.Token = accessToken;
                    return Task.CompletedTask;
                }
            };
        });

        // 4. Authorization
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // 5. Swagger
        services.AddSwaggerDocumentation();

        // 6. Resolvers
        services.AddScoped<GastroErp.Presentation.Resolution.IBranchResolver, BranchResolver>();
        services.AddHttpContextAccessor();

        // 7. Passive Infrastructure
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();
        services.AddScoped<IFileUploadService, LocalFileUploadService>();

        // 8. Health Checks
        services.AddHealthChecks(); // Further config like .AddSqlServer() can be done when connecting Persistence

        // 9. Response Compression
        services.AddResponseCompression();

        // 10. CORS
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCorsPolicy", builder =>
            {
                if (allowedOrigins.Contains("*"))
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
                else if (allowedOrigins.Any())
                {
                    builder.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                }
                else
                {
                    // Fallback
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            });
        });

        // 11. Rate Limiting
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("Fixed", opt =>
            {
                opt.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:FixedWindowSeconds", 10));
                opt.PermitLimit = configuration.GetValue<int>("RateLimiting:FixedPermitLimit", 100);
                opt.QueueLimit = configuration.GetValue<int>("RateLimiting:QueueLimit", 2);
                opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            });
            
            options.AddSlidingWindowLimiter("Sliding", opt =>
            {
                opt.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:SlidingWindowSeconds", 30));
                opt.PermitLimit = configuration.GetValue<int>("RateLimiting:SlidingPermitLimit", 100);
                opt.SegmentsPerWindow = configuration.GetValue<int>("RateLimiting:SegmentsPerWindow", 3);
                opt.QueueLimit = configuration.GetValue<int>("RateLimiting:QueueLimit", 2);
                opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        // 12. Request Limits
        services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = configuration.GetValue<long>("RequestLimits:MaxRequestBodySize", 10485760);
        });
        
        services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = configuration.GetValue<long>("RequestLimits:MaxRequestBodySize", 10485760);
        });

        services.AddSignalR();
        services.AddScoped<IKitchenRealtimeNotifier, KitchenRealtimeNotifier>();

        return services;
    }
}
