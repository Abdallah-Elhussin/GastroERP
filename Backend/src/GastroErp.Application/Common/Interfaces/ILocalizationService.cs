namespace GastroErp.Application.Common.Interfaces;

public interface ILocalizationService
{
    string GetMessage(string key, params object[] args);
}
