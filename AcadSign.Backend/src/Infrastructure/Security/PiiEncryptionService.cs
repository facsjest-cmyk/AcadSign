using System.Security.Cryptography;
using AcadSign.Backend.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace AcadSign.Backend.Infrastructure.Security;

public class PiiEncryptionService : IPiiEncryptionService
{
    private readonly IDataProtector _protector;

    public PiiEncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("AcadSign.PII.v1");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return plainText;
        }

        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return cipherText;
        }

        try
        {
            return _protector.Unprotect(cipherText);
        }
        catch (CryptographicException)
        {
            throw new InvalidOperationException("Unable to decrypt data. The encryption key may have changed.");
        }
    }
}
