namespace GastroErp.Infrastructure.Options;

/// <summary>
/// إعدادات الطباعة (Printing Options)
/// </summary>
public class PrintingOptions
{
    public const string SectionName = "Printing";

    public string DefaultReceiptPrinter { get; set; } = string.Empty;
    public string DefaultInvoicePrinter { get; set; } = string.Empty;
    public string DefaultKitchenPrinter { get; set; } = string.Empty;
}
