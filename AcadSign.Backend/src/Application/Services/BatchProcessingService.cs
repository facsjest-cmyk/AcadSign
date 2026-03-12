using Hangfire;
using Microsoft.Extensions.Logging;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Domain.Enums;
using Microsoft.Extensions.Hosting;

namespace AcadSign.Backend.Application.Services;

public class BatchProcessingService
{
    private readonly IPdfGenerationService _pdfService;
    private readonly IS3StorageService _storageService;
    private readonly IBatchRepository _batchRepo;
    private readonly IDocumentRepository _documentRepo;
    private readonly IAuditLogService _auditService;
    private readonly IHostEnvironment _env;
    private readonly ILogger<BatchProcessingService> _logger;
    
    public BatchProcessingService(
        IPdfGenerationService pdfService,
        IS3StorageService storageService,
        IBatchRepository batchRepo,
        IDocumentRepository documentRepo,
        IAuditLogService auditService,
        IHostEnvironment env,
        ILogger<BatchProcessingService> logger)
    {
        _pdfService = pdfService;
        _storageService = storageService;
        _batchRepo = batchRepo;
        _documentRepo = documentRepo;
        _auditService = auditService;
        _env = env;
        _logger = logger;
    }
    
    [Queue("batch")]
    [AutomaticRetry(Attempts = 6, DelaysInSeconds = new[] { 0, 60, 300, 900, 3600, 21600 }, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task ProcessBatchAsync(Guid batchId, List<BatchDocumentRequest> documents)
    {
        _logger.LogInformation("Starting batch processing for {BatchId} with {Count} documents", 
            batchId, documents.Count);
        
        var batch = await _batchRepo.GetByIdAsync(batchId);
        if (batch == null)
        {
            throw new InvalidOperationException($"Batch {batchId} not found");
        }
        batch.Status = BatchStatus.PROCESSING;
        batch.StartedAt = DateTime.UtcNow;
        await _batchRepo.UpdateAsync(batch);

        foreach (var doc in documents)
        {
            try
            {
                await ProcessSingleDocumentAsync(batchId, doc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process document in batch {BatchId}", batchId);
                await _batchRepo.IncrementFailedCountAsync(batchId);
            }
        }
        
        batch = await _batchRepo.GetByIdAsync(batchId);
        if (batch == null)
        {
            throw new InvalidOperationException($"Batch {batchId} not found after processing");
        }
        batch.CompletedAt = DateTime.UtcNow;

        if (batch.ProcessedDocuments < batch.TotalDocuments)
        {
            batch.Status = BatchStatus.PROCESSING;
        }
        else if (batch.FailedDocuments == 0)
        {
            batch.Status = BatchStatus.COMPLETED;
        }
        else if (batch.FailedDocuments < batch.TotalDocuments)
        {
            batch.Status = BatchStatus.PARTIAL;
        }
        else
        {
            batch.Status = BatchStatus.FAILED;
        }
        
        await _batchRepo.UpdateAsync(batch);

        await _auditService.LogEventAsync(AuditEventType.BATCH_COMPLETED, null, new
        {
            batchId,
            status = batch.Status.ToString(),
            totalDocuments = batch.TotalDocuments,
            processedDocuments = batch.ProcessedDocuments,
            failedDocuments = batch.FailedDocuments
        });
        
        _logger.LogInformation("Batch {BatchId} completed: {Processed}/{Total} successful, {Failed} failed",
            batchId, batch.ProcessedDocuments - batch.FailedDocuments, batch.TotalDocuments, batch.FailedDocuments);
    }
    
    private async Task ProcessSingleDocumentAsync(Guid batchId, BatchDocumentRequest request)
    {
        var documentId = Guid.NewGuid();
        var devStudentId = Guid.Parse("d3b3c1a2-7c68-4c1f-9f25-0d2e4d2c5f3a");
        var batchDoc = await _batchRepo.GetBatchDocumentAsync(batchId, request.StudentId);
        if (batchDoc != null)
        {
            batchDoc.Status = DocumentStatus.Processing;
            await _batchRepo.UpdateBatchDocumentAsync(batchDoc);
        }
        
        try
        {
            var studentData = new StudentData
            {
                FirstNameFr = request.FirstName,
                LastNameFr = request.LastName,
                FirstNameAr = request.FirstName,
                LastNameAr = request.LastName,
                CIN = request.CIN,
                CNE = request.CNE,
                ProgramNameFr = request.ProgramName,
                ProgramNameAr = request.ProgramName,
                AcademicYear = request.AcademicYear,
                FacultyFr = string.Empty,
                FacultyAr = string.Empty,
                DocumentId = documentId
            };

            var pdfBytes = await _pdfService.GenerateDocumentAsync(request.DocumentType, studentData);
            var objectPath = await _storageService.UploadDocumentAsync(pdfBytes, documentId.ToString());

            var document = new Document
            {
                PublicId = documentId,
                DocumentType = request.DocumentType.ToString(),
                StudentId = devStudentId,
                Status = "UNSIGNED",
                S3ObjectPath = objectPath
            };

            await _documentRepo.CreateAsync(document);

            await _auditService.LogEventAsync(AuditEventType.DOCUMENT_GENERATED, documentId, new
            {
                batchId,
                documentType = request.DocumentType.ToString(),
                studentId = request.StudentId
            });

            await _auditService.LogEventAsync(AuditEventType.DOCUMENT_UPLOADED, documentId, new
            {
                batchId,
                s3ObjectPath = objectPath
            });

            if (batchDoc != null)
            {
                batchDoc.DocumentId = documentId;
                batchDoc.Status = DocumentStatus.Unsigned;
                batchDoc.ProcessedAt = DateTime.UtcNow;
                await _batchRepo.UpdateBatchDocumentAsync(batchDoc);
            }

            await _batchRepo.IncrementProcessedCountAsync(batchId);

            _logger.LogInformation("Document {DocumentId} generated successfully", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate document");

            if (batchDoc != null)
            {
                batchDoc.Status = DocumentStatus.Failed;
                batchDoc.ErrorMessage = ex.Message;
                batchDoc.ProcessedAt = DateTime.UtcNow;
                await _batchRepo.UpdateBatchDocumentAsync(batchDoc);
            }

            await _batchRepo.IncrementFailedCountAsync(batchId);
        }
    }
}
