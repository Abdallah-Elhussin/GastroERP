namespace GastroErp.Application.Common.Interfaces.Platform;

public interface IConnectionStringResolver
{
    string Resolve(string? connectionStringName = null, Guid? tenantId = null);
}
