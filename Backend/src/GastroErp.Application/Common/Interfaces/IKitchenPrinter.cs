namespace GastroErp.Application.Common.Interfaces;

/// <summary>
/// واجهة طباعة طلبات المطبخ
/// </summary>
public interface IKitchenPrinter
{
    Task PrintAsync(byte[] document, string printerName, CancellationToken cancellationToken = default);
}
