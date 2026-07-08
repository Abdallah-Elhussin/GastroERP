using GastroErp.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Pdf;

/// <summary>
/// خدمة تحويل الـ HTML أو النصوص إلى PDF
/// (تطبيق مبدئي / Skeleton)
/// </summary>
public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
    }

    public byte[] GeneratePdf(string htmlContent)
    {
        _logger.LogInformation("Generating PDF from HTML content (Skeleton). Length: {Length}", htmlContent?.Length ?? 0);
        
        // TODO: استخدم مكتبة مثل DinkToPdf أو PuppeteerSharp هنا مستقبلاً
        // حالياً سنعيد ملف PDF فارغ وهمي
        return new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A, 0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A };
    }
}
