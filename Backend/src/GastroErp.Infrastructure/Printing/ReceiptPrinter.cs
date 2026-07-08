using GastroErp.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Printing;

/// <summary>
/// خدمة طباعة الإيصالات
/// </summary>
public class ReceiptPrinter : IReceiptPrinter
{
    private readonly ILogger<ReceiptPrinter> _logger;

    public ReceiptPrinter(ILogger<ReceiptPrinter> logger)
    {
        _logger = logger;
    }

    public Task PrintAsync(byte[] document, string printerName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Receipt document sent to printer {PrinterName}", printerName);
        // TODO: Implement actual ESC/POS or specific printer logic
        return Task.CompletedTask;
    }
}
