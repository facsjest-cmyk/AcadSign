using System;
using System.Threading.Tasks;

namespace AcadSign.Desktop.Services.Signature;

public class SignatureService : ISignatureService
{
    public async Task<byte[]> SignPdfAsync(byte[] unsignedPdf, string pin)
    {
        await Task.Delay(1500);
        return unsignedPdf;
    }

    public async Task<byte[]> SignDocumentAsync(Guid documentId)
    {
        await Task.Delay(2000);
        return Array.Empty<byte>();
    }
    
    public async Task<bool> ValidateSignatureAsync(byte[] signedDocument)
    {
        await Task.Delay(500);
        return true;
    }
}
