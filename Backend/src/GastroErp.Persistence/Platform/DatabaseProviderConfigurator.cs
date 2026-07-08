using GastroErp.Application.Common.Interfaces.Platform;
using GastroErp.Application.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GastroErp.Persistence.Platform;

public sealed class DatabaseProviderConfigurator : IDatabaseProviderConfigurator
{
    private readonly DatabaseOptions _options;

    public DatabaseProviderConfigurator(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public void Configure(DbContextOptionsBuilder optionsBuilder, string connectionString, string migrationsAssembly)
    {
        switch (_options.Provider.Trim().ToLowerInvariant())
        {
            case "postgresql":
            case "postgres":
                optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly(migrationsAssembly));
                break;
            case "sqlserver":
            default:
                optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly(migrationsAssembly));
                break;
        }
    }
}
