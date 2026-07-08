namespace GastroErp.Application.Common.Interfaces;

/// <summary>
/// واجهة توليد الباركود للمنتجات
/// </summary>
public interface IProductBarcodeGenerator
{
    byte[] Generate(string productCode);
}
