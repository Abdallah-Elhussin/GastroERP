namespace GastroErp.Application.Common.Interfaces.Platform;

public interface ITenantResolver
{
    Guid? ResolveTenantId();
    string? ResolveTenantSlug();
}
