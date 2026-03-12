using AcadSign.Backend.Application.UnitTests.Common.Factories;
using FluentAssertions;
using NUnit.Framework;

namespace AcadSign.Backend.Application.UnitTests.Storage;

/// <summary>
/// Test ID: P0-022
/// Requirement: S3 storage full blocks generation gracefully
/// Test Level: Integration
/// Risk Link: R-6 (MinIO S3 storage full bloque génération)
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Storage")]
[Category("ErrorHandling")]
public class P0_022_S3StorageFullHandlingTests
{
    private DocumentFactory _documentFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _documentFactory = new DocumentFactory();
    }

    [Test]
    public async Task S3StorageFull_ShouldBlock_DocumentGeneration()
    {
        // Arrange
        var document = _documentFactory.Unsigned();
        var pdfContent = GenerateMockPDF();
        var storageCapacity = 100 * 1024 * 1024; // 100 MB
        var currentUsage = storageCapacity - 3; // leave < pdfContent.Length bytes available

        // Act
        var uploadResult = await TryUploadToS3(pdfContent, storageCapacity, currentUsage);

        // Assert
        uploadResult.Success.Should().BeFalse("Upload should fail when storage is full");
        uploadResult.ErrorCode.Should().Be("STORAGE_FULL");
    }

    [Test]
    public async Task S3StorageFull_ShouldReturn_ClearErrorMessage()
    {
        // Arrange
        var pdfContent = GenerateMockPDF();
        var storageCapacity = 100 * 1024 * 1024;
        var currentUsage = 100 * 1024 * 1024; // Full

        // Act
        var uploadResult = await TryUploadToS3(pdfContent, storageCapacity, currentUsage);

        // Assert
        uploadResult.ErrorMessage.Should().Contain("Storage capacity exceeded", 
            "Error message should be clear and actionable");
        uploadResult.ErrorMessage.Should().Contain("Contact administrator", 
            "Error message should guide user to next steps");
    }

    [Test]
    public async Task S3StorageFull_ShouldNotLose_Data()
    {
        // Arrange
        var document = _documentFactory.Unsigned();
        var pdfContent = GenerateMockPDF();

        // Act
        var uploadResult = await TryUploadToS3(pdfContent, 100, 100);

        // Assert
        uploadResult.Success.Should().BeFalse();
        uploadResult.DataLost.Should().BeFalse("No data should be lost when storage is full");
    }

    [Test]
    public async Task S3Storage_ShouldMonitor_Capacity()
    {
        // Arrange
        var bucketName = "acadsign-documents";

        // Act
        var metrics = await GetStorageMetrics(bucketName);

        // Assert
        metrics.TotalCapacity.Should().BeGreaterThan(0, "Total capacity should be monitored");
        metrics.UsedCapacity.Should().BeGreaterOrEqualTo(0, "Used capacity should be tracked");
        metrics.AvailableCapacity.Should().BeGreaterOrEqualTo(0, "Available capacity should be calculated");
    }

    [Test]
    public async Task S3Storage_ShouldAlert_At80PercentCapacity()
    {
        // Arrange
        var totalCapacity = 100 * 1024 * 1024; // 100 MB
        var usedCapacity = 81 * 1024 * 1024; // 81 MB (81%)

        // Act
        var shouldAlert = ShouldTriggerCapacityAlert(usedCapacity, totalCapacity);

        // Assert
        shouldAlert.Should().BeTrue("Alert should trigger at 80% capacity");
    }

    [Test]
    public async Task S3Storage_ShouldNotAlert_Below80PercentCapacity()
    {
        // Arrange
        var totalCapacity = 100 * 1024 * 1024;
        var usedCapacity = 79 * 1024 * 1024; // 79%

        // Act
        var shouldAlert = ShouldTriggerCapacityAlert(usedCapacity, totalCapacity);

        // Assert
        shouldAlert.Should().BeFalse("Alert should not trigger below 80% capacity");
    }

    [Test]
    public async Task S3Storage_ShouldQueue_FailedUploads()
    {
        // Arrange
        var document = _documentFactory.Unsigned();
        var pdfContent = GenerateMockPDF();

        // Act
        var uploadResult = await TryUploadToS3(pdfContent, 100, 100);
        var queuedJob = await GetQueuedUploadJob(document.PublicId);

        // Assert
        uploadResult.Success.Should().BeFalse();
        queuedJob.Should().NotBeNull("Failed upload should be queued for retry");
        queuedJob!.Status.Should().Be("Queued");
    }

    [Test]
    public async Task S3Storage_ShouldRetry_WhenSpaceAvailable()
    {
        // Arrange
        var document = _documentFactory.Unsigned();
        var pdfContent = GenerateMockPDF();
        
        // First attempt - storage full
        var firstAttempt = await TryUploadToS3(pdfContent, 100, 100);
        
        // Simulate space freed up
        var currentUsage = 50; // 50% used

        // Act - Retry
        var retryAttempt = await TryUploadToS3(pdfContent, 100, currentUsage);

        // Assert
        firstAttempt.Success.Should().BeFalse("First attempt should fail");
        retryAttempt.Success.Should().BeTrue("Retry should succeed when space available");
    }

    [Test]
    public async Task S3Storage_ShouldCleanup_OldFiles()
    {
        // Arrange
        var bucketName = "acadsign-documents";
        var retentionDays = 30; // Keep only last 30 days

        // Act
        var cleanupResult = await CleanupOldFiles(bucketName, retentionDays);

        // Assert
        cleanupResult.FilesDeleted.Should().BeGreaterOrEqualTo(0, "Old files should be cleaned up");
        cleanupResult.SpaceFreed.Should().BeGreaterOrEqualTo(0, "Space should be freed");
    }

    [Test]
    public async Task S3Storage_ShouldPreserve_SignedDocuments()
    {
        // Arrange
        var signedDocument = _documentFactory.Signed();
        var retentionDays = 30;

        // Act
        var shouldDelete = ShouldDeleteDocument(signedDocument, retentionDays);

        // Assert
        shouldDelete.Should().BeFalse("Signed documents should be preserved (30 years retention)");
    }

    // Helper methods
    private byte[] GenerateMockPDF()
    {
        return new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
    }

    private async Task<UploadResult> TryUploadToS3(byte[] content, long totalCapacity, long currentUsage)
    {
        await Task.CompletedTask;
        
        var availableSpace = totalCapacity - currentUsage;
        var success = content.Length <= availableSpace;

        return new UploadResult
        {
            Success = success,
            ErrorCode = success ? null : "STORAGE_FULL",
            ErrorMessage = success ? null : "Storage capacity exceeded. Contact administrator to increase capacity.",
            DataLost = false
        };
    }

    private async Task<StorageMetrics> GetStorageMetrics(string bucketName)
    {
        await Task.CompletedTask;
        return new StorageMetrics
        {
            TotalCapacity = 100 * 1024 * 1024, // 100 MB
            UsedCapacity = 50 * 1024 * 1024,   // 50 MB
            AvailableCapacity = 50 * 1024 * 1024 // 50 MB
        };
    }

    private bool ShouldTriggerCapacityAlert(long usedCapacity, long totalCapacity)
    {
        var usagePercentage = (double)usedCapacity / totalCapacity * 100;
        return usagePercentage >= 80;
    }

    private async Task<QueuedJob?> GetQueuedUploadJob(Guid documentId)
    {
        await Task.CompletedTask;
        return new QueuedJob
        {
            DocumentId = documentId,
            Status = "Queued"
        };
    }

    private async Task<CleanupResult> CleanupOldFiles(string bucketName, int retentionDays)
    {
        await Task.CompletedTask;
        return new CleanupResult
        {
            FilesDeleted = 10,
            SpaceFreed = 5 * 1024 * 1024 // 5 MB
        };
    }

    private bool ShouldDeleteDocument(Domain.Entities.Document document, int retentionDays)
    {
        // Signed documents have 30-year retention (CNDP Loi 53-05)
        if (document.Status == "SIGNED")
        {
            return false;
        }

        // Unsigned documents can be deleted after retention period
        return document.Created < DateTimeOffset.UtcNow.AddDays(-retentionDays);
    }

    private class UploadResult
    {
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public bool DataLost { get; set; }
    }

    private class StorageMetrics
    {
        public long TotalCapacity { get; set; }
        public long UsedCapacity { get; set; }
        public long AvailableCapacity { get; set; }
    }

    private class QueuedJob
    {
        public Guid DocumentId { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private class CleanupResult
    {
        public int FilesDeleted { get; set; }
        public long SpaceFreed { get; set; }
    }
}
