namespace AcadSign.Backend.Application.Common.Interfaces;

public interface IS3StorageService
{
    Task<string> UploadDocumentAsync(byte[] pdfData, string documentId);
    Task<byte[]> DownloadDocumentAsync(string documentId);
    Task<string> GeneratePresignedDownloadUrlAsync(string documentId, int expiryMinutes = 60);
    Task DeleteDocumentAsync(string documentId);
    Task<bool> DocumentExistsAsync(string documentId);
}
