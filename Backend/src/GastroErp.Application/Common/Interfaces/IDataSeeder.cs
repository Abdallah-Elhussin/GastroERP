namespace GastroErp.Application.Common.Interfaces;

/// <summary>Idempotent tenant-scoped master data seeder.</summary>
public interface IDataSeeder
{
    int Order { get; }
    Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken cancellationToken = default);
}
