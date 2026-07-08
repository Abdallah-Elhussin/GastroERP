using GastroErp.Application.Common.Interfaces;

namespace GastroErp.Infrastructure.DateTime;

/// <summary>
/// مزود الوقت الحالي (Machine Date Time Provider)
/// </summary>
public class MachineDateTime : IDateTime
{
    public System.DateTime UtcNow => System.DateTime.UtcNow;
}
