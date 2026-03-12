using AcadSign.Backend.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Encryption;
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

        try
        {
            var sse = new SSES3();

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("application/pdf")
                .WithServerSideEncryption(sse));

            await _minioClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SSE-S3 upload failed for {ObjectName}. Retrying upload without SSE.", objectName);

            stream.Position = 0;

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("application/pdf"));

            await _minioClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName));
        }

        _logger.LogInformation("Document {DocumentId} uploaded to bucket {BucketName} as {ObjectName}", documentId, _bucketName, objectName);

        return objectName;
    }

    public async Task<byte[]> DownloadDocumentAsync(string documentId)
    {
        _logger.LogInformation("Downloading document '{DocumentId}' from bucket {BucketName}", documentId, _bucketName);

        var sse = new SSES3();

        foreach (var objectName in GetCandidateObjectPaths(documentId))
        {
            try
            {
                _logger.LogInformation("Trying object key {ObjectName}", objectName);
                using var memoryStream = new MemoryStream();

                await _minioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithServerSideEncryption(sse)
                    .WithCallbackStream(stream =>
                    {
                        stream.CopyTo(memoryStream);
                    }));

                return memoryStream.ToArray();
            }
            catch (ObjectNotFoundException)
            {
                _logger.LogInformation("Object key {ObjectName} not found", objectName);
                // Try next candidate
            }
        }

        throw new ObjectNotFoundException();
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
        if (documentId.Contains('/') || documentId.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return documentId;
        }

        return $"{documentId}.pdf";
    }

    private IEnumerable<string> GetCandidateObjectPaths(string documentId)
    {
        // 1) Exact path or explicit .pdf key
        yield return GetObjectPath(documentId);

        // 2) If caller passed a full key with .pdf, also try without extension
        if (documentId.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            var withoutExt = documentId[..^4];
            yield return GetObjectPath(withoutExt);
        }

        // 3) Legacy path (previous implementation used year/month folders)
        if (!documentId.Contains('/'))
        {
            var now = DateTime.UtcNow;
            var normalized = documentId.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ? documentId[..^4]
                : documentId;

            yield return $"{now.Year:D4}/{now.Month:D2}/{normalized}.pdf";
        }
    }
}
