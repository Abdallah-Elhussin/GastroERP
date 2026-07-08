using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Identity;
using GastroErp.Domain.Entities.Organization;
using GastroErp.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence;

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly TenantMasterDataSeeder _masterDataSeeder;

    public ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger,
        ApplicationDbContext context,
        TenantMasterDataSeeder masterDataSeeder)
    {
        _logger = logger;
        _context = context;
        _masterDataSeeder = masterDataSeeder;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        var tenantEntity = await _context.Tenants.FirstOrDefaultAsync(t => t.Slug == "default");
        if (tenantEntity is null)
        {
            tenantEntity = new Tenant("الشركة الافتراضية", "default", "SAR", "ar", "Arab Standard Time", "Default Tenant");
            _context.Tenants.Add(tenantEntity);
            await _context.SaveChangesAsync();
        }

        if (!await _context.Roles.AnyAsync(r => r.TenantId == tenantEntity.Id && r.Name == "Administrator"))
        {
            var role = new Role(tenantEntity.Id, "Administrator", "مدير النظام", "Has all permissions");
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
        }

        if (!await _context.AppUsers.AnyAsync(u => u.TenantId == tenantEntity.Id && u.Email == "admin"))
        {
            var adminUser = new AppUser(tenantEntity.Id, "admin", "admin", "System", "Admin");
            _context.AppUsers.Add(adminUser);
            await _context.SaveChangesAsync();
        }

        await _masterDataSeeder.SeedAllTenantsAsync(_context);
        _logger.LogInformation("Database seed completed.");
    }
}
