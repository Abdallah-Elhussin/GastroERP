namespace GastroErp.Application.Common.Interfaces;

/// <summary>
/// واجهة توليد QR Code للمنيو
/// </summary>
public interface IMenuQrCodeGenerator
{
    byte[] Generate(string menuUrl);
}
