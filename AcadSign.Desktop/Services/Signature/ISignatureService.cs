using System;
using System.Threading.Tasks;

namespace AcadSign.Desktop.Services.Signature;

public interface ISignatureService
{
    Task<byte[]> SignPdfAsync(byte[] unsignedPdf, string pin);
    Task<byte[]> SignDocumentAsync(Guid documentId);
    Task<bool> ValidateSignatureAsync(byte[] signedDocument);
}
