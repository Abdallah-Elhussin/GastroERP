using GastroErp.Application.Common.Interfaces.Platform;
using GastroErp.Application.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GastroErp.Infrastructure.Platform;

public sealed class ConnectionStringResolver : IConnectionStringResolver
{
    private readonly IConfiguration _configuration;
    private readonly DatabaseOptions _databaseOptions;
    private readonly IHostEnvironment _environment;

    public ConnectionStringResolver(
        IConfiguration configuration,
        IOptions<DatabaseOptions> databaseOptions,
        IHostEnvironment environment)
    {
        _configuration = configuration;
        _databaseOptions = databaseOptions.Value;
        _environment = environment;
    }

    public string Resolve(string? connectionStringName = null, Guid? tenantId = null)
    {
        var name = connectionStringName ?? _databaseOptions.ConnectionStringName;

        if (tenantId.HasValue && tenantId != Guid.Empty)
        {
            var tenantConnection = _configuration.GetConnectionString($"Tenant_{tenantId:N}");
            if (!string.IsNullOrWhiteSpace(tenantConnection))
            {
                return tenantConnection;
            }
        }

        var environmentSpecific = _configuration.GetConnectionString($"{name}_{_environment.EnvironmentName}");
        if (!string.IsNullOrWhiteSpace(environmentSpecific))
        {
            return environmentSpecific;
        }

        var connection = _configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connection))
        {
            throw new InvalidOperationException($"Connection string '{name}' was not found for environment '{_environment.EnvironmentName}'.");
        }

        return connection;
    }
}
