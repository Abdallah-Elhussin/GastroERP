using GastroErp.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Printing;

/// <summary>
/// خدمة طباعة الفواتير A4 أو المقاسات الأخرى
/// </summary>
public class InvoicePrinter : IInvoicePrinter
{
    private readonly ILogger<InvoicePrinter> _logger;

    public InvoicePrinter(ILogger<InvoicePrinter> logger)
    {
        _logger = logger;
    }

    public Task PrintAsync(byte[] document, string printerName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Invoice document sent to printer {PrinterName}", printerName);
        // TODO: Implement actual printer logic
        return Task.CompletedTask;
    }
}
