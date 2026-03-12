using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hangfire;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Domain.Enums;
using AcadSign.Backend.Application.Services;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using System.Security.Claims;

namespace AcadSign.Backend.Web.Controllers;

[ApiController]
[Route("api/v1/documents")]
[Authorize(Roles = "API Client,Administrator")]
public class BatchController : ControllerBase
{
    private readonly IBatchRepository _batchRepo;
    private readonly IAuditLogService _auditService;
    private readonly ILogger<BatchController> _logger;
    
    public BatchController(
        IBatchRepository batchRepo,
        IAuditLogService auditService,
        ILogger<BatchController> logger)
    {
        _batchRepo = batchRepo;
        _auditService = auditService;
        _logger = logger;
    }
    
    [HttpGet("batch/{batchId}/status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBatchStatus(Guid batchId)
    {
        var batch = await _batchRepo.GetByIdAsync(batchId);
        
        if (batch == null)
        {
            return NotFound(new { error = "Batch not found" });
        }
        
        var estimatedCompletion = CalculateEstimatedCompletion(batch);
        
        return Ok(new BatchStatusResponse
        {
            BatchId = batch.Id,
            Status = batch.Status.ToString(),
            TotalDocuments = batch.TotalDocuments,
            ProcessedDocuments = batch.ProcessedDocuments,
            FailedDocuments = batch.FailedDocuments,
            SuccessfulDocuments = batch.ProcessedDocuments - batch.FailedDocuments,
            StartedAt = batch.StartedAt,
            CompletedAt = batch.CompletedAt,
            EstimatedCompletionAt = estimatedCompletion,
            ProgressPercentage = batch.TotalDocuments > 0 
                ? (double)batch.ProcessedDocuments / batch.TotalDocuments * 100 
                : 0,
            Documents = batch.Documents.Select(d => new DocumentStatusDto
            {
                DocumentId = d.DocumentId,
                StudentId = d.StudentId,
                Status = d.Status.ToString(),
                DownloadUrl = d.DocumentId.HasValue ? $"/api/v1/documents/{d.DocumentId}/download" : null,
                Error = d.ErrorMessage
            }).ToList()
        });
    }
    
    private DateTime? CalculateEstimatedCompletion(Batch batch)
    {
        if (batch.Status == BatchStatus.COMPLETED || batch.Status == BatchStatus.FAILED)
        {
            return batch.CompletedAt;
        }
        
        if (batch.ProcessedDocuments == 0 || !batch.StartedAt.HasValue)
        {
            return null;
        }
        
        var elapsed = DateTime.UtcNow - batch.StartedAt.Value;
        var avgTimePerDoc = elapsed.TotalSeconds / batch.ProcessedDocuments;
        var remainingDocs = batch.TotalDocuments - batch.ProcessedDocuments;
        var estimatedSeconds = avgTimePerDoc * remainingDocs;
        
        return DateTime.UtcNow.AddSeconds(estimatedSeconds);
    }
    
    [HttpPost("batch")]
    public async Task<IActionResult> CreateBatch([FromBody] CreateBatchRequest request)
    {
        if (request.Documents == null || request.Documents.Count == 0)
        {
            return BadRequest(new { error = "Documents list cannot be empty" });
        }
        
        if (request.Documents.Count > 500)
        {
            return BadRequest(new { error = "Maximum 500 documents per batch" });
        }
        
        var batchId = request.BatchId ?? Guid.NewGuid();
        var batch = new Batch
        {
            Id = batchId,
            TotalDocuments = request.Documents.Count,
            ProcessedDocuments = 0,
            FailedDocuments = 0,
            Status = BatchStatus.PENDING,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString()),
            Documents = request.Documents.Select(d => new BatchDocument
            {
                Id = Guid.NewGuid(),
                BatchId = batchId,
                StudentId = d.StudentId,
                Status = DocumentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };
        
        await _batchRepo.AddAsync(batch);

        await _auditService.LogEventAsync(AuditEventType.BATCH_CREATED, null, new
        {
            batchId = batchId,
            totalDocuments = request.Documents.Count
        });
        
        BackgroundJob.Enqueue<BatchProcessingService>(
            x => x.ProcessBatchAsync(batchId, request.Documents));
        
        _logger.LogInformation("Batch {BatchId} created with {Count} documents", batchId, request.Documents.Count);
        
        return Accepted(new CreateBatchResponse
        {
            BatchId = batchId,
            TotalDocuments = batch.TotalDocuments,
            Status = "PROCESSING",
            CreatedAt = batch.CreatedAt,
            StatusUrl = $"/api/v1/documents/batch/{batchId}/status"
        });
    }
}

public class CreateBatchRequest
{
    public Guid? BatchId { get; set; }
    public List<BatchDocumentRequest> Documents { get; set; } = new();
}

public class CreateBatchResponse
{
    public Guid BatchId { get; set; }
    public int TotalDocuments { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string StatusUrl { get; set; } = string.Empty;
}

public class BatchStatusResponse
{
    public Guid BatchId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalDocuments { get; set; }
    public int ProcessedDocuments { get; set; }
    public int FailedDocuments { get; set; }
    public int SuccessfulDocuments { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? EstimatedCompletionAt { get; set; }
    public double ProgressPercentage { get; set; }

    public List<DocumentStatusDto> Documents { get; set; } = new();
}

public class DocumentStatusDto
{
    public Guid? DocumentId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? DownloadUrl { get; set; }
    public string? Error { get; set; }
}
