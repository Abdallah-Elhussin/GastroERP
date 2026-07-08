using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Platform;
using GastroErp.Application.Common.Options;
using GastroErp.Persistence.Platform;
using GastroErp.Persistence.Seeders;

namespace GastroErp.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.AddScoped<IDatabaseProviderConfigurator, DatabaseProviderConfigurator>();
        services.AddScoped<IIdentityPlatformSeedService, IdentityPlatformSeedService>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var resolver = serviceProvider.GetRequiredService<IConnectionStringResolver>();
            var configurator = serviceProvider.GetRequiredService<IDatabaseProviderConfigurator>();
            var connectionString = resolver.Resolve();
            configurator.Configure(
                options,
                connectionString,
                typeof(ApplicationDbContext).Assembly.FullName!);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ApplicationDbContextInitializer>();
        services.AddScoped<TenantMasterDataSeeder>();
        services.AddScoped<ITenantMasterDataSeedService, TenantMasterDataSeedService>();

        services.AddScoped<IDataSeeder, OrganizationMasterDataSeeder>();
        services.AddScoped<IDataSeeder, TaxAndFiscalSeeder>();
        services.AddScoped<IDataSeeder, ChartOfAccountsSeeder>();
        services.AddScoped<IDataSeeder, InventoryMasterDataSeeder>();
        services.AddScoped<IDataSeeder, MenuMasterDataSeeder>();
        services.AddScoped<IDataSeeder, WorkflowDefinitionsSeeder>();

        return services;
    }
}
