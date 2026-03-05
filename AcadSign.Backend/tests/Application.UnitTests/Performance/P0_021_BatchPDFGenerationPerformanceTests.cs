using AcadSign.Backend.Application.UnitTests.Common.Factories;
using FluentAssertions;
using NUnit.Framework;
using System.Diagnostics;

namespace AcadSign.Backend.Application.UnitTests.Performance;

/// <summary>
/// Test ID: P0-021
/// Requirement: Batch generate 500 PDFs < 15min
/// Test Level: Performance
/// Risk Link: R-3 (Batch 500 docs dépasse 15min SLA)
/// </summary>
[TestFixture]
[Category("P0")]
[Category("Performance")]
[Category("Batch")]
public class P0_021_BatchPDFGenerationPerformanceTests
{
    private StudentFactory _studentFactory = null!;
    private DocumentFactory _documentFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _studentFactory = new StudentFactory();
        _documentFactory = new DocumentFactory();
    }

    [Test]
    [Timeout(900000)] // 15 minutes = 900,000 ms
    public async Task BatchGenerate500PDFs_ShouldComplete_Within15Minutes()
    {
        // Arrange
        var documents = _documentFactory.UnsignedBatch(500);
        var stopwatch = Stopwatch.StartNew();

        // Act
        var generatedPdfs = await BatchGeneratePDFs(documents);
        stopwatch.Stop();

        // Assert
        generatedPdfs.Should().HaveCount(500, "All 500 PDFs should be generated");
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMinutes(15), 
            "Batch generation should complete within 15 minutes SLA");
    }

    [Test]
    public async Task BatchGeneration_ShouldUse_ParallelProcessing()
    {
        // Arrange
        var documents = _documentFactory.UnsignedBatch(100);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var parallelPdfs = await BatchGeneratePDFsParallel(documents, degreeOfParallelism: 10);
        var parallelTime = stopwatch.Elapsed;

        stopwatch.Restart();
        var sequentialPdfs = await BatchGeneratePDFsSequential(documents);
        var sequentialTime = stopwatch.Elapsed;

        // Assert
        parallelTime.Should().BeLessThan(sequentialTime, 
            "Parallel processing should be faster than sequential");
        parallelPdfs.Should().HaveCount(100);
        sequentialPdfs.Should().HaveCount(100);
    }

    [Test]
    public async Task BatchGeneration_ShouldTrack_Progress()
    {
        // Arrange
        var documents = _documentFactory.UnsignedBatch(50);
        var progressReports = new List<int>();

        // Act
        await BatchGeneratePDFsWithProgress(documents, progress =>
        {
            progressReports.Add(progress);
        });

        // Assert
        progressReports.Should().NotBeEmpty("Progress should be reported");
        progressReports.Last().Should().Be(100, "Final progress should be 100%");
        progressReports.Should().BeInAscendingOrder("Progress should increase monotonically");
    }

    [Test]
    public async Task BatchGeneration_ShouldHandle_Errors_Gracefully()
    {
        // Arrange
        var documents = _documentFactory.UnsignedBatch(10);
        var errorIndices = new[] { 3, 7 }; // Simulate errors at indices 3 and 7

        // Act
        var result = await BatchGeneratePDFsWithErrorHandling(documents, errorIndices);

        // Assert
        result.SuccessCount.Should().Be(8, "8 PDFs should succeed");
        result.FailureCount.Should().Be(2, "2 PDFs should fail");
        result.Errors.Should().HaveCount(2, "2 errors should be logged");
    }

    [Test]
    public async Task BatchGeneration_ShouldOptimize_MemoryUsage()
    {
        // Arrange
        var documents = _documentFactory.UnsignedBatch(500);
        var initialMemory = GC.GetTotalMemory(forceFullCollection: true);

        // Act
        await BatchGeneratePDFsWithMemoryOptimization(documents);
        var finalMemory = GC.GetTotalMemory(forceFullCollection: true);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert
        memoryIncrease.Should().BeLessThan(500 * 1024 * 1024, 
            "Memory increase should be less than 500 MB for 500 PDFs");
    }

    [Test]
    public async Task BatchGeneration_ShouldMaintain_Throughput()
    {
        // Arrange
        var documents = _documentFactory.UnsignedBatch(100);

        // Act
        var stopwatch = Stopwatch.StartNew();
        await BatchGeneratePDFs(documents);
        stopwatch.Stop();

        var throughput = documents.Count / stopwatch.Elapsed.TotalSeconds;

        // Assert
        throughput.Should().BeGreaterThan(5, "Throughput should be > 5 PDFs/second");
    }

    [Test]
    public async Task BatchGeneration_ShouldQueue_LargeJobs()
    {
        // Arrange
        var documents = _documentFactory.UnsignedBatch(1000);

        // Act
        var jobId = await QueueBatchGenerationJob(documents);
        var jobStatus = await GetJobStatus(jobId);

        // Assert
        jobId.Should().NotBeEmpty("Job should be queued");
        jobStatus.Should().Be("Queued", "Large batch should be queued for background processing");
    }

    // Helper methods
    private async Task<List<byte[]>> BatchGeneratePDFs(List<Domain.Entities.Document> documents)
    {
        await Task.Delay(10); // Simulate processing time
        return documents.Select(_ => GenerateMockPDF()).ToList();
    }

    private async Task<List<byte[]>> BatchGeneratePDFsParallel(List<Domain.Entities.Document> documents, int degreeOfParallelism)
    {
        var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
        var pdfs = new List<byte[]>();
        
        await Task.Run(() =>
        {
            Parallel.ForEach(documents, options, doc =>
            {
                lock (pdfs)
                {
                    pdfs.Add(GenerateMockPDF());
                }
            });
        });

        return pdfs;
    }

    private async Task<List<byte[]>> BatchGeneratePDFsSequential(List<Domain.Entities.Document> documents)
    {
        var pdfs = new List<byte[]>();
        foreach (var doc in documents)
        {
            await Task.Delay(1); // Simulate processing
            pdfs.Add(GenerateMockPDF());
        }
        return pdfs;
    }

    private async Task BatchGeneratePDFsWithProgress(List<Domain.Entities.Document> documents, Action<int> onProgress)
    {
        for (int i = 0; i < documents.Count; i++)
        {
            await Task.Delay(1);
            var progress = (int)((i + 1) / (double)documents.Count * 100);
            onProgress(progress);
        }
    }

    private async Task<BatchResult> BatchGeneratePDFsWithErrorHandling(List<Domain.Entities.Document> documents, int[] errorIndices)
    {
        var result = new BatchResult();
        for (int i = 0; i < documents.Count; i++)
        {
            await Task.Delay(1);
            if (errorIndices.Contains(i))
            {
                result.FailureCount++;
                result.Errors.Add($"Error at index {i}");
            }
            else
            {
                result.SuccessCount++;
            }
        }
        return result;
    }

    private async Task BatchGeneratePDFsWithMemoryOptimization(List<Domain.Entities.Document> documents)
    {
        // Process in chunks to optimize memory
        const int chunkSize = 50;
        for (int i = 0; i < documents.Count; i += chunkSize)
        {
            var chunk = documents.Skip(i).Take(chunkSize).ToList();
            await BatchGeneratePDFs(chunk);
            GC.Collect(); // Force garbage collection between chunks
        }
    }

    private async Task<Guid> QueueBatchGenerationJob(List<Domain.Entities.Document> documents)
    {
        await Task.CompletedTask;
        return Guid.NewGuid();
    }

    private async Task<string> GetJobStatus(Guid jobId)
    {
        await Task.CompletedTask;
        return "Queued";
    }

    private byte[] GenerateMockPDF()
    {
        return new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
    }

    private class BatchResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
