using AcadSign.Desktop.Models;

namespace AcadSign.Desktop.Services.Api;

public interface IApiClientService
{
    Task<List<DocumentDto>> GetPendingDocumentsAsync();
    Task<byte[]> DownloadDocumentAsync(Guid documentId);
    Task UploadSignedDocumentAsync(Guid documentId, byte[] signedData);
}
