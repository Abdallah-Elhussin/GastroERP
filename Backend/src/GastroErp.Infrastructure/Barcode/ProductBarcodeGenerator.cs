using BarcodeStandard;
using GastroErp.Application.Common.Interfaces;

namespace GastroErp.Infrastructure.Barcode;

/// <summary>
/// خدمة توليد باركود للمنتجات
/// </summary>
public class ProductBarcodeGenerator : IProductBarcodeGenerator
{
    public byte[] Generate(string productCode)
    {
        var barcode = new BarcodeStandard.Barcode();
        var image = barcode.Encode(BarcodeStandard.Type.Code128, productCode, SkiaSharp.SKColors.Black, SkiaSharp.SKColors.White, 290, 120);
        
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
