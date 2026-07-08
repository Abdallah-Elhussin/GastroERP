using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Persistence.Seeders;

namespace GastroErp.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

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
