using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Ai;
using GastroErp.Application.Common.Interfaces.BackgroundJobs;
using GastroErp.Infrastructure.Ai;
using GastroErp.Application.Common.Interfaces.Logging;
using GastroErp.Application.Common.Interfaces.Resilience;
using GastroErp.Application.Common.Interfaces.Sync;
using GastroErp.Infrastructure.Authentication;
using GastroErp.Application.Features.Automation.Services;
using GastroErp.Infrastructure.BackgroundJobs;
using GastroErp.Infrastructure.Integrations;
using GastroErp.Infrastructure.Barcode;
using GastroErp.Infrastructure.Cache;
using GastroErp.Infrastructure.DateTime;
using GastroErp.Infrastructure.Health;
using GastroErp.Infrastructure.Localization;
using GastroErp.Infrastructure.Logging;
using GastroErp.Infrastructure.Notifications;
using GastroErp.Infrastructure.Options;
using GastroErp.Infrastructure.Pdf;
using GastroErp.Infrastructure.Printing;
using GastroErp.Infrastructure.QrCode;
using GastroErp.Infrastructure.Resilience;
using GastroErp.Infrastructure.Security;
using GastroErp.Infrastructure.Storage;
using GastroErp.Infrastructure.Sync;
using GastroErp.Infrastructure.Tenant;
using GastroErp.Application.Common.Interfaces.Security;
using GastroErp.Application.Common.Interfaces.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GastroErp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<PrintingOptions>(configuration.GetSection(PrintingOptions.SectionName));
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));

        // Security
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IEncryptionService, EncryptionService>();
        services.AddSingleton<IRandomGenerator, RandomGenerator>();
        services.AddSingleton<IGuidGenerator, GuidGenerator>();

        // Authentication & Identity
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenService>();
        services.AddScoped<IJwtTokenValidator, JwtTokenValidator>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IClaimsFactory, ClaimsFactory>();

        // Tenant & DateTime
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddSingleton<IDateTime, MachineDateTime>();

        // QrCode & Barcode
        services.AddSingleton<IInvoiceQrCodeGenerator, InvoiceQrCodeGenerator>();
        services.AddSingleton<IMenuQrCodeGenerator, MenuQrCodeGenerator>();
        services.AddSingleton<IProductBarcodeGenerator, ProductBarcodeGenerator>();
        services.AddSingleton<IInventoryBarcodeGenerator, InventoryBarcodeGenerator>();

        // Notifications
        services.AddHttpClient("Webhooks");
        services.AddHttpClient("OpenAi");
        services.AddScoped<IWebhookDispatchService, WebhookDispatchService>();
        services.AddTransient<IEmailSender, SmtpEmailSender>();
        services.AddTransient<ISmsSender, DummySmsSender>();
        services.AddTransient<INotificationService, NotificationDispatcher>();

        // Printing & PDF
        services.AddTransient<IReceiptPrinter, ReceiptPrinter>();
        services.AddTransient<IKitchenPrinter, KitchenPrinter>();
        services.AddTransient<IInvoicePrinter, InvoicePrinter>();
        // services.AddTransient<ILabelPrinter, LabelPrinter>();
        services.AddTransient<IPdfService, PdfService>();

        // Localization
        services.AddSingleton<ILocalizationService, LocalizationService>();

        // Logging
        services.AddSingleton<IAuditLogger, AuditLogger>();
        services.AddSingleton<ISecurityLogger, SecurityLogger>();
        services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
        services.AddSingleton<IBusinessLogger, BusinessLogger>();

        // Cache
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // Storage
        services.AddTransient<IFileStorage, LocalStorage>();
        // Skeletons:
        // services.AddTransient<IFileStorage, AzureBlobStorage>();
        // services.AddTransient<IFileStorage, AwsS3Storage>();
        // services.AddTransient<IFileStorage, MinIoStorage>();

        // Sync
        services.AddScoped<ISyncAgent, SyncAgent>();
        services.AddScoped<ISyncQueue, SyncQueue>();
        services.AddScoped<ISyncDispatcher, SyncDispatcher>();

        // Resilience
        services.AddSingleton<IResilienceService, ResilienceService>();

        // Background Jobs
        services.AddSingleton<IBackgroundJobService, BackgroundJobService>();
        services.AddSingleton<IBackgroundJobManager, BackgroundJobManager>();
        services.AddSingleton<RecurringJobHostedService>();
        services.AddHostedService(sp => sp.GetRequiredService<RecurringJobHostedService>());
        services.AddSingleton<IRecurringJobService>(sp => sp.GetRequiredService<RecurringJobHostedService>());

        services.AddSingleton<IPaymentGatewayAdapter, StripeAdapter>();
        services.AddSingleton<IPaymentGatewayAdapter, MyFatoorahAdapter>();
        services.AddSingleton<IPaymentGatewayAdapter, HyperPayAdapter>();
        services.AddSingleton<IPaymentGatewayAdapter, MoyasarAdapter>();

        var aiProvider = configuration.GetSection(AiOptions.SectionName).GetValue<string>("Provider") ?? "Heuristic";
        if (string.Equals(aiProvider, "OpenAI", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IGenerativeAiAdapter, OpenAiGenerativeAiAdapter>();
        else
            services.AddSingleton<IGenerativeAiAdapter, HeuristicGenerativeAiAdapter>();

        services.AddScoped<IFraudAnalysisEngine, HeuristicFraudAnalysisEngine>();
        services.AddScoped<ICustomerSegmentationEngine, HeuristicCustomerSegmentationEngine>();
        services.AddScoped<IChurnPredictionEngine, HeuristicChurnPredictionEngine>();
        services.AddScoped<IProductRecommendationEngine, HeuristicProductRecommendationEngine>();

        // Health Checks
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database")
            .AddCheck<CacheHealthCheck>("Cache")
            .AddCheck<StorageHealthCheck>("Storage")
            .AddCheck<SmtpHealthCheck>("SMTP")
            .AddCheck<SmsHealthCheck>("SMS")
            .AddCheck<QueueHealthCheck>("Queue")
            .AddCheck<AiIntelligenceHealthCheck>("AIIntelligence")
            .AddCheck<HrWorkforceHealthCheck>("HRWorkforce")
            .AddCheck<WorkflowEngineHealthCheck>("WorkflowEngine")
            .AddCheck<ReportingHealthCheck>("ReportingPlatform");

        return services;
    }
}
