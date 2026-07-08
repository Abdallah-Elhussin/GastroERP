namespace GastroErp.Application.Common.Interfaces.Security;

/// <summary>
/// واجهة توليد المعرفات الفريدة (Guid Generator Interface)
/// </summary>
public interface IGuidGenerator
{
    Guid Generate();
}
