using System;

namespace GastroErp.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? Id { get; }
    string? Email { get; }
    string? Name { get; }
    Guid TenantId { get; }
    bool IsAuthenticated { get; }
}
