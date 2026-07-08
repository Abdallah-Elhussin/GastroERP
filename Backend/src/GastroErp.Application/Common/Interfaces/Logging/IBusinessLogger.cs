namespace GastroErp.Application.Common.Interfaces.Logging;

public interface IBusinessLogger
{
    void LogImportantAction(string actionName, string userId, string details);
}
