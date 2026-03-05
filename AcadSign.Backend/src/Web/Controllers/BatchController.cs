using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hangfire;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Application.Services;
using System.Security.Claims;

namespace AcadSign.Backend.Web.Controllers;

[ApiController]
[Route("api/v1/documents")]
[Authorize(Roles = "API Client,Admin")]
public class BatchController : ControllerBase
{
    private readonly IBatchRepository _batchRepo;
    private readonly ILogger<BatchController> _logger;
    
    public BatchController(
        IBatchRepository batchRepo,
        ILogger<BatchController> logger)
    {
        _batchRepo = batchRepo;
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
                : 0
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
    public List<DocumentGenerationRequest> Documents { get; set; } = new();
}

public class DocumentGenerationRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CIN { get; set; } = string.Empty;
    public string CNE { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
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
}
