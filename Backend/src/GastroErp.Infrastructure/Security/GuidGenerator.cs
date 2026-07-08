using GastroErp.Application.Common.Interfaces.Security;

namespace GastroErp.Infrastructure.Security;

/// <summary>
/// خدمة توليد المعرفات الفريدة (Guid Generator)
/// </summary>
public class GuidGenerator : IGuidGenerator
{
    public Guid Generate()
    {
        return Guid.NewGuid();
    }
}
