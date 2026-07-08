namespace GastroErp.Application.Common.Interfaces;

/// <summary>
/// واجهة طباعة الفواتير A4 أو غيرها
/// </summary>
public interface IInvoicePrinter
{
    Task PrintAsync(byte[] document, string printerName, CancellationToken cancellationToken = default);
}
