namespace GastroErp.Application.Common.Interfaces;

/// <summary>
/// واجهة طباعة الإيصالات
/// </summary>
public interface IReceiptPrinter
{
    Task PrintAsync(byte[] document, string printerName, CancellationToken cancellationToken = default);
}
