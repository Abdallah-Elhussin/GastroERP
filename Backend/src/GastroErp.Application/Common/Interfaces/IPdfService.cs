namespace GastroErp.Application.Common.Interfaces;

public interface IPdfService
{
    byte[] GeneratePdf(string htmlContent);
}
