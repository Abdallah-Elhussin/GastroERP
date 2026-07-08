using GastroErp.Application.Common.Interfaces.Security;
using GastroErp.Domain.Entities.Identity;
using GastroErp.Domain.Entities.Organization;
using GastroErp.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence;

public class ApplicationDbContextInitializer
{
    public const string DefaultAdminEmail = "admin@gastroerp.com";
    public const string DefaultAdminPassword = "admin";

    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly TenantMasterDataSeeder _masterDataSeeder;
    private readonly IPasswordHasher _passwordHasher;

    public ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger,
        ApplicationDbContext context,
        TenantMasterDataSeeder masterDataSeeder,
        IPasswordHasher passwordHasher)
    {
        _logger = logger;
        _context = context;
        _masterDataSeeder = masterDataSeeder;
        _passwordHasher = passwordHasher;
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

        var adminRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.TenantId == tenantEntity.Id && r.Name == "Administrator");

        if (adminRole is null)
        {
            adminRole = new Role(tenantEntity.Id, "Administrator", "مدير النظام", "Has all permissions");
            _context.Roles.Add(adminRole);
            await _context.SaveChangesAsync();
        }

        var adminUser = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.TenantId == tenantEntity.Id &&
                (u.Email == DefaultAdminEmail || u.Email == "admin"));

        if (adminUser is null)
        {
            adminUser = new AppUser(
                tenantEntity.Id,
                DefaultAdminEmail,
                _passwordHasher.HashPassword(DefaultAdminPassword),
                "System",
                "Admin");
            _context.AppUsers.Add(adminUser);
            await _context.SaveChangesAsync();
        }
        else
        {
            await EnsureAdminCredentialsAsync(adminUser);
        }

        if (!await _context.UserRoles.AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id))
        {
            _context.UserRoles.Add(new UserRole(adminUser.Id, adminRole.Id, tenantEntity.Id));
            await _context.SaveChangesAsync();
        }

        await _masterDataSeeder.SeedAllTenantsAsync(_context);
        _logger.LogInformation("Database seed completed.");
    }

    private async Task EnsureAdminCredentialsAsync(AppUser adminUser)
    {
        var changed = false;

        if (!string.Equals(adminUser.Email, DefaultAdminEmail, StringComparison.Ordinal))
        {
            adminUser.UpdateEmail(DefaultAdminEmail);
            changed = true;
        }

        if (!adminUser.PasswordHash.StartsWith("$2", StringComparison.Ordinal))
        {
            adminUser.ChangePassword(_passwordHasher.HashPassword(DefaultAdminPassword));
            changed = true;
        }

        if (changed)
        {
            await _context.SaveChangesAsync();
        }
    }
}
