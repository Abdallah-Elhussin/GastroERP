namespace GastroErp.Application.Common.Interfaces;

/// <summary>
/// واجهة توليد الباركود للمخزون (للطباعة على الملصقات)
/// </summary>
public interface IInventoryBarcodeGenerator
{
    byte[] Generate(string batchNumber, DateTime expiryDate);
}
