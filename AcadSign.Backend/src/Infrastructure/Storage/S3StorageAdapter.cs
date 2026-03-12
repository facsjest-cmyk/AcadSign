using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Services;

namespace AcadSign.Backend.Infrastructure.Storage;

public class S3StorageAdapter : IStorageService
{
    private readonly IS3StorageService _s3;

    public S3StorageAdapter(IS3StorageService s3)
    {
        _s3 = s3;
    }

    public async Task UploadDocumentAsync(byte[] fileBytes, string fileName)
    {
        await _s3.UploadDocumentAsync(fileBytes, fileName);
    }

    public async Task<byte[]> DownloadDocumentAsync(string fileName)
    {
        return await _s3.DownloadDocumentAsync(fileName);
    }

    public async Task<string> GeneratePreSignedUrlAsync(string fileName, TimeSpan expiration)
    {
        var minutes = (int)Math.Ceiling(expiration.TotalMinutes);
        return await _s3.GeneratePresignedDownloadUrlAsync(fileName, minutes);
    }

    public async Task DeleteDocumentAsync(string fileName)
    {
        await _s3.DeleteDocumentAsync(fileName);
    }
}
