using GastroErp.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Printing;

/// <summary>
/// خدمة طباعة طلبات المطبخ
/// </summary>
public class KitchenPrinter : IKitchenPrinter
{
    private readonly ILogger<KitchenPrinter> _logger;

    public KitchenPrinter(ILogger<KitchenPrinter> logger)
    {
        _logger = logger;
    }

    public Task PrintAsync(byte[] document, string printerName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Kitchen document sent to printer {PrinterName}", printerName);
        // TODO: Implement actual ESC/POS or specific printer logic
        return Task.CompletedTask;
    }
}
