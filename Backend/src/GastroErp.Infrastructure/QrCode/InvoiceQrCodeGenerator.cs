using System.Text;
using GastroErp.Application.Common.Interfaces;
using QRCoder;

namespace GastroErp.Infrastructure.QrCode;

/// <summary>
/// خدمة توليد كيو آر كود للفواتير متوافق مع هيئة الزكاة والضريبة والجمارك
/// </summary>
public class InvoiceQrCodeGenerator : IInvoiceQrCodeGenerator
{
    public byte[] Generate(string sellerName, string taxNumber, System.DateTime invoiceDate, decimal totalAmount, decimal taxAmount)
    {
        // 1. Seller Name
        var sellerNameBytes = GetTlvValue(1, sellerName);
        // 2. VAT Registration Number
        var taxNumberBytes = GetTlvValue(2, taxNumber);
        // 3. TimeStamp
        var timeStampBytes = GetTlvValue(3, invoiceDate.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        // 4. Invoice Total (with VAT)
        var totalAmountBytes = GetTlvValue(4, totalAmount.ToString("0.00"));
        // 5. VAT Total
        var taxAmountBytes = GetTlvValue(5, taxAmount.ToString("0.00"));

        var tlvBytes = sellerNameBytes
            .Concat(taxNumberBytes)
            .Concat(timeStampBytes)
            .Concat(totalAmountBytes)
            .Concat(taxAmountBytes)
            .ToArray();

        var base64Tlv = Convert.ToBase64String(tlvBytes);

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(base64Tlv, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        return qrCode.GetGraphic(20);
    }

    private byte[] GetTlvValue(int tag, string value)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value);
        var result = new byte[2 + valueBytes.Length];
        result[0] = (byte)tag;
        result[1] = (byte)valueBytes.Length;
        Array.Copy(valueBytes, 0, result, 2, valueBytes.Length);
        return result;
    }
}
