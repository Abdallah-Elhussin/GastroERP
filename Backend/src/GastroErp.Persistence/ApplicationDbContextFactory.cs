using GastroErp.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GastroErp.Persistence;

/// <summary>
/// Design-time factory so migrations can run without locking Presentation binaries.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=GastroErpDb;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options;

        return new ApplicationDbContext(options, new DesignTimeCurrentUser());
    }

    private sealed class DesignTimeCurrentUser : ICurrentUser
    {
        public Guid? Id => null;
        public string? Email => null;
        public string? Name => "design-time";
        public Guid TenantId => Guid.Empty;
        public bool IsAuthenticated => false;
    }
}
