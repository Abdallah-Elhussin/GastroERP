using GastroErp.Application.Common.Interfaces;
using QRCoder;

namespace GastroErp.Infrastructure.QrCode;

/// <summary>
/// خدمة توليد كيو آر كود لروابط المنيو
/// </summary>
public class MenuQrCodeGenerator : IMenuQrCodeGenerator
{
    public byte[] Generate(string menuUrl)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(menuUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        return qrCode.GetGraphic(20);
    }
}
