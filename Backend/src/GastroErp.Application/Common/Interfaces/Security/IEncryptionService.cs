namespace GastroErp.Application.Common.Interfaces.Security;

/// <summary>
/// واجهة التشفير وفك التشفير (Encryption and Decryption Interface)
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
