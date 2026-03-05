namespace AcadSign.Backend.Application.Common.Interfaces;

public interface IPiiEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
