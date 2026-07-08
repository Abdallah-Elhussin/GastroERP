using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Common.Interfaces.Platform;

public interface IDatabaseProviderConfigurator
{
    void Configure(DbContextOptionsBuilder optionsBuilder, string connectionString, string migrationsAssembly);
}
