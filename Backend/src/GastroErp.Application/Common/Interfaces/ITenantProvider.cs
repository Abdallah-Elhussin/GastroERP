using System;

namespace GastroErp.Application.Common.Interfaces;

public interface ITenantProvider
{
    Guid? TenantId { get; }
}
