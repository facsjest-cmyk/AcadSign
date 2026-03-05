namespace AcadSign.Backend.Application.Services;

public interface IStorageService
{
    Task UploadDocumentAsync(byte[] fileBytes, string fileName);
    Task<byte[]> DownloadDocumentAsync(string fileName);
    Task<string> GeneratePreSignedUrlAsync(string fileName, TimeSpan expiration);
    Task DeleteDocumentAsync(string fileName);
}
