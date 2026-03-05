using AcadSign.Backend.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace AcadSign.Backend.Infrastructure.Storage;

public class S3StorageService : IS3StorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(
        IMinioClient minioClient,
        IConfiguration configuration,
        ILogger<S3StorageService> logger)
    {
        _minioClient = minioClient;
        _bucketName = configuration["MinIO:BucketName"] ?? "acadsign-documents";
        _logger = logger;

        EnsureBucketExistsAsync().Wait();
    }

    private async Task EnsureBucketExistsAsync()
    {
        try
        {
            var bucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_bucketName));

            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs()
                        .WithBucket(_bucketName)
                        .WithLocation("us-east-1"));

                await _minioClient.SetVersioningAsync(
                    new SetVersioningArgs()
                        .WithBucket(_bucketName)
                        .WithVersioningEnabled());

                _logger.LogInformation("Bucket {BucketName} created with versioning enabled", _bucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring bucket exists");
            throw;
        }
    }

    public async Task<string> UploadDocumentAsync(byte[] pdfData, string documentId)
    {
        var objectName = GetObjectPath(documentId);

        using var stream = new MemoryStream(pdfData);

        var sse = new SSES3();

        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType("application/pdf")
            .WithServerSideEncryption(sse));

        _logger.LogInformation("Document {DocumentId} uploaded to {ObjectName}", documentId, objectName);

        return objectName;
    }

    public async Task<byte[]> DownloadDocumentAsync(string documentId)
    {
        var objectName = GetObjectPath(documentId);

        using var memoryStream = new MemoryStream();

        await _minioClient.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream =>
            {
                stream.CopyTo(memoryStream);
            }));

        return memoryStream.ToArray();
    }

    public async Task<string> GeneratePresignedDownloadUrlAsync(string documentId, int expiryMinutes = 60)
    {
        var objectName = GetObjectPath(documentId);

        var presignedUrl = await _minioClient.PresignedGetObjectAsync(
            new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithExpiry(expiryMinutes * 60));

        return presignedUrl;
    }

    public async Task DeleteDocumentAsync(string documentId)
    {
        var objectName = GetObjectPath(documentId);

        await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName));

        _logger.LogInformation("Document {DocumentId} deleted from {ObjectName}", documentId, objectName);
    }

    public async Task<bool> DocumentExistsAsync(string documentId)
    {
        var objectName = GetObjectPath(documentId);

        try
        {
            await _minioClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName));

            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
    }

    private string GetObjectPath(string documentId)
    {
        var now = DateTime.UtcNow;
        return $"{now.Year:D4}/{now.Month:D2}/{documentId}.pdf";
    }
}
