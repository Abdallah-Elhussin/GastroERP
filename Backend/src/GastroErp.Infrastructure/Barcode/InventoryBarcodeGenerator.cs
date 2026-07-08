using BarcodeStandard;
using GastroErp.Application.Common.Interfaces;

namespace GastroErp.Infrastructure.Barcode;

/// <summary>
/// خدمة توليد باركود المخزون والباتشات
/// </summary>
public class InventoryBarcodeGenerator : IInventoryBarcodeGenerator
{
    public byte[] Generate(string batchNumber, System.DateTime expiryDate)
    {
        var barcodeText = $"{batchNumber}-{expiryDate:yyMMdd}";
        
        var barcode = new BarcodeStandard.Barcode();
        var image = barcode.Encode(BarcodeStandard.Type.Code128, barcodeText, SkiaSharp.SKColors.Black, SkiaSharp.SKColors.White, 350, 100);
        
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
