namespace AcadSign.Desktop.Services.Signature;

public interface ISignatureService
{
    Task<byte[]> SignDocumentAsync(Guid documentId);
    Task<bool> ValidateSignatureAsync(byte[] signedDocument);
}
