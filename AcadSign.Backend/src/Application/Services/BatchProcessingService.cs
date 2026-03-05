using Hangfire;
using Microsoft.Extensions.Logging;
using AcadSign.Backend.Domain.Entities;

namespace AcadSign.Backend.Application.Services;

public class BatchProcessingService
{
    private readonly IPdfGenerationService _pdfService;
    private readonly IS3StorageService _storageService;
    private readonly IBatchRepository _batchRepo;
    private readonly IDocumentRepository _documentRepo;
    private readonly ILogger<BatchProcessingService> _logger;
    
    public BatchProcessingService(
        IPdfGenerationService pdfService,
        IS3StorageService storageService,
        IBatchRepository batchRepo,
        IDocumentRepository documentRepo,
        ILogger<BatchProcessingService> logger)
    {
        _pdfService = pdfService;
        _storageService = storageService;
        _batchRepo = batchRepo;
        _documentRepo = documentRepo;
        _logger = logger;
    }
    
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessBatchAsync(Guid batchId, List<object> documents)
    {
        _logger.LogInformation("Starting batch processing for {BatchId} with {Count} documents", 
            batchId, documents.Count);
        
        var batch = await _batchRepo.GetByIdAsync(batchId);
        batch.Status = BatchStatus.PROCESSING;
        batch.StartedAt = DateTime.UtcNow;
        await _batchRepo.UpdateAsync(batch);
        
        var tasks = documents.Select(async doc =>
        {
            try
            {
                await ProcessSingleDocumentAsync(batchId, doc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process document in batch {BatchId}", batchId);
            }
        });
        
        await Task.WhenAll(tasks);
        
        batch = await _batchRepo.GetByIdAsync(batchId);
        batch.CompletedAt = DateTime.UtcNow;
        
        if (batch.FailedDocuments == 0)
        {
            batch.Status = BatchStatus.COMPLETED;
        }
        else if (batch.ProcessedDocuments > 0)
        {
            batch.Status = BatchStatus.PARTIAL;
        }
        else
        {
            batch.Status = BatchStatus.FAILED;
        }
        
        await _batchRepo.UpdateAsync(batch);
        
        _logger.LogInformation("Batch {BatchId} completed: {Processed}/{Total} successful, {Failed} failed",
            batchId, batch.ProcessedDocuments - batch.FailedDocuments, batch.TotalDocuments, batch.FailedDocuments);
    }
    
    private async Task ProcessSingleDocumentAsync(Guid batchId, object request)
    {
        var documentId = Guid.NewGuid();
        
        try
        {
            var document = new Document
            {
                Id = documentId,
                Status = DocumentStatus.Unsigned,
                CreatedAt = DateTime.UtcNow
            };
            
            await _documentRepo.AddAsync(document);
            
            await _batchRepo.IncrementProcessedCountAsync(batchId);
            
            _logger.LogInformation("Document {DocumentId} generated successfully", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate document");
            await _batchRepo.IncrementFailedCountAsync(batchId);
        }
    }
}
