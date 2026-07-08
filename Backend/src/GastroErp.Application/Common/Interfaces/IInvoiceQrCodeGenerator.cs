namespace GastroErp.Application.Common.Interfaces;

/// <summary>
/// واجهة توليد QR Code للفواتير (متوافق مع هيئة الزكاة والدخل ZATCA)
/// </summary>
public interface IInvoiceQrCodeGenerator
{
    byte[] Generate(string sellerName, string taxNumber, DateTime timestamp, decimal totalAmount, decimal taxAmount);
}
