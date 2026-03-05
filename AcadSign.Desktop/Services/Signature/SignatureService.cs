namespace AcadSign.Desktop.Services.Signature;

public class SignatureService : ISignatureService
{
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
